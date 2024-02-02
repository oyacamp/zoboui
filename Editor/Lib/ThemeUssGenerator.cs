using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Text;
using ZoboUI.Core;
using ZoboUI.Core.Utils;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine;
using ZoboUI.Core.Plugins;
using ZoboUI.Editor.Attributes;
using System;

namespace ZoboUI.Editor
{

    /// <summary>
    /// This should be inherited by all utility configs or plugins that generate utility classes. 
    /// </summary>
    public interface IGenerateUtilityClassBag
    {
        /// <summary>
        /// This generates the class names and adds them to the bag. The key is the selector and the value is a dictionary that maps a USS property to the value of the USS property. e.g 'border-radius' : '4px', 'background-color' : '#FFF', etc.
        /// </summary>
        /// <param name="themeConfig"></param>
        /// <param name="prettyUtilityNameForLogging"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        void GenerateUtilityClassBagFromData(ThemeConfig themeConfig, UtilityRuleBag bag, string prettyUtilityNameForLogging, ICustomLogger logger = null);
    }



    /// <summary>
    /// Holds information about how to generate a class item along with the values to use for the USS properties.
    /// </summary>
    public class ClassItemGenerationInfo
    {
        public class ClassGenerationSettings
        {
            /// <summary>
            /// Whether to skip adding the dot prefix to the class name during generation. This could be useful if it has already been added in the selector.
            /// </summary>
            public bool SkipAddingDotPrefix { get; set; }


            public ClassGenerationSettings(bool skipAddingDotPrefix)
            {
                SkipAddingDotPrefix = skipAddingDotPrefix;
            }
        }

        public ClassGenerationSettings GenerationSettings { get; set; }



        public USSPropertyToValueDictionary USSPropertyToValueDictionary { get; set; }

        // Constructor
        public ClassItemGenerationInfo(USSPropertyToValueDictionary ussPropertyToValueDictionary, ClassGenerationSettings classGenerationSettings)
        {
            USSPropertyToValueDictionary = ussPropertyToValueDictionary;
            GenerationSettings = classGenerationSettings;
        }
    }

    /// <summary>
    /// A bag of utility classes rules that will be added to the generated USS file. The key is the selector to generate. The value is the USS property name and the value of the USS property.
    /// </summary>
    public class UtilityRuleBag : Dictionary<string, USSPropertyToValueDictionary>
    {



    }



    public class ThemeUssGenerator
    {
        /// <summary>
        /// If a key in a dictionary of values is set to this value, it will be used as the default value for the generated class. e.g if the key is 'DEFAULT' for one of the values in the utility for border radius, the generated class will be border, and not border-DEFAULT. It will be the value used when the class tag is used without a value. e.g 'border' instead of 'border-4'
        /// </summary>
        public readonly static string DEFAULT_VALUE_KEY_NAME = "DEFAULT";

        /// <summary>
        /// The template string in custom USS that will be replaced with the generated USS file data
        /// </summary>
        public static readonly string GENERATED_USS_STYLES_TEMPLATE_STRING = "{{generated_uss_styles}}";

        /// <summary>
        /// This is the separator used between the class tag and the key. e.g 'bg-[#FFF]' or 'hover_bg-[#FFF]' etc. It's also used between the class and the custom prefix if it is set. e.g 'su-bg-[#FFF]' or 'hover_su-bg-[#FFF]' etc.
        /// </summary>
        public readonly static string BASE_SEPARATOR = "-";
        private ICustomLogger customLogger;

        private ThemeConfigValidator themeConfigValidator;

        public class PurgeSettings
        {
            public UssPurger UssPurger { get; set; }
            public string RootDirectoryPath { get; set; }
        }

        public ThemeUssGenerator(ICustomLogger customLogger = null)
        {
            this.customLogger = customLogger == null ? new CustomLogger(prefix: "ThemeUssGenerator", logLevel: LogLevel.Warning) : customLogger;
            themeConfigValidator = new ThemeConfigValidator(this.customLogger);
        }

        public class RequiredUtilityConfigDataForPlugin
        {
            public IGenerateUtilityClassBag ClassGenerator { get; set; }
            public string UtilityName { get; set; }

            public RequiredUtilityConfigDataForPlugin(IGenerateUtilityClassBag classGenerator, string utilityName)
            {
                ClassGenerator = classGenerator;
                UtilityName = utilityName;
            }
        }

        /// <summary>
        /// Generates the class name from the class tag and the key. If the key is the default key, the class tag is returned. If the class tag is null or empty, the key is returned without the dash. This is useful for classes that don't need a class tag e.g. 'hidden' instead of 'display-hidden'. If the key is not the default key, the class tag and the key are returned.
        /// </summary>
        /// <param name="classTag"></param>
        /// <param name="key"></param>
        /// <param name="isDefaultKey"></param>
        /// <returns></returns>
        public static string GenerateClassName(string classTag, string key, bool isDefaultKey)
        {
            // If the key is the default key, return the class tag
            if (isDefaultKey)
            {
                return classTag;
            }

            // If the class tag is null or empty, return the key without the dash. This is useful for classes that don't need a class tag e.g. 'hidden' instead of 'display-hidden'
            if (string.IsNullOrEmpty(classTag))
            {
                return $"{key}";
            }

            // If the key is not the default key, return the class tag and the key
            return $"{classTag}{ThemeUssGenerator.BASE_SEPARATOR}{key}";
        }

        /// <summary>
        /// Merges a class tag with a custom prefix. e.g. if the class tag is 'bg', and the custom prefix is 'mycustomprefix-', the merged class tag will be 'mycustomprefix-bg'
        /// </summary>
        /// <param name="classTag"></param>
        /// <param name="customPrefix"></param>
        /// <returns></returns>
        public static string AddCustomPrefixToClassTag(string classTag, string customPrefix)
        {
            // If the custom prefix is empty, return the class tag
            if (string.IsNullOrEmpty(customPrefix))
            {
                return classTag;
            }

            // If the custom prefix is not empty, return the class tag and the custom prefix
            return $"{customPrefix}{classTag}";
        }


        private static List<RequiredUtilityConfigDataForPlugin> GetEnabledUtilityConfigs(ThemeConfig config)
        {
            List<RequiredUtilityConfigDataForPlugin> utilityConfigsDataList = new List<RequiredUtilityConfigDataForPlugin>();

            List<Tuple<int, RequiredUtilityConfigDataForPlugin>> utilitiesToAddBeforeDefaultUtilies = new List<Tuple<int, RequiredUtilityConfigDataForPlugin>>();
            List<Tuple<int, RequiredUtilityConfigDataForPlugin>> utilitiesToAddAfterDefaultUtilies = new List<Tuple<int, RequiredUtilityConfigDataForPlugin>>();


            // Use reflection to get all the fields in the config.utilties class
            FieldInfo[] fields = config.utilities.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            // Loop through every property in the config.utilties class
            foreach (var field in fields)
            {
                var value = field.GetValue(config.utilities);


                // If the value is null, skip it
                if (value == null)
                {
                    Debug.LogWarning($"The value of the field {field.Name} is null");
                    continue;
                }

                if (value is BaseUtilityConfig)
                {
                    BaseUtilityConfig baseUtilityConfig = (BaseUtilityConfig)value;
                    if (!baseUtilityConfig.enabled)
                    {
                        continue;
                    }
                    ClassTagToUssPropertyDictionary classTagToUssPropertyDictionary = baseUtilityConfig.tagPropertyMap;

                    RequiredUtilityConfigDataForPlugin requiredUtilityConfigDataForPlugin = new RequiredUtilityConfigDataForPlugin(baseUtilityConfig, field.Name);

                    // Check if the field has the UtilityGenerationOrderAttribute attribute
                    UtilityGenerationOrderAttribute utilityGenerationOrderAttribute = field.GetCustomAttribute<UtilityGenerationOrderAttribute>();

                    if (utilityGenerationOrderAttribute != null)
                    {
                        // If the attribute is not null, add it to the correct list
                        if (utilityGenerationOrderAttribute.Order == UtilityGenerationOrderAttribute.GenerationOrder.BeforeOtherUtilites)
                        {
                            utilitiesToAddBeforeDefaultUtilies.Add(new Tuple<int, RequiredUtilityConfigDataForPlugin>(utilityGenerationOrderAttribute.GenerationIndex, requiredUtilityConfigDataForPlugin));
                        }
                        else if (utilityGenerationOrderAttribute.Order == UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities)
                        {
                            utilitiesToAddAfterDefaultUtilies.Add(new Tuple<int, RequiredUtilityConfigDataForPlugin>(utilityGenerationOrderAttribute.GenerationIndex, requiredUtilityConfigDataForPlugin));
                        }

                        continue;
                    }

                    // If the value is a UtilityConfig, add it to the list if it doesn't have the UtilityGenerationOrderAttribute attribute
                    utilityConfigsDataList.Add(requiredUtilityConfigDataForPlugin);
                }

            }

            // Sort the list of utilities to add before the default utilities in the utilityConfigsDataList
            utilitiesToAddBeforeDefaultUtilies.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            utilityConfigsDataList.InsertRange(0, utilitiesToAddBeforeDefaultUtilies.ConvertAll(x => x.Item2));

            // Sort the list of utilities to add after the default utilities in the utilityConfigsDataList
            utilitiesToAddAfterDefaultUtilies.Sort((x, y) => x.Item1.CompareTo(y.Item1));

            utilityConfigsDataList.AddRange(utilitiesToAddAfterDefaultUtilies.ConvertAll(x => x.Item2));



            return utilityConfigsDataList;
        }


        public string GenerateUssRuleString(string selector, USSPropertyToValueDictionary ussPropertyToValueDictionary)
        {
            StringBuilder uss = new StringBuilder();


            // Add the class name to the uss string
            uss.AppendLine($"{selector} {{");

            // Loop through every USS property in the class
            foreach (var ussPair in ussPropertyToValueDictionary)
            {
                string ussPropertyName = ussPair.Key;
                string ussPropertyValue = ussPair.Value;

                // Add the USS property name and value to the uss string
                uss.AppendLine($"\t{ussPropertyName}: {ussPropertyValue};");
            }

            // Close the class
            uss.AppendLine("}");
            return uss.ToString();
        }



        protected UtilityRuleBag RemoveUnusedClasses(UtilityRuleBag bag, UssPurger ussPurger, string rootDirectoryPath)
        {
            UtilityRuleBag newBag = new UtilityRuleBag();

            var usedClassLikeStrings = ussPurger.GetUsedClassLikeStrings(rootDirectoryPath, true);

            // Loop through every class in the bag
            foreach (var pair in bag)
            {
                string selector = pair.Key;

                USSPropertyToValueDictionary ussPropertyToValueDictionary = pair.Value;

                bool selectorIsUsed = ussPurger.IsUsedSelector(selector, usedClassLikeStrings);

                // If the class name is not in the used class names, skip it
                if (!selectorIsUsed)
                {
                    continue;
                }

                // If the class name is in the used class names, add it to the new bag
                newBag.Add(pair.Key, ussPropertyToValueDictionary);
            }

            return newBag;
        }



        /// <summary>
        /// Merges the custom uss with the generated uss. The custom uss should contain the template string {{generated_uss_styles}}. The generated uss will be inserted in place of the template string.
        /// </summary>
        /// <param name="customUss"></param>
        /// <param name="generatedUss"></param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public string MergeCustomUssWithGeneratedUss(string customUss, string generatedUss, string templateString)
        {
            // Check how many times the template string appears in the custom uss
            int count = System.Text.RegularExpressions.Regex.Matches(customUss, templateString).Count;

            StringBuilder ussStringBuilder = new StringBuilder();

            // If the template string is not in the custom uss, then add the custom uss to the top of the generated uss. If the template string appears multiple times, throw an error
            if (count == 0)
            {
                ussStringBuilder.AppendLine(customUss);
                ussStringBuilder.AppendLine(generatedUss);
                return ussStringBuilder.ToString();
            }
            else if (count > 1)
            {
                throw new System.Exception(customLogger.FormatMessage($"The template string {templateString} appears multiple times in the custom uss. It should only appear once"));
            }


            // If the template string is in the custom uss, replace it with the generated uss
            string uss = customUss.Replace(templateString, generatedUss);

            return uss;
        }

        /// <summary>
        /// This adds any class extractors from the plugins to the uss purger
        /// </summary>
        /// <param name="ussPurger"></param>
        /// <param name="config"></param>
        protected void AddPluginExtractorsToUssPurger(UssPurger ussPurger, ThemeConfig config)
        {
            // Create a new list of extractors
            List<IClassExtractor> extractors = new List<IClassExtractor>();

            // Add the existing extractors to the list
            extractors.AddRange(ussPurger.ClassExtractors);

            // Loop through the plugins and add them to the correct list
            foreach (var pluginValueHolder in config.core.plugins.Values)
            {
                var assetPath = pluginValueHolder.assetPath;
                var plugin = AssetDatabase.LoadAssetAtPath<BaseUtilityPlugin>(assetPath);

                if (plugin == null)
                {
                    customLogger.LogWarning($"The plugin at path {assetPath} could not be loaded");
                    continue;
                }

                if (plugin is IUseCustomClassExtractor)
                {
                    IClassExtractor currentExtractor = plugin.GetClassExtractor();

                    if (currentExtractor != null)
                    {
                        extractors.Add(currentExtractor);
                    }
                }
            }

            // Set the extractors to the new list
            ussPurger.ClassExtractors = extractors;
        }


        /// <summary>
        /// This processes the class generators in the config including the plugins
        /// </summary>
        /// <param name="config"></param>
        /// <param name="bag"></param>
        /// <param name="logger"></param>
        protected void ProcessClassGenerators(ThemeConfig config, UtilityRuleBag bag, ICustomLogger logger)
        {
            // Loop through every property in the config.utilties class
            var requiredClassGenerationInfoList = GetEnabledUtilityConfigs(config);

            List<IProcessThemeConfig> pluginsToProcessAfterDefaultUtilityConfigs = new List<IProcessThemeConfig>();


            BaseUtilityPlugin LoadPluginFromConfig(PluginValueHolder pluginValueHolder)
            {
                var assetPath = pluginValueHolder.assetPath;
                var plugin = AssetDatabase.LoadAssetAtPath<BaseUtilityPlugin>(assetPath);

                if (plugin == null)
                {
                    logger.LogWarning($"The plugin at path {assetPath} could not be loaded");
                    return null;
                }

                return plugin;
            }

            // Loop through the plugins and add them to the correct list
            foreach (var pluginValueHolder in config.core.plugins.Values)
            {
                var plugin = LoadPluginFromConfig(pluginValueHolder);
                if (plugin != null && plugin is IProcessThemeConfig)
                {
                    IProcessThemeConfig processThemeConfigPlugin = plugin;

                    if (processThemeConfigPlugin.RuleGenerationPriority == RuleGenerationPriority.BeforeDefaultUtilities)
                    {
                        // Process the plugin before the default utility configs
                        plugin.ProcessThemeConfig(config, bag, logger);

                    }
                    else if (processThemeConfigPlugin.RuleGenerationPriority == RuleGenerationPriority.AfterDefaultUtilities)
                    {
                        pluginsToProcessAfterDefaultUtilityConfigs.Add(processThemeConfigPlugin);
                    }
                }
            }



            foreach (var item in requiredClassGenerationInfoList)
            {


                string prettyUtilityName = PropertyFormatter.FormatPropertyNameForDisplay(item.UtilityName);
                logger.Log($"Processing config for the '{prettyUtilityName}' utility");

                item.ClassGenerator.GenerateUtilityClassBagFromData(config, bag, prettyUtilityName, logger);

            }

            // Loop through the plugins and process them
            foreach (var plugin in pluginsToProcessAfterDefaultUtilityConfigs)
            {
                plugin.ProcessThemeConfig(config, bag, logger);
            }

        }

        /// <summary>
        /// This runs the OnPurgeComplete method for all the plugins that implement IProcessThemeConfig
        /// </summary>
        /// <param name="config"></param>
        /// <param name="bag"></param>
        /// <param name="logger"></param>
        protected void RunPurgeCompleteForPlugins(ThemeConfig config, UtilityRuleBag bag, ICustomLogger logger)
        {
            // Loop through the plugins and add them to the correct list
            foreach (var pluginValueHolder in config.core.plugins.Values)
            {
                var assetPath = pluginValueHolder.assetPath;
                var plugin = AssetDatabase.LoadAssetAtPath<BaseUtilityPlugin>(assetPath);

                if (plugin == null)
                {
                    logger.LogWarning($"The plugin at path {assetPath} could not be loaded");
                    continue;
                }

                if (plugin is IProcessThemeConfig)
                {
                    IProcessThemeConfig processThemeConfigPlugin = plugin;

                    processThemeConfigPlugin.OnPurgeComplete(config, bag, logger);
                }
            }
        }

        protected void ValidateThemeConfigForUssGeneration(ThemeConfig config)
        {
            if (config == null)
            {
                throw new System.Exception(customLogger.FormatMessage("The provided config is null"));
            }

            if (config.core == null)
            {
                throw new System.Exception(customLogger.FormatMessage("The provided config.core is null"));
            }

            if (config.utilities == null)
            {
                throw new System.Exception(customLogger.FormatMessage("The provided config.utilities is null"));
            }

            themeConfigValidator.ValidateThemeConfig(config);


        }

        /// <summary>
        /// Generates the USS file from the provided ThemeConfig.
        /// </summary>
        /// <param name="config"></param>
        public string GenerateUssFileContentFromThemeConfig(ThemeConfig config, PurgeSettings purgeSettings = null)
        {
            ValidateThemeConfigForUssGeneration(config);


            UtilityRuleBag bag = new UtilityRuleBag();

            // Process the class generators
            ProcessClassGenerators(config, bag, customLogger);


            // If the uss purger is not null, remove unused classes
            if (purgeSettings != null)
            {
                AddPluginExtractorsToUssPurger(purgeSettings.UssPurger, config);
                bag = RemoveUnusedClasses(bag, purgeSettings.UssPurger, purgeSettings.RootDirectoryPath);
                RunPurgeCompleteForPlugins(config, bag, customLogger);
            }

            // Generate the USS file
            string uss = GenerateUssFileContentFromBag(bag);

            if (!string.IsNullOrEmpty(config.core.customUss))
            {
                // Merge the custom uss with the generated uss
                uss = MergeCustomUssWithGeneratedUss(config.core.customUss, uss, ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING);
            }

            return uss;
        }

        /// <summary>
        /// Generates the USS file content from the provided UtilityClassBag.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        public string GenerateUssFileContentFromBag(UtilityRuleBag bag)
        {
            StringBuilder uss = new StringBuilder();

            // Loop through every class in the bag
            foreach (var pair in bag)
            {
                string ruleText = GenerateUssRuleString(pair.Key, pair.Value);

                // Add the class to the uss string
                uss.AppendLine(ruleText);
            }

            return uss.ToString();
        }


    }
}
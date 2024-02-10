using System;
using System.Collections.Generic;
using ZoboUI.Core.Utils;
using ZoboUI.Editor;
using ZoboUI.Editor.Attributes;
using ZoboUI.Editor.Utils;
using UnityEngine;
using Newtonsoft.Json;

namespace ZoboUI.Core
{

    /// <summary>
    /// This should be inherited by config values that also support additional USS properties. e.g you have a string value for the font size, but you also want to add a USS property that sets the line height at the same time.
    /// </summary>
    [System.Serializable]
    public class ValueHolderWithUssDictionary
    {
        public USSPropertyToValueDictionary uss;

    }






    [Serializable]
    public class ImageValueHolder : ValueHolderWithUssDictionary
    {
        /// <summary>
        /// This is the path to the image asset.
        /// </summary>
        public string value;
    }


    /// <summary>
    /// This is the final value that every IConfigValue should return. It contains the key value pairs and the uss dictionary.
    /// </summary>
    [Serializable]
    public class ConfigValueResultItem
    {

        /// <summary>
        /// The key is the string that will be appened to the the class tag eg the "red" part of "bg-red" or the "sm" part of "text-sm".
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of the USS property that will be set depending on the given property name in a utility. "#FFF" or  "24px"
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Any additional USS properties that should be set for the class. e.g. if the class is 'text-sm', and we want to set the line height as well, the uss property name is 'line-height', and the value is '24px'
        /// </summary>
        public USSPropertyToValueDictionary UssDictionary { get; set; }
    }



    // VALUE TYPES
    public interface IConfigValue
    {



        /// <summary>
        /// The data required for the config value to generate the USS classes.
        /// </summary>
        /// <returns></returns>
        public List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration();

        /// <summary>
        /// Whether the other config value is the same type as this config value. This helps prevent errors when extending other config fields. e.g. if you accidentially use the values from the 'spacing' config field for the 'color' config field, the values will be ignored.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameValueType(IConfigValue other);


    }

    [System.Serializable]
    public class StringKeyValueHolder : ValueHolderWithUssDictionary
    {
        public string value;
    }

    [System.Serializable]
    public class FontAssetValueHolder : ValueHolderWithUssDictionary
    {
        public string value;
    }

    [System.Serializable]
    public class ColorValueHolder : ValueHolderWithUssDictionary
    {
        public SwatchDictionary value;

    }

    [System.Serializable]
    public class ModifierValueHolder
    {


        /// <summary>
        /// The value of the modifier that will be used to generate the class name. e.g. if the modifier is 'hover', this would be "{{generated_class}}:hover"
        /// </summary>
        public string value;

    }

    [System.Serializable]
    public abstract class SimpleConfigValueDictionary<TValue> : Dictionary<string, TValue>, IConfigValue
    {
        public abstract List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration();

        public abstract bool IsSameValueType(IConfigValue other);
    }

    [System.Serializable]
    public abstract class DictionaryWithUssValueHolder<TValue> : Dictionary<string, TValue>, IConfigValue
    {
        public abstract List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration();

        public abstract bool IsSameValueType(IConfigValue other);
    }








    [System.Serializable]
    public abstract class BaseUtilityConfig : IGenerateUtilityClassBag
    {
        /// <summary>
        /// Whether the plugin utility is enabled or not.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// The types of modifer variations to generate for this utility. E.g ['hover', 'focus', 'active'] would generate classes like 'hover_bg-red', 'focus:bg-red', etc.
        /// </summary>
        public StringListValue modifierVariations = new StringListValue();

        /// <summary>
        /// Whether to include the values from other config fields.  This is useful for creating utilities that use the same values as other utilities. e.g. if you want to use the values from the 'spacing' config field for the 'margin' config field, you would set this to ['spacing']. If you wanted to extend another field, you would do the add the field name to the array. Keep in mind that the order of the array matters. The values will be applied in the order they are listed in the array so if you want to override a value, make sure to list it after the field you want to override.
        /// </summary>
        public StringListValue extendFields = new StringListValue();



        /// <summary>
        /// A dictionary mapping class tags to a list of USS property names. e.g 'rounded' : ["border-radius"] or 'rounded-t' : ["border-top-left-radius", "border-top-right-radius"] , etc. The key is the class tag, and the value is a list of USS property names to generate using the values in the provided value type's dictionary.
        /// </summary>
        public ClassTagToUssPropertyDictionary tagPropertyMap = new ClassTagToUssPropertyDictionary();

        /// <summary>
        /// The data required for the config value to generate the USS classes.
        /// </summary>
        /// <returns></returns>
        protected abstract IConfigValue GetConfigValueData();






        protected StringDropdownFieldValueListConverter<ClassModifierStringDropdownFieldValue> stringAsDropdownListConverterForClassModifier = new StringDropdownFieldValueListConverter<ClassModifierStringDropdownFieldValue>();
        protected StringDropdownFieldValueListConverter<ExtendFieldsStringDropdownFieldValue> stringAsDropdownListConverterForFieldExtensions = new StringDropdownFieldValueListConverter<ExtendFieldsStringDropdownFieldValue>();

        public BaseUtilityConfig InitializeFromUtilityConfigDisplay(UtilityConfigDisplay configDisplay, ICustomLogger logger = null)
        {
            var classTagToUssPropertyMapConverter = new ClassTagToUssPropertyMapConverter();

            enabled = configDisplay.Enabled;
            modifierVariations = stringAsDropdownListConverterForClassModifier.ConvertFromDisplay(configDisplay.ModifierVariations);
            extendFields = stringAsDropdownListConverterForFieldExtensions.ConvertFromDisplay(configDisplay.ExtendFields);
            tagPropertyMap = classTagToUssPropertyMapConverter.ConvertFromDisplay(configDisplay.ClassTagToUssPropertyMap);

            return this;
        }

        public UtilityConfigDisplay InitializeUtilityConfigDisplayFromUtilityConfig(ICustomLogger logger = null)
        {
            var classTagToUssPropertyMapConverter = new ClassTagToUssPropertyMapConverter();

            UtilityConfigDisplay configDisplay = new UtilityConfigDisplay();

            configDisplay.Enabled = enabled;
            configDisplay.ModifierVariations = stringAsDropdownListConverterForClassModifier.ConvertToDisplay(modifierVariations);
            configDisplay.ExtendFields = stringAsDropdownListConverterForFieldExtensions.ConvertToDisplay(extendFields);
            configDisplay.ClassTagToUssPropertyMap = classTagToUssPropertyMapConverter.ConvertToDisplay(tagPropertyMap);

            return configDisplay;
        }

        public abstract void GenerateUtilityClassBagFromData(ThemeConfig themeConfig, UtilityRuleBag bag, string prettyUtilityNameForLogging, ICustomLogger logger = null);
    }





    [System.Serializable]
    public abstract class UtilityConfig<T> : BaseUtilityConfig, IConvertibleToBaseUtilityConfig, IConvertibleToUtilityConfigDisplay where T : IConfigValue
    {
        public T data;

        public virtual BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger = null)
        {
            return this.InitializeFromUtilityConfigDisplay(configDisplay, logger);

        }

        public abstract UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger = null);

        protected override IConfigValue GetConfigValueData()
        {
            return data;
        }

        /// <summary>
        /// This takes the field name in the config e.g core.spacing , and returns the value of the field in the config using reflection. This is useful for getting the value of a custom property type or dictionary.
        /// </summary>
        /// <param name="propertyPathKey"></param>
        /// <returns></returns>
        protected T GetConfigValueForExtendedField(string propertyPathKey, ThemeConfig themeConfig)
        {
            T configValue = SerializedPropertyHelper.GetValueFromSerializedObject<T>(themeConfig, propertyPathKey);

            return configValue;
        }



        protected string GenerateSelectorFromBaseClassName(string className)
        {
            // If it already starts with a dot, return it
            if (className.StartsWith("."))
            {
                return className;
            }

            return $".{className}";
        }

        /// <summary>
        /// This should be implemented by the utility config to generate the class names (without the dot prefix) for the given key. e.g. if the class tag is 'rounded', the key is 'sm', the generated class name will be 'rounded-sm'.
        /// </summary>
        /// <param name="classTag"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual string GenerateClassName(string classTag, string key)
        {
            string className = ThemeUssGenerator.GenerateClassName(classTag, key, key.Equals(ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME));
            return className;
        }

        protected abstract T GenerateMergedDataFromExtendedData(List<T> extendedData);

        /// <summary>
        /// This should be implemented by the utility config to generate the class names (without the dot prefix) and the USS property names and values for that class name.
        /// </summary>
        /// <returns></returns>
        protected virtual List<Tuple<string, USSPropertyToValueDictionary>> GenerateClassNamesAndUSSPropertyDictionaryFromData(string classTag, List<string> ussPropertyNamesToSet, T mergedData)
        {
            List<Tuple<string, USSPropertyToValueDictionary>> classNamesAndUSSPropertyDictionary = new List<Tuple<string, USSPropertyToValueDictionary>>();


            List<ConfigValueResultItem> requiredConfigData = mergedData.GetRequiredConfigValuesForUssGeneration();



            foreach (var pair in requiredConfigData)
            {
                string key = pair.Key;
                string value = pair.Value;


                string className = this.GenerateClassName(classTag, key);


                USSPropertyToValueDictionary ussPropertyToValueDictionary = new USSPropertyToValueDictionary();

                // For each property name, add the value to the uss property dictionary
                foreach (string propertyName in ussPropertyNamesToSet)
                {
                    // Get the value of the property
                    string propertyValue = value;

                    // If the value is null or empty, skip it
                    if (string.IsNullOrEmpty(propertyValue))
                    {
                        continue;
                    }

                    // Add the value to the uss property dictionary
                    ussPropertyToValueDictionary[propertyName] = propertyValue;
                }

                // Add the items in the additional uss field to the uss property dictionary if any
                if (pair.UssDictionary != null)
                {
                    foreach (var ussPair in pair.UssDictionary)
                    {
                        string ussPropertyName = ussPair.Key;
                        string ussPropertyValue = ussPair.Value;

                        // If the value is null or empty, skip it
                        if (string.IsNullOrEmpty(ussPropertyValue))
                        {
                            continue;
                        }

                        // Add the value to the uss property dictionary
                        ussPropertyToValueDictionary[ussPropertyName] = ussPropertyValue;
                    }
                }


                if (ussPropertyToValueDictionary.Count == 0) continue;


                // Add the class name and the uss property dictionary to the list
                classNamesAndUSSPropertyDictionary.Add(new Tuple<string, USSPropertyToValueDictionary>(className, ussPropertyToValueDictionary));
            }

            return classNamesAndUSSPropertyDictionary;
        }



        public override void GenerateUtilityClassBagFromData(ThemeConfig themeConfig, UtilityRuleBag bag, string prettyUtilityNameForLogging, ICustomLogger logger = null)
        {

            logger = logger == null ? new CustomLogger(prefix: "BaseUtilityPlugin", logLevel: LogLevel.Warning) : logger;




            // Loop through the ClassTagToUssPropertyDictionary and generate the class names and the USS property names and values
            foreach (var pair in this.tagPropertyMap)
            {
                // We add the custom prefix to the class tag if it is provided
                string ClassTag = ThemeUssGenerator.AddCustomPrefixToClassTag(pair.Key, themeConfig.core.prefix);
                var PropertyNames = pair.Value;
                // If the USS property name is empty, return
                if (string.IsNullOrEmpty(ClassTag))
                {
                    logger.Log($"No class tag for key in '{prettyUtilityNameForLogging}' utility. If this is intentional, you can ignore this message");
                }

                // If the USS property value is empty, return
                if (PropertyNames == null)
                {
                    throw new System.Exception(logger.FormatMessage($"No USS property list provided for the key - {ClassTag} in the utility config - {prettyUtilityNameForLogging}"));

                }

                List<T> extendedDataList = new List<T>();

                // Loop through the property names and get the values from the config
                foreach (var fieldPath in this.extendFields)
                {
                    // If the USS property name is empty, return
                    if (string.IsNullOrEmpty(fieldPath))
                    {
                        logger.LogWarning($"No field path provided for item in extendFields for the utility config - {prettyUtilityNameForLogging}");
                    }

                    // Get the value of the property from the config
                    T extendedFieldData = GetConfigValueForExtendedField(fieldPath, themeConfig);

                    // Add the value to the list
                    extendedDataList.Add(extendedFieldData);
                }

                T mergedData = GenerateMergedDataFromExtendedData(extendedDataList);

                List<Tuple<string, USSPropertyToValueDictionary>> classNamesAndUSSPropertyDictionary = GenerateClassNamesAndUSSPropertyDictionaryFromData(ClassTag, pair.Value.properties, mergedData);



                // Add the items in the baseUtilityClassBag and the USS Dictionary to the bag
                foreach (var basePair in classNamesAndUSSPropertyDictionary)
                {
                    string selectorToStore = GenerateSelectorFromBaseClassName(basePair.Item1);
                    USSPropertyToValueDictionary ussToValueDict = basePair.Item2;


                    // Add the class name and the USS Dictionary to the bag
                    bag[selectorToStore] = ussToValueDict;
                }


                // IF there are class variations (e.g hover, focus, checked), generate the class names and the USS property names and values
                if (this.modifierVariations.Count > 0)
                {


                    string modifierSeparator = themeConfig.core.modifierSeparator;





                    // Generate modifiers for the class tag e.g 'hover_bg-red' or 'focus_bg-red' etc.
                    foreach (var modifierName in this.modifierVariations)
                    {
                        if (!themeConfig.core.modifiers.ContainsKey(modifierName))
                        {
                            logger.LogWarning($"No modifier value provided for item ${modifierName} in modifierVariations for utility config - {prettyUtilityNameForLogging}. You might want to add it to the modifiers dictionary in the core properties. ");
                            continue;
                        }

                        ModifierValueHolder modifierValueHolder = themeConfig.core.modifiers[modifierName];

                        if (modifierValueHolder?.value == null)
                        {
                            logger.LogWarning($"No modifier value provided for item ${modifierName} in classVariations for utility config - {prettyUtilityNameForLogging}. ");
                            continue;
                        }

                        // Modifiers add the dot prefix and custom prefix by default

                        // Loop through the property names and get the values from the config
                        foreach (var classPair in classNamesAndUSSPropertyDictionary)
                        {

                            string classNameWithoutDotPrefix = classPair.Item1;
                            // Add the modifier to the class name
                            string selectorWithModifier = ClassModifierDictionary.FormatClassNameWithModifier(classNameWithoutDotPrefix, modifierSeparator, modifierName, modifierValueHolder);


                            // Add the selector and the USS Dictionary to the bag
                            bag[selectorWithModifier] = classPair.Item2;
                        }

                    }
                }


            }

        }


    }

    [System.Serializable]
    public class UtilityConfigWithStringDictionary : UtilityConfig<KeyValueDictionary>
    {
        public override BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger)
        {
            base.ConvertToBaseUtilityConfig(configDisplay, logger);
            var stringValueDictionaryConverter = new StringKeyValueDictionaryConverter();
            BaseCorePropertyValueDisplay display = configDisplay.PropertyDisplay;

            if (display == null)
            {
                throw new Exception($"The config display property display is not of type {nameof(BaseCorePropertyValueDisplay)}");
            }
            var stringValueDisplay = stringValueDictionaryConverter.ConvertFromDisplay(display.ValuesKeyValueDictionary);
            this.data = stringValueDisplay;
            return this;
        }

        public override UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger)
        {
            UtilityConfigDisplay configDisplay = base.InitializeUtilityConfigDisplayFromUtilityConfig();

            var stringValueDictionaryConverter = new StringKeyValueDictionaryConverter(logger);
            configDisplay.PropertyDisplay = this.data.ConvertToBaseCorePropertyValueDisplay("Values");

            return configDisplay;
        }




        protected override KeyValueDictionary GenerateMergedDataFromExtendedData(List<KeyValueDictionary> extendedData)
        {
            KeyValueDictionary mergedData = new KeyValueDictionary();

            // Add the values from the extended fields first
            foreach (var item in extendedData)
            {
                foreach (var pair in item)
                {
                    string key = pair.Key;
                    StringKeyValueHolder value = pair.Value;

                    mergedData[key] = value;
                }
            }

            // Add the values from the current field after the extended field so that they override the extended fields if they have the same key
            foreach (var item in this.data)
            {
                string key = item.Key;
                StringKeyValueHolder value = item.Value;

                mergedData[key] = value;
            }

            return mergedData;
        }
    }

    [System.Serializable]
    public class UtilityConfigWithStringImageDictionary : UtilityConfig<StringImageDictionary>
    {
        public override BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger)
        {
            base.ConvertToBaseUtilityConfig(configDisplay, logger);
            var imageValueDictionaryConverter = new ImageValueDictionaryConverter(logger);
            BaseCorePropertyValueDisplay display = configDisplay.PropertyDisplay;

            if (display == null)
            {
                throw new Exception($"The config display property display is not of type {nameof(BaseCorePropertyValueDisplay)}");
            }

            var imageValueDictionaryDisplay = imageValueDictionaryConverter.ConvertFromDisplay(display.ValuesImageValueDictionary);
            this.data = imageValueDictionaryDisplay;
            return this;
        }

        public override UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger)
        {
            UtilityConfigDisplay configDisplay = base.InitializeUtilityConfigDisplayFromUtilityConfig();

            var imageValueDictionaryConverter = new ImageValueDictionaryConverter(logger);
            configDisplay.PropertyDisplay = this.data.ConvertToBaseCorePropertyValueDisplay("Values");

            return configDisplay;
        }

        protected override StringImageDictionary GenerateMergedDataFromExtendedData(List<StringImageDictionary> extendedData)
        {
            StringImageDictionary mergedData = new StringImageDictionary();

            // Add the values from the extended fields first
            foreach (var item in extendedData)
            {
                foreach (var pair in item)
                {
                    string key = pair.Key;
                    ImageValueHolder value = pair.Value;

                    mergedData[key] = value;
                }
            }

            // Add the values from the current field after the extended field so that they override the extended fields if they have the same key
            foreach (var item in this.data)
            {
                string key = item.Key;
                ImageValueHolder value = item.Value;

                mergedData[key] = value;
            }

            return mergedData;
        }
    }

    [System.Serializable]
    public class UtilityConfigWithStringFontAssetDictionary : UtilityConfig<FontAssetDictionary>
    {
        public override BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger)
        {
            base.ConvertToBaseUtilityConfig(configDisplay, logger);
            var fontAssetValueDictionaryConverter = new FontAssetValueDictionaryConverter(logger);
            BaseCorePropertyValueDisplay display = configDisplay.PropertyDisplay;

            if (display == null)
            {
                throw new Exception($"The config display property display is not of type {nameof(BaseCorePropertyValueDisplay)}");
            }

            var fontAssetValueDictionaryDisplay = fontAssetValueDictionaryConverter.ConvertFromDisplay(display.ValuesFontAssetValueDictionary);
            this.data = fontAssetValueDictionaryDisplay;
            return this;
        }

        public override UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger)
        {
            UtilityConfigDisplay configDisplay = base.InitializeUtilityConfigDisplayFromUtilityConfig();

            var fontAssetValueDictionaryConverter = new FontAssetValueDictionaryConverter(logger);
            configDisplay.PropertyDisplay = this.data.ConvertToBaseCorePropertyValueDisplay("Values");

            return configDisplay;
        }

        protected override FontAssetDictionary GenerateMergedDataFromExtendedData(List<FontAssetDictionary> extendedData)
        {
            FontAssetDictionary mergedData = new FontAssetDictionary();

            // Add the values from the extended fields first
            foreach (var item in extendedData)
            {
                foreach (var pair in item)
                {
                    string key = pair.Key;
                    FontAssetValueHolder value = pair.Value;

                    mergedData[key] = value;
                }
            }

            // Add the values from the current field after the extended field so that they override the extended fields if they have the same key
            foreach (var item in this.data)
            {
                string key = item.Key;
                FontAssetValueHolder value = item.Value;

                mergedData[key] = value;
            }

            return mergedData;
        }
    }


    [System.Serializable]
    public class UtilityConfigWithColorPalette : UtilityConfig<ColorPaletteDictionary>
    {
        public override BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger)
        {
            base.ConvertToBaseUtilityConfig(configDisplay, logger);
            var colorPaletteConverter = new ColorPaletteConverter(logger);

            BaseCorePropertyValueDisplay display = configDisplay.PropertyDisplay;

            if (display == null)
            {
                throw new Exception($"The config display property display is not of type {nameof(BaseCorePropertyValueDisplay)}");
            }

            var colorPaletteDisplay = colorPaletteConverter.ConvertFromDisplay(display.ValuesColorPalette);
            this.data = colorPaletteDisplay;
            return this;
        }

        public override UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger)
        {
            UtilityConfigDisplay configDisplay = base.InitializeUtilityConfigDisplayFromUtilityConfig();

            var colorPaletteConverter = new ColorPaletteConverter(logger);
            configDisplay.PropertyDisplay = this.data.ConvertToBaseCorePropertyValueDisplay("Values");

            return configDisplay;
        }

        protected override ColorPaletteDictionary GenerateMergedDataFromExtendedData(List<ColorPaletteDictionary> extendedData)
        {
            ColorPaletteDictionary mergedData = new ColorPaletteDictionary();

            // Add the values from the extended fields first
            foreach (var item in extendedData)
            {
                foreach (var pair in item)
                {
                    string key = pair.Key;
                    ColorValueHolder value = pair.Value;

                    mergedData[key] = value;
                }
            }

            // Add the values from the current field after the extended field so that they override the extended fields if they have the same key
            foreach (var item in this.data)
            {
                string key = item.Key;
                ColorValueHolder value = item.Value;

                mergedData[key] = value;
            }

            return mergedData;
        }
    }


    /// <summary>
    /// We need to override the GenerateUtilityClassBagFromData method for the space utility because we need to add "> *" to the end of the class names
    /// </summary>
    [System.Serializable]
    public class SpaceUtilityConfig : UtilityConfigWithStringDictionary
    {
        protected override string GenerateClassName(string classTag, string key)
        {
            string className = base.GenerateClassName(classTag, key);

            // Add "> *" to the end of the class name
            return $"{className} > *";
        }


    }



    /// <summary>
    /// The core properties for the theme. These are usually shared across utilities.
    /// </summary>
    [System.Serializable]
    public class BaseCoreProperties
    {

        /// <summary>
        /// The prefix for the classes generated by the theme. Usefull for namespacing your theme.
        /// </summary>
        [TextFieldAttribute(isMultiline: false, isRequired: false)]
        [Tooltip("The prefix for the classes generated by the theme. Useful for namespacing your theme. e.g. if you set this to 'myprefix-', the class name for the 'bg-red-500' class would be 'myprefix-bg-red-500'.")]
        public string prefix = String.Empty;

        [RequiredStringValueValidatorAttribute]
        [TextFieldAttribute(isMultiline: false, isRequired: true)]
        [Tooltip("The separator between modifiers like hover, focus, etc. and the class name. e.g. if you set this to '_', the separator for the 'bg-red-500' class would be 'hover_bg-red-500'.")]
        public string modifierSeparator = "_";

        [Tooltip("This allows you to customize the class modifiers in your project. e.g. hover, focus, etc.")]
        public ClassModifierDictionary modifiers = new DefaultClassModifierDictionary();

        [HideUssDictionaryInDisplayAttribute]
        [CanBeExtendedAttribute]
        [Tooltip("The colors for the theme. This is used to generate the color utilities and will be used to generate the color palette for different properties unless overridden.")]
        public ColorPaletteDictionary colors = new DefaultColorPaletteDictionary();

        /// <summary>
        /// The spacing scale for the theme. This is used to generate the spacing utilities.
        /// </summary>
        [HideUssDictionaryInDisplayAttribute]
        [CanBeExtendedAttribute]
        [Tooltip("The spacing scale for the theme. This is used to generate the spacing and length utilities.")]
        public KeyValueDictionary spacing = new DefaultSpacingDictionary();

        /// <summary>
        /// The negative spacing scale for the theme. This is used to generate the negative spacing utilities e.g. -m-1, -m-2, -m-3, etc.
        /// </summary>
        [HideUssDictionaryInDisplayAttribute]
        [CanBeExtendedAttribute]
        [Tooltip("The negative spacing scale for the theme. This is used to generate the negative spacing utilities")]
        public KeyValueDictionary negativeSpacing = new DefaultSpacingDictionary(isNegative: true);


        [Tooltip("These are custom or third party plugins that you want to use when generating USS files. If you want to use a specific plugin, you would add it here.")]
        public PluginDictionary plugins = new PluginDictionary();

        [CustomDisplayNameAttribute("Custom USS")]
        [TextFieldAttribute(isMultiline: true)]
        [Tooltip("These are custom USS styles that will be added to top of the uss file by default. These styles will not be purged. This is especially useful if you use a bunch of custom USS variables in your utilities and want to initialize them here. ")]
        public string customUss = String.Empty;


    }


    [System.Serializable]
    public class CompilationConfig
    {
        [Tooltip("These are the paths, globs, or file patterns for the files we want to reference for used classes. During compilation, only the classes that are used in these paths will be included in the uss file.")]
        [TextFieldAttribute(isMultiline: false, isRequired: true)]
        public StringListValue content = new StringListValue(){
             "Assets/*.cs",
      "Assets/*.uxml"
        };

        [TextFieldAttribute(isMultiline: false, isRequired: true)]
        [Tooltip("Use this to indicate specific classes that you want to include in the uss file. This is useful for classes that are not referenced in the project but you want to include them in the uss file. e.g. if you have a class that is only used in a prefab, you can add it here to make sure it is included in the uss file.")]
        public StringListValue safelist = new StringListValue();

        [TextFieldAttribute(isMultiline: false, isRequired: true)]
        [Tooltip("Use this to indicate specific classes that you want to exclude from the uss file. This is useful for classes that are referenced in the project but you want to exclude them from the uss file. e.g. if you have a class that is only used in a prefab, you can add it here to make sure it is not included in the uss file.")]
        public StringListValue blocklist = new StringListValue();
    }

    /// <summary>
    /// The utility properties for the theme. These are used to create the classes
    /// </summary>
    [System.Serializable]
    public class UtilityProperties
    {
        [Tooltip("Values for utilities controlling how rows are positioned in multi-row flex and grid containers.")]
        public UtilityConfigWithStringDictionary alignContent = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "start", new StringKeyValueHolder(){ value = "flex-start" } },
                { "center", new StringKeyValueHolder(){ value = "center" } },
                { "end", new StringKeyValueHolder(){ value = "flex-end"} },
                { "stretch", new StringKeyValueHolder(){ value = "stretch" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "content", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "align-content" } } }
            }
        };


        [Tooltip("Values for utilities controlling how flex and grid items are positioned along a container's cross axis.")]
        public UtilityConfigWithStringDictionary alignItems = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "start", new StringKeyValueHolder(){ value = "flex-start" } },
                { "center", new StringKeyValueHolder(){ value = "center" } },
                { "end", new StringKeyValueHolder(){ value = "flex-end"} },
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                { "stretch", new StringKeyValueHolder(){ value = "stretch" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "items", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "align-items" } } }
            }
        };


        [Tooltip("Values for utilities controlling how an individual flex or grid item is positioned along its container's cross axis.")]
        public UtilityConfigWithStringDictionary alignSelf = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "start", new StringKeyValueHolder(){ value = "flex-start" } },
                { "center", new StringKeyValueHolder(){ value = "center" } },
                { "end", new StringKeyValueHolder(){ value = "flex-end"} },
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                { "stretch", new StringKeyValueHolder(){ value = "stretch" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "self", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "align-self" } } }
            }
        };

        [Tooltip("Values for utilities controlling the background color of an element.")]
        public UtilityConfigWithColorPalette backgroundColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
                "checked"
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "background-color" } } }
            },
            data = new ColorPaletteDictionary()
        };

        [Tooltip("Values for utilities controlling the background image of an element.")]
        public UtilityConfigWithStringImageDictionary backgroundImage = new UtilityConfigWithStringImageDictionary()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "background-image" } } }
            },
            data = new StringImageDictionary(){
                { "none", new ImageValueHolder(){ value = "none" } }
            }

        };

        [Tooltip("Values for utilities controlling the background image tint color of an element.")]
        public UtilityConfigWithColorPalette backgroundImageTintColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg-tint", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-background-image-tint-color" } } }
            },
            data = new ColorPaletteDictionary()
        };

        [Tooltip("Values for utilities controlling the position of an element's background image.")]
        public UtilityConfigWithStringDictionary backgroundPosition = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "bottom", new StringKeyValueHolder(){ value = "bottom" } },
                { "center", new StringKeyValueHolder(){ value = "center" } },
                { "left", new StringKeyValueHolder(){ value = "left"} },
                { "left-bottom", new StringKeyValueHolder(){ value = "left bottom" } },
                { "left-top", new StringKeyValueHolder(){ value = "left top" } },
                { "right", new StringKeyValueHolder(){ value = "right" } },
                { "right-bottom", new StringKeyValueHolder(){ value = "right bottom" } },
                { "right-top", new StringKeyValueHolder(){ value = "right top" } },
                { "top", new StringKeyValueHolder(){ value = "top" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "background-position" } } }
            }

        };

        [Tooltip("Values for utilities controlling the repeat of an element's background image.")]
        public UtilityConfigWithStringDictionary backgroundRepeat = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "repeat", new StringKeyValueHolder(){ value = "repeat" } },
                { "no-repeat", new StringKeyValueHolder(){ value = "no-repeat" } },
                { "repeat-x", new StringKeyValueHolder(){ value = "repeat-x"} },
                { "repeat-y", new StringKeyValueHolder(){ value = "repeat-y" } },
                 { "repeat-space", new StringKeyValueHolder(){ value = "space" } },
                { "repeat-round", new StringKeyValueHolder(){ value = "round" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "background-repeat" } } }
            }
        };

        [Tooltip("Values for utilities controlling the background image scaling in the element’s box.")]
        public UtilityConfigWithStringDictionary backgroundScaleMode = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "stretch", new StringKeyValueHolder(){ value = "stretch-to-fill" } },
                { "crop", new StringKeyValueHolder(){ value = "scale-and-crop" } },
                { "fit", new StringKeyValueHolder(){ value = "scale-to-fit"} },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg-scale", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-background-scale-mode"} } }
            }

        };

        [Tooltip("Values for utilities controlling the background size of an element's background image.")]
        public UtilityConfigWithStringDictionary backgroundSize = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                { "cover", new StringKeyValueHolder(){ value = "cover" } },
                { "contain", new StringKeyValueHolder(){ value = "contain"} },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "bg", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "background-size" } } }
            }

        };

        [Tooltip("Values for utilities controlling the border color of an element.")]
        public UtilityConfigWithColorPalette borderColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
                "checked"
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "border", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-color" } } },
                 { "border-t", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-color" } } },
                { "border-r", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-right-color" } } },
                { "border-b", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-bottom-color" } } },
                { "border-l", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-left-color" } } }
            },
            data = new ColorPaletteDictionary()
        };

        [Tooltip("Values for utilities controlling the border radius of an element.")]
        public UtilityConfigWithStringDictionary borderRadius = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "none", new StringKeyValueHolder(){ value = "0" } },
                { "sm", new StringKeyValueHolder(){ value = "2px" } },
                { ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME  , new StringKeyValueHolder(){ value = "4px" } },
                { "md", new StringKeyValueHolder(){ value = "6px" } },
                { "lg", new StringKeyValueHolder(){ value = "8px" } },
                { "xl", new StringKeyValueHolder(){ value = "12px" } },
                { "2xl", new StringKeyValueHolder(){ value = "16px" } },
                { "3xl", new StringKeyValueHolder(){ value = "24px" } },
                { "full", new StringKeyValueHolder(){ value = "9999px" } }

            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "rounded", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-radius" } } },
                { "rounded-t", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-left-radius", "border-top-right-radius" } } },
                { "rounded-r", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-right-radius", "border-bottom-right-radius" } } },
                { "rounded-b", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-bottom-right-radius", "border-bottom-left-radius" } } },
                { "rounded-l", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-left-radius", "border-bottom-left-radius" } } },
                { "rounded-tl", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-left-radius" } } },
                { "rounded-tr", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-right-radius" } } },
                { "rounded-br", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-bottom-right-radius" } } },
                { "rounded-bl", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-bottom-left-radius" } } }
            }
        };

        [Tooltip("Values for utilities controlling the border width of an element.")]
        public UtilityConfigWithStringDictionary borderWidth = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
                "checked"
            },
            data = new KeyValueDictionary()
            {
                { "0", new StringKeyValueHolder(){ value = "0px" } },
                { "2", new StringKeyValueHolder(){ value = "2px" } },
                { "4", new StringKeyValueHolder(){ value = "4px" } },
                { "8", new StringKeyValueHolder(){ value = "8px" } },
                { ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME, new StringKeyValueHolder(){ value = "1px" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary()
            {
                { "border", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-width" } } },
                { "border-t", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-top-width" } } },
                { "border-r", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-right-width" } } },
                { "border-b", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-bottom-width" } } },
                { "border-l", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "border-left-width" } } }
            }
        };

        [Tooltip("Values for utilities controlling the cursor style when hovering over an element.")]
        public UtilityConfigWithStringDictionary cursor = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary()
            {
                { "arrow", new StringKeyValueHolder(){ value = "arrow"} },
                { "text", new StringKeyValueHolder(){ value = "text"} },
                { "resize-vertical", new StringKeyValueHolder(){ value = "resize-vertical" } },
                { "resize-horizontal", new StringKeyValueHolder(){ value = "resize-horizontal"} },
                { "link", new StringKeyValueHolder(){ value = "link"} },
                { "slide-arrow", new StringKeyValueHolder(){ value = "slide-arrow" } },
                { "resize-up-right", new StringKeyValueHolder(){ value = "resize-up-right"} },
                { "resize-up-left", new StringKeyValueHolder(){ value = "resize-up-left", } },
                { "move-arrow", new StringKeyValueHolder(){ value = "move-arrow" } },
                { "rotate-arrow", new StringKeyValueHolder(){ value = "rotate-arrow" } },
                { "scale-arrow", new StringKeyValueHolder(){ value = "scale-arrow"} },
                { "arrow-plus", new StringKeyValueHolder(){ value = "arrow-plus"} },
                { "arrow-minus", new StringKeyValueHolder(){ value = "arrow-minus"} },
                { "pan", new StringKeyValueHolder(){ value = "pan"} },
                { "orbit", new StringKeyValueHolder(){ value = "orbit"} },
                { "zoom", new StringKeyValueHolder(){ value = "zoom"} },
                { "fps", new StringKeyValueHolder(){ value = "fps"} },
                { "split-resize-up-down", new StringKeyValueHolder(){ value = "split-resize-up-down"} },
                { "split-resize-left-right", new StringKeyValueHolder(){ value = "split-resize-left-right" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary()
            {
                { "cursor", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "cursor" } } }
            }
        };

        [Tooltip("Values for utilities controlling the cursor image.")]
        public UtilityConfigWithStringImageDictionary cursorImage = new UtilityConfigWithStringImageDictionary()
        {
            enabled = true,
            modifierVariations = new StringListValue()
            {
            },
            data = new StringImageDictionary(),
            tagPropertyMap = new ClassTagToUssPropertyDictionary()
            {
                { "cursor-image", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "cursor" } } }
            },

        };

        [Tooltip("Values for utilities controlling the display of an element.")]
        public UtilityConfigWithStringDictionary display = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "flex", new StringKeyValueHolder(){ value = "flex" } },
                { "hidden", new StringKeyValueHolder(){ value = "none" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "display" } } }
            }
        };

        [Tooltip("Values for utilities controlling how flex items both grow and shrink.")]
        public UtilityConfigWithStringDictionary flex = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "1", new StringKeyValueHolder(){ value = "1 1 0%"} },
                { "auto", new StringKeyValueHolder(){ value = "1 1 auto" } },
                { "initial", new StringKeyValueHolder(){ value =  "0 1 auto" } },
                { "none", new StringKeyValueHolder(){ value = "none" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex" } } }
            }
        };

        [Tooltip("Values for utilities controlling how flex items shrink.")]
        public UtilityConfigWithStringDictionary flexBasis = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                { "1of2", new StringKeyValueHolder(){ value = "50%" } },
                { "1of3", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "2of3", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "1of4", new StringKeyValueHolder(){ value = "25%" } },
                { "2of4", new StringKeyValueHolder(){ value = "50%" } },
                { "3of4", new StringKeyValueHolder(){ value = "75%" } },
                { "1of5", new StringKeyValueHolder(){ value = "20%" } },
                { "2of5", new StringKeyValueHolder(){ value = "40%" } },
                { "3of5", new StringKeyValueHolder(){ value = "60%" } },
                { "4of5", new StringKeyValueHolder(){ value = "80%" } },
                { "1of6", new StringKeyValueHolder(){ value = "16.666667%" } },
                { "2of6", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "3of6", new StringKeyValueHolder(){ value = "50%" } },
                { "4of6", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "5of6", new StringKeyValueHolder(){ value = "83.333333%" } },
                { "1of12", new StringKeyValueHolder(){ value = "8.333333%" } },
                { "2of12", new StringKeyValueHolder(){ value = "16.666667%" } },
                { "3of12", new StringKeyValueHolder(){ value = "25%" } },
                { "4of12", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "5of12", new StringKeyValueHolder(){ value = "41.666667%" } },
                { "6of12", new StringKeyValueHolder(){ value = "50%" } },
                { "7of12", new StringKeyValueHolder(){ value = "58.333333%" } },
                { "8of12", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "9of12", new StringKeyValueHolder(){ value = "75%" } },
                { "10of12", new StringKeyValueHolder(){ value = "83.333333%" } },
                { "11of12", new StringKeyValueHolder(){ value = "91.666667%" } },
                { "full", new StringKeyValueHolder(){ value = "100%" } }
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex-basis", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex-basis" } } }
            }

        };

        [Tooltip("Values for utilities controlling the direction of flex items.")]
        public UtilityConfigWithStringDictionary flexDirection = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "row", new StringKeyValueHolder(){ value = "row" } },
                { "row-reverse", new StringKeyValueHolder(){ value = "row-reverse" } },
                { "col", new StringKeyValueHolder(){ value = "column" } },
                { "col-reverse", new StringKeyValueHolder(){ value = "column-reverse" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex-direction" } } }
            }

        };

        [Tooltip("Values for utilities controlling how flex items grow.")]
        public UtilityConfigWithStringDictionary flexGrow = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "0", new StringKeyValueHolder(){ value = "0" } },
                { ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME, new StringKeyValueHolder(){ value = "1" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex-grow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex-grow" } } }
            }
        };

        [Tooltip("Values for utilities controlling the flex shrink of an element.")]
        public UtilityConfigWithStringDictionary flexShrink = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "0", new StringKeyValueHolder(){ value = "0" } },
                { ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME, new StringKeyValueHolder(){ value = "1" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex-shrink", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex-shrink" } } }
            }

        };

        [Tooltip("Values for utilities controlling the wrapping of flex items.")]
        public UtilityConfigWithStringDictionary flexWrap = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "wrap", new StringKeyValueHolder(){ value = "wrap" } },
                { "wrap-reverse", new StringKeyValueHolder(){ value = "wrap-reverse" } },
                { "nowrap", new StringKeyValueHolder(){ value = "nowrap" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "flex", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "flex-wrap" } } }
            }

        };

        [Tooltip("Values for utilities controlling the font family of an element.")]
        public UtilityConfigWithStringFontAssetDictionary fontDefinition = new UtilityConfigWithStringFontAssetDictionary()
        {
            enabled = true,
            data = new FontAssetDictionary(),
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "font", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-font-definition" } } }
            }

        };

        [Tooltip("Values for utilities setting the height of an element.")]
        public UtilityConfigWithStringDictionary fontSize = new UtilityConfigWithStringDictionary()
        {

            enabled = true,
            data = new KeyValueDictionary(){
                { "xs", new StringKeyValueHolder(){ value = "12px" } },
                { "sm", new StringKeyValueHolder(){ value = "14px" } },
                { "base", new StringKeyValueHolder(){ value = "16px" } },
                { "lg", new StringKeyValueHolder(){ value = "18px" } },
                { "xl", new StringKeyValueHolder(){ value = "20px" } },
                { "2xl", new StringKeyValueHolder(){ value = "24px" } },
                { "3xl", new StringKeyValueHolder(){ value = "30px" } },
                { "4xl", new StringKeyValueHolder(){ value = "36px" } },
                { "5xl", new StringKeyValueHolder(){ value = "48px" } },
                { "6xl", new StringKeyValueHolder(){ value = "60px" } },
                { "7xl", new StringKeyValueHolder(){ value = "72px" } },
                { "8xl", new StringKeyValueHolder(){ value = "96px" } },
                { "9xl", new StringKeyValueHolder(){ value = "128px" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "font-size" } } }
            }
        };

        [Tooltip("Values for utilities controlling the font style of an element.")]
        public UtilityConfigWithStringDictionary fontStyle = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "italic", new StringKeyValueHolder(){ value = "italic" } },
                { "normal", new StringKeyValueHolder(){ value = "normal" } },
                { "bold", new StringKeyValueHolder(){ value = "bold" } },
                { "bold-italic", new StringKeyValueHolder(){ value = "bold-and-italic" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-font-style" } } }
            }

        };

        [Tooltip("Values for utilities for setting the height of an element.")]
        public UtilityConfigWithStringDictionary height = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "1of2", new StringKeyValueHolder(){ value = "50%" } },
                { "1of3", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "2of3", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "1of4", new StringKeyValueHolder(){ value = "25%" } },
                { "2of4", new StringKeyValueHolder(){ value = "50%" } },
                { "3of4", new StringKeyValueHolder(){ value = "75%" } },
                { "1of5", new StringKeyValueHolder(){ value = "20%" } },
                { "2of5", new StringKeyValueHolder(){ value = "40%" } },
                { "3of5", new StringKeyValueHolder(){ value = "60%" } },
                { "4of5", new StringKeyValueHolder(){ value = "80%" } },
                { "1of6", new StringKeyValueHolder(){ value = "16.666667%" } },
                { "2of6", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "3of6", new StringKeyValueHolder(){ value = "50%" } },
                { "4of6", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "5of6", new StringKeyValueHolder(){ value = "83.333333%" } },
                { "1of12", new StringKeyValueHolder(){ value = "8.333333%" } },
                { "2of12", new StringKeyValueHolder(){ value = "16.666667%" } },
                { "3of12", new StringKeyValueHolder(){ value = "25%" } },
                { "4of12", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "5of12", new StringKeyValueHolder(){ value = "41.666667%" } },
                { "6of12", new StringKeyValueHolder(){ value = "50%" } },
                { "7of12", new StringKeyValueHolder(){ value = "58.333333%" } },
                { "8of12", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "9of12", new StringKeyValueHolder(){ value = "75%" } },
                { "10of12", new StringKeyValueHolder(){ value = "83.333333%" } },
                { "11of12", new StringKeyValueHolder(){ value = "91.666667%" } },
                { "full", new StringKeyValueHolder(){ value = "100%" } }
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "h", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "height" } } }
            }

        };

        [Tooltip("Values for utility classes that control the source of an image component.")]
        public UtilityConfigWithStringImageDictionary image = new UtilityConfigWithStringImageDictionary()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "image", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--unity-image"} } }
            },
            data = new StringImageDictionary()
            {
            }

        };

        [Tooltip("Values for utilities controlling the image scaling in the image element's box.")]
        public UtilityConfigWithStringDictionary imageScaleMode = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "stretch", new StringKeyValueHolder(){ value = "stretch-to-fill" } },
                { "crop", new StringKeyValueHolder(){ value = "scale-and-crop" } },
                { "fit", new StringKeyValueHolder(){ value = "scale-to-fit"} },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "image-scale", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--unity-image-size"} } }
            }

        };

        [Tooltip("Values for utilities controlling the image tint color of an element.")]
        public UtilityConfigWithColorPalette imageTintColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            modifierVariations = new StringListValue(){
                "hover",
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "image-tint", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--unity-image-tint-color" } } }
            },
            data = new ColorPaletteDictionary()
        };

        [Tooltip("Values for utilities controlling the placement of positioned elements.")]
        public UtilityConfigWithStringDictionary inset = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                { "1of2", new StringKeyValueHolder(){ value = "50%" } },
                { "1of3", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "2of3", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "1of4", new StringKeyValueHolder(){ value = "25%" } },
                { "2of4", new StringKeyValueHolder(){ value = "50%" } },
                { "3of4", new StringKeyValueHolder(){ value = "75%" } },
                { "1of5", new StringKeyValueHolder(){ value = "20%" } },
                { "2of5", new StringKeyValueHolder(){ value = "40%" } },
                { "3of5", new StringKeyValueHolder(){ value = "60%" } },
                { "4of5", new StringKeyValueHolder(){ value = "80%" } },
                { "1of6", new StringKeyValueHolder(){ value = "16.666667%" } },
                { "2of6", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "3of6", new StringKeyValueHolder(){ value = "50%" } },
                { "4of6", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "5of6", new StringKeyValueHolder(){ value = "83.333333%" } },
                { "full", new StringKeyValueHolder(){ value = "100%" } },
                { "minus-1of2", new StringKeyValueHolder(){ value = "-50%" } },
                { "minus-1of3", new StringKeyValueHolder(){ value = "-33.333333%" } },
                { "minus-2of3", new StringKeyValueHolder(){ value = "-66.666667%" } },
                { "minus-1of4", new StringKeyValueHolder(){ value = "-25%" } },
                { "minus-2of4", new StringKeyValueHolder(){ value = "-50%" } },
                { "minus-3of4", new StringKeyValueHolder(){ value = "-75%" } },
                { "minus-full", new StringKeyValueHolder(){ value = "-100%" } }
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core),
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.negativeSpacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "inset", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "top", "right", "bottom", "left" } } },
                { "inset-x", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "left", "right" } } },
                { "inset-y", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "top", "bottom" } } },
                { "top", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "top" } } },
                { "right", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "right" } } },
                { "bottom", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "bottom" } } },
                { "left", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "left" } } }
            }

        };

        [Tooltip("Values for utilities controlling how items are justified within a container.")]
        public UtilityConfigWithStringDictionary justifyContent = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "start", new StringKeyValueHolder(){ value = "flex-start" } },
                { "center", new StringKeyValueHolder(){ value = "center" } },
                { "end", new StringKeyValueHolder(){ value = "flex-end"} },
                { "between", new StringKeyValueHolder(){ value = "space-between" } },
                { "around", new StringKeyValueHolder(){ value = "space-around" } },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "justify", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "justify-content" } } }
            }
        };

        [Tooltip("Values for utilities controlling the tracking (letter spacing) of an element.")]
        public UtilityConfigWithStringDictionary letterSpacing = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "tighter", new StringKeyValueHolder(){ value = "-5px" } },
                { "tight", new StringKeyValueHolder(){ value = "-2.5px" } },
                { "normal", new StringKeyValueHolder(){ value = "0" } },
                { "wide", new StringKeyValueHolder(){ value = "2.5px"} },
                { "wider", new StringKeyValueHolder(){ value = "5px" } },
                { "widest", new StringKeyValueHolder(){ value = "10px" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "tracking", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "letter-spacing" } } }
            }

        };

        [Tooltip("Values for utilities controlling the margin of an element.")]
        public UtilityConfigWithStringDictionary margin = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "auto", new StringKeyValueHolder(){ value = "auto" } },
                },
            extendFields = new StringListValue(){
                    PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core),
                    PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.negativeSpacing), PropertyFormatter.PropertyExtensionContext.Core)
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                    { "m", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin" } } },
                    { "mx", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-left", "margin-right" } } },
                    { "my", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-top", "margin-bottom" } } },
                    { "mt", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-top" } } },
                    { "mr", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-right" } } },
                    { "mb", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-bottom" } } },
                    { "ml", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-left" } } }
                }
        };

        [Tooltip("Values for utilities setting the maximum height of an element.")]
        public UtilityConfigWithStringDictionary maxHeight = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "full", new StringKeyValueHolder(){ value = "100%" } },
                { "none", new StringKeyValueHolder(){ value = "none" } }
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "max-h", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "max-height" } } }
            }

        };

        [Tooltip("Values for utilities setting the maximum width of an element.")]
        public UtilityConfigWithStringDictionary maxWidth = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            data = new KeyValueDictionary(){
                    { "none", new StringKeyValueHolder(){ value = "none" } },
                    { "xs", new StringKeyValueHolder(){ value = "320px" } },
                    { "sm", new StringKeyValueHolder(){ value = "384px" } },
                    { "md", new StringKeyValueHolder(){ value = "448px" } },
                    { "lg", new StringKeyValueHolder(){ value = "512px" } },
                    { "xl", new StringKeyValueHolder(){ value = "576px" } },
                    { "2xl", new StringKeyValueHolder(){ value = "672px" } },
                    { "3xl", new StringKeyValueHolder(){ value = "768px" } },
                    { "4xl", new StringKeyValueHolder(){ value = "896px" } },
                    { "5xl", new StringKeyValueHolder(){ value = "1024px" } },
                    { "6xl", new StringKeyValueHolder(){ value = "1152px" } },
                    { "7xl", new StringKeyValueHolder(){ value = "1280px" } },
                    { "full", new StringKeyValueHolder(){ value = "100%" } },
                    { "screen-sm", new StringKeyValueHolder(){ value = "640px" } },
                    { "screen-md", new StringKeyValueHolder(){ value = "768px" } },
                    { "screen-lg", new StringKeyValueHolder(){ value = "1024px" } },
                    { "screen-xl", new StringKeyValueHolder(){ value = "1280px" } },
                    { "screen-2xl", new StringKeyValueHolder(){ value = "1536px" } }
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "max-w", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "max-width" } } }
            }


        };

        [Tooltip("Values for utilities setting the minimum height of an element.")]
        public UtilityConfigWithStringDictionary minHeight = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            data = new KeyValueDictionary(){
                { "full", new StringKeyValueHolder(){ value = "100%" } },
                { "auto", new StringKeyValueHolder(){ value = "auto" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "min-h", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "min-height" } } }
            }
        };

        [Tooltip("Values for utilities setting the minimum width of an element.")]
        public UtilityConfigWithStringDictionary minWidth = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            data = new KeyValueDictionary(){
                { "full", new StringKeyValueHolder(){ value = "100%" } },
                { "auto", new StringKeyValueHolder(){ value = "auto" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "min-w", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "min-width" } } }
            }

        };

        [Tooltip("Values for utilities controlling the opacity of an element.")]
        public UtilityConfigWithStringDictionary opacity = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    { "0", new StringKeyValueHolder(){ value = "0" } },
                    { "5", new StringKeyValueHolder(){ value = "0.05"} },
                    { "10", new StringKeyValueHolder(){ value = "0.1" } },
                    { "20", new StringKeyValueHolder(){ value = "0.2"} },
                    { "25", new StringKeyValueHolder(){ value = "0.25"} },
                    { "30", new StringKeyValueHolder(){ value = "0.3"} },
                    { "40", new StringKeyValueHolder(){ value = "0.4"} },
                    { "50", new StringKeyValueHolder(){ value = "0.5"} },
                    { "60", new StringKeyValueHolder(){ value = "0.6"} },
                    { "70", new StringKeyValueHolder(){ value = "0.7"} },
                    { "75", new StringKeyValueHolder(){ value = "0.75"} },
                    { "80", new StringKeyValueHolder(){ value = "0.8"} },
                    { "90", new StringKeyValueHolder(){ value = "0.9"} },
                    { "95", new StringKeyValueHolder(){ value = "0.95"} },
                    { "100", new StringKeyValueHolder(){ value = "1" } }
                },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "opacity", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "opacity" } } }
            }
        };

        [Tooltip("Values for utilities controlling the overflow of an element.")]
        public UtilityConfigWithStringDictionary overflow = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "hidden", new StringKeyValueHolder(){ value = "hidden" } },
                { "visible", new StringKeyValueHolder(){ value = "visible" } },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "overflow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "overflow" } } }
            }

        };

        [Tooltip("Values for defining the clipping rectangle for the element content.")]
        public UtilityConfigWithStringDictionary overflowClipBox = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "padding", new StringKeyValueHolder(){ value = "padding-box" } },
                { "content", new StringKeyValueHolder(){ value = "content-box" } },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "overflow-clip", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-overflow-clip-box" } } }
            }
        };

        [Tooltip("Values for utilities controlling the padding of an element.")]
        public UtilityConfigWithStringDictionary padding = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "p", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding" } } },
                { "px", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-left", "padding-right" } } },
                { "py", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-top", "padding-bottom" } } },
                { "pt", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-top" } } },
                { "pr", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-right" } } },
                { "pb", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-bottom" } } },
                { "pl", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "padding-left" } } }
            }
        };

        [Tooltip("Values for utilities controlling the paragraph spacing of an element.")]
        public UtilityConfigWithStringDictionary paragraphSpacing = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "paragraph-spacing", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-paragraph-spacing" } } }
            }
        };

        [Tooltip("Values for utilities controlling the position of an element.")]
        public UtilityConfigWithStringDictionary position = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "absolute", new StringKeyValueHolder(){ value = "absolute" } },
                { "relative", new StringKeyValueHolder(){ value = "relative" } },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "position", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "position" } } }
            }

        };

        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 100)]
        [Tooltip("Values for utilities that rotate elements with transform.")]
        public UtilityConfigWithStringDictionary rotate = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    { "0", new StringKeyValueHolder(){ value = "0" } },
                    { "1", new StringKeyValueHolder(){ value = "1deg" } },
                    { "2", new StringKeyValueHolder(){ value = "2deg" } },
                    { "3", new StringKeyValueHolder(){ value = "3deg" } },
                    { "6", new StringKeyValueHolder(){ value = "6deg" } },
                    { "12", new StringKeyValueHolder(){ value = "12deg" } },
                    { "45", new StringKeyValueHolder(){ value = "45deg" } },
                    { "90", new StringKeyValueHolder(){ value = "90deg" } },
                    { "180", new StringKeyValueHolder(){ value = "180deg" } },
                    { "none", new StringKeyValueHolder(){ value = "none" } },
                    { "minus-1", new StringKeyValueHolder(){ value = "-1deg" } },
                    { "minus-2", new StringKeyValueHolder(){ value = "-2deg" } },
                    { "minus-3", new StringKeyValueHolder(){ value = "-3deg" } },
                    { "minus-6", new StringKeyValueHolder(){ value = "-6deg" } },
                    { "minus-12", new StringKeyValueHolder(){ value = "-12deg" } },
                    { "minus-45", new StringKeyValueHolder(){ value = "-45deg" } },
                    { "minus-90", new StringKeyValueHolder(){ value = "-90deg" } },
                    { "minus-180", new StringKeyValueHolder(){ value = "-180deg" } }
                },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "rotate", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-rotate" } } }
            }
        };

        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 101)]
        [Tooltip("Values for utilities that scale elements with transform.")]
        public UtilityConfigWithStringDictionary scale = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                        { "0", new StringKeyValueHolder(){ value = "0" } },
                        { "50", new StringKeyValueHolder(){ value = "0.5" } },
                        { "75", new StringKeyValueHolder(){ value = "0.75" } },
                        { "90", new StringKeyValueHolder(){ value = "0.9" } },
                        { "95", new StringKeyValueHolder(){ value = "0.95" } },
                        { "100", new StringKeyValueHolder(){ value = "1" } },
                        { "105", new StringKeyValueHolder(){ value = "1.05" } },
                        { "110", new StringKeyValueHolder(){ value = "1.1" } },
                        { "125", new StringKeyValueHolder(){ value = "1.25" } },
                        { "150", new StringKeyValueHolder(){ value = "1.5" } },
                        { "none", new StringKeyValueHolder(){ value = "none" } }
                    },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                    { "scale", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-scale-x", "--zb-scale-y" } } },
                    { "scale-x", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-scale-x" } } },
                    { "scale-y", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-scale-y" } } }
                }
        };

        [Tooltip("Values for utilities that control space between and around child elements.")]
        public SpaceUtilityConfig space = new SpaceUtilityConfig()
        {
            enabled = true,
            data = new KeyValueDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core),
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.negativeSpacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "space-x", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-left", "margin-right" } } },
                { "space-y", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "margin-top", "margin-bottom" } } },
            }
        };


        [Tooltip("Values for utilities that control the text alignment of an element.")]
        public UtilityConfigWithStringDictionary textAlign = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    {"upper-left", new StringKeyValueHolder(){ value = "upper-left" } },
                    { "left", new StringKeyValueHolder(){ value = "middle-left" } },
                    { "lower-left", new StringKeyValueHolder(){ value = "lower-left" } },
                    { "upper-center", new StringKeyValueHolder(){ value = "upper-center" } },
                    { "center", new StringKeyValueHolder(){ value = "middle-center" } },
                    { "lower-center", new StringKeyValueHolder(){ value = "lower-center" } },
                    { "upper-right", new StringKeyValueHolder(){ value = "upper-right" } },
                    { "right", new StringKeyValueHolder(){ value = "middle-right" } },
                    { "lower-right", new StringKeyValueHolder(){ value = "lower-right" } }
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-text-align" } } }
            }
        };

        [Tooltip("Values for utilities that control the text color of an element.")]
        public UtilityConfigWithColorPalette textColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            data = new ColorPaletteDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "color" } } }
            }

        };

        [Tooltip("Values for utilities that control the text outline width of an element.")]
        public UtilityConfigWithStringDictionary textOutlineWidth = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "0", new StringKeyValueHolder(){ value = "0" } },
                { "1", new StringKeyValueHolder(){ value = "1px" } },
                { "2", new StringKeyValueHolder(){ value = "2px" } },
                { "3", new StringKeyValueHolder(){ value = "3px" } },
                { "4", new StringKeyValueHolder(){ value = "4px" } },
                { "8", new StringKeyValueHolder(){ value = "8px" } },
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-outline", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-text-outline-width" } } }
            }
        };

        [Tooltip("Values for utilities that control the text outline color of an element.")]
        public UtilityConfigWithColorPalette textOutlineColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            data = new ColorPaletteDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-outline", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-text-outline-color" } } }
            }
        };

        [Tooltip("Values for utilities that control the text overflow of an element.")]
        public UtilityConfigWithStringDictionary textOverflow = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "clip", new StringKeyValueHolder(){ value = "clip" } },
                { "ellipsis", new StringKeyValueHolder(){ value = "ellipsis" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-overflow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "text-overflow" } } }
            }
        };

        [Tooltip("Values for utilities that control the text overflow position of an element.")]
        public UtilityConfigWithStringDictionary textOverflowPosition = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "start", new StringKeyValueHolder(){ value = "start" } },
                { "middle", new StringKeyValueHolder(){ value = "middle" } },
                { "end", new StringKeyValueHolder(){ value = "end" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-overflow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "-unity-text-overflow-position" } } }
            }
        };

        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 8)]
        [Tooltip("Values for utilities that control the text shadow size of an element.")]
        public UtilityConfigWithStringDictionary textShadow = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
        {"none", new StringKeyValueHolder(){ value = "0 0 #000" }},
        {"sm", new StringKeyValueHolder(){ value = "0 1px 2px var(--zb-text-shadow-color)" }},
        {ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME, new StringKeyValueHolder(){ value = "0 2px 4px var(--zb-text-shadow-color)" }},
        {"md", new StringKeyValueHolder(){ value = "0 4px 8px var(--zb-text-shadow-color)" }},
        {"lg", new StringKeyValueHolder(){ value = "0 8px 16px var(--zb-text-shadow-color)" }}
    },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-shadow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "text-shadow" } } }
            }
        };

        // We want to generate the text shadow color utility after the text shadow utility so that the text shadow color utility will override the color in the text shadow utility.
        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 9)]
        [Tooltip("Values for utilities that control the text shadow color of an element.")]
        public UtilityConfigWithColorPalette textShadowColor = new UtilityConfigWithColorPalette()
        {
            enabled = true,
            data = new ColorPaletteDictionary(),
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.colors), PropertyFormatter.PropertyExtensionContext.Core)
            },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "text-shadow", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-text-shadow-color" } } }
            }
        };

        // We want to generate this before the translate, rotate, and scale utilities so that the variables used in the transform utility will be overridden by the variables in those utilities.
        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 99)]
        [Tooltip("Values for utilities that control the transform of an element.")]
        public UtilityConfigWithStringDictionary transform = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                {
                    "transform", new StringKeyValueHolder()
                    {
                        value = "0",
                        uss = new USSPropertyToValueDictionary()
                        {
                            { "--zb-scale-x", "1" },
                            { "--zb-scale-y", "1" },
                            { "translate", "var(--zb-translate-x) var(--zb-translate-y)" },
                            { "rotate", "var(--zb-rotate)" },
                            { "scale", "var(--zb-scale-x) var(--zb-scale-y)" }
                        }
                    }
                }
            },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-translate-x", "--zb-translate-y", "--zb-rotate" } } }
            }
        };

        [Tooltip("Values for utilities that specify the origin for an element's transformations.")]
        public UtilityConfigWithStringDictionary transformOrigin = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "center", new StringKeyValueHolder(){ value = "center center" } },
                { "top", new StringKeyValueHolder(){ value = "top" } },
                { "top-right", new StringKeyValueHolder(){ value = "top right" } },
                { "right", new StringKeyValueHolder(){ value = "right" } },
                { "bottom-right", new StringKeyValueHolder(){ value = "bottom right" } },
                { "bottom", new StringKeyValueHolder(){ value = "bottom" } },
                { "bottom-left", new StringKeyValueHolder(){ value = "bottom left" } },
                { "left", new StringKeyValueHolder(){ value = "left" } },
                { "top-left", new StringKeyValueHolder(){ value = "top left" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "origin", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "transform-origin" } } }
            }

        };

        [Tooltip("Values for utilities that control the delay of USS transitions.")]
        public UtilityConfigWithStringDictionary transitionDelay = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    { "0", new StringKeyValueHolder(){ value = "0s" } },
                    { "75", new StringKeyValueHolder(){ value = "75ms" } },
                    { "100", new StringKeyValueHolder(){ value = "100ms" } },
                    { "150", new StringKeyValueHolder(){ value = "150ms" } },
                    { "200", new StringKeyValueHolder(){ value = "200ms" } },
                    { "300", new StringKeyValueHolder(){ value = "300ms" } },
                    { "500", new StringKeyValueHolder(){ value = "500ms" } },
                    { "700", new StringKeyValueHolder(){ value = "700ms" } },
                    { "1000", new StringKeyValueHolder(){ value = "1000ms" } }
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "transition-delay", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "transition-delay" } } }
            }
        };

        [Tooltip("Values for utilities that control the duration of USS transitions.")]
        public UtilityConfigWithStringDictionary transitionDuration = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                        { "0", new StringKeyValueHolder(){ value = "0s" } },
                        { "75", new StringKeyValueHolder(){ value = "75ms" } },
                        { "100", new StringKeyValueHolder(){ value = "100ms" } },
                        { "150", new StringKeyValueHolder(){ value = "150ms" } },
                        { "200", new StringKeyValueHolder(){ value = "200ms" } },
                        { "300", new StringKeyValueHolder(){ value = "300ms" } },
                        { "500", new StringKeyValueHolder(){ value = "500ms" } },
                        { "700", new StringKeyValueHolder(){ value = "700ms" } },
                        { "1000", new StringKeyValueHolder(){ value = "1000ms" } }
                    },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "transition-duration", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "transition-duration" } } }
            }
        };

        [Tooltip("Values for utilities that control which USS properties transition.")]
        public UtilityConfigWithStringDictionary transitionProperty = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "none", new StringKeyValueHolder(){ value = "none" } },
                { "all", new StringKeyValueHolder(){ value = "all" } },
                { ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME, new StringKeyValueHolder(){ value = "background-color, border-color, color, fill, -unity-background-image-tint-color, -unity-text-outline-color, box-shadow, opacity, scale, rotate, translate, transform-origin" } },
                { "colors", new StringKeyValueHolder(){ value = "background-color, border-color, color, fill, -unity-background-image-tint-color, -unity-text-outline-color " } },
                { "opacity", new StringKeyValueHolder(){ value = "opacity" } },
                { "shadow", new StringKeyValueHolder(){ value = "text-shadow, box-shadow" } },
                { "transform", new StringKeyValueHolder(){ value = "scale, rotate, translate, transform-origin" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "transition", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "transition-property" } } }
            }
        };

        [Tooltip("Values for utilities that control the easing of USS transitions.")]
        public UtilityConfigWithStringDictionary transitionTimingFunction = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "linear", new StringKeyValueHolder(){ value = "linear" } },
                { "in", new StringKeyValueHolder(){ value = "ease-in" } },
                { "out", new StringKeyValueHolder(){ value = "ease-out" } },
                { "in-out", new StringKeyValueHolder(){ value = "ease-in-out" } },
                { "in-sine", new StringKeyValueHolder(){ value = "ease-in-sine" } },
                { "out-sine", new StringKeyValueHolder(){ value = "ease-out-sine" } },
                { "in-out-sine", new StringKeyValueHolder(){ value = "ease-in-out-sine" } },
                { "in-cubic", new StringKeyValueHolder(){ value = "ease-in-cubic" } },
                { "out-cubic", new StringKeyValueHolder(){ value = "ease-out-cubic" } },
                { "in-out-cubic", new StringKeyValueHolder(){ value = "ease-in-out-cubic" } },
                { "in-circ", new StringKeyValueHolder(){ value = "ease-in-circ" } },
                { "out-circ", new StringKeyValueHolder(){ value = "ease-out-circ" } },
                { "in-out-circ", new StringKeyValueHolder(){ value = "ease-in-out-circ" } },
                { "in-elastic", new StringKeyValueHolder(){ value = "ease-in-elastic" } },
                { "out-elastic", new StringKeyValueHolder(){ value = "ease-out-elastic" } },
                { "in-out-elastic", new StringKeyValueHolder(){ value = "ease-in-out-elastic" } },
                { "in-back", new StringKeyValueHolder(){ value = "ease-in-back" } },
                { "out-back", new StringKeyValueHolder(){ value = "ease-out-back" } },
                { "in-out-back", new StringKeyValueHolder(){ value = "ease-in-out-back" } },
                { "in-bounce", new StringKeyValueHolder(){ value = "ease-in-bounce" } },
                { "out-bounce", new StringKeyValueHolder(){ value = "ease-out-bounce" } },
                { "in-out-bounce", new StringKeyValueHolder(){ value = "ease-in-out-bounce" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "ease", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "transition-timing-function" } } }
            }

        };

        [UtilityGenerationOrderAttribute(UtilityGenerationOrderAttribute.GenerationOrder.AfterOtherUtilities, 102)]
        [Tooltip("Values for utilities that translate elements with transform.")]
        public UtilityConfigWithStringDictionary translate = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "1of2", new StringKeyValueHolder(){ value = "50%" } },
                { "1of3", new StringKeyValueHolder(){ value = "33.333333%" } },
                { "2of3", new StringKeyValueHolder(){ value = "66.666667%" } },
                { "1of4", new StringKeyValueHolder(){ value = "25%" } },
                { "2of4", new StringKeyValueHolder(){ value = "50%" } },
                { "3of4", new StringKeyValueHolder(){ value = "75%" } },
                { "full", new StringKeyValueHolder(){ value = "100%" } },
                { "minus-1of2", new StringKeyValueHolder(){ value = "-50%" } },
                { "minus-1of3", new StringKeyValueHolder(){ value = "-33.333333%" } },
                { "minus-2of3", new StringKeyValueHolder(){ value = "-66.666667%" } },
                { "minus-1of4", new StringKeyValueHolder(){ value = "-25%" } },
                { "minus-2of4", new StringKeyValueHolder(){ value = "-50%" } },
                { "minus-3of4", new StringKeyValueHolder(){ value = "-75%" } },
                { "minus-full", new StringKeyValueHolder(){ value = "-100%" } }
            },
            modifierVariations = new StringListValue(){
                "hover",
                "focus",
            },
            extendFields = new StringListValue(){
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core),
                PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.negativeSpacing), PropertyFormatter.PropertyExtensionContext.Core)
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "translate-x", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-translate-x" } } },
                { "translate-y", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "--zb-translate-y" } } }
            }

        };

        [Tooltip("Values for utilities that control the visibility of an element.")]
        public UtilityConfigWithStringDictionary visibility = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "visible", new StringKeyValueHolder(){ value = "visible" } },
                { "invisible", new StringKeyValueHolder(){ value = "hidden" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "visibility" } } }
            }
        };

        [Tooltip("Values for utilities that control the white space of an element.")]
        public UtilityConfigWithStringDictionary whitespace = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                { "normal", new StringKeyValueHolder(){ value = "normal" } },
                { "nowrap", new StringKeyValueHolder(){ value = "nowrap" } }
            },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "whitespace", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "white-space" } } }
            }
        };

        [Tooltip("Values for utilities for setting the width of an element.")]
        public UtilityConfigWithStringDictionary width = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    { "1of2", new StringKeyValueHolder(){ value = "50%" } },
                    { "1of3", new StringKeyValueHolder(){ value = "33.333333%" } },
                    { "2of3", new StringKeyValueHolder(){ value = "66.666667%" } },
                    { "1of4", new StringKeyValueHolder(){ value = "25%" } },
                    { "2of4", new StringKeyValueHolder(){ value = "50%" } },
                    { "3of4", new StringKeyValueHolder(){ value = "75%" } },
                    { "1of5", new StringKeyValueHolder(){ value = "20%" } },
                    { "2of5", new StringKeyValueHolder(){ value = "40%" } },
                    { "3of5", new StringKeyValueHolder(){ value = "60%" } },
                    { "4of5", new StringKeyValueHolder(){ value = "80%" } },
                    { "1of6", new StringKeyValueHolder(){ value = "16.666667%" } },
                    { "2of6", new StringKeyValueHolder(){ value = "33.333333%" } },
                    { "3of6", new StringKeyValueHolder(){ value = "50%" } },
                    { "4of6", new StringKeyValueHolder(){ value = "66.666667%" } },
                    { "5of6", new StringKeyValueHolder(){ value = "83.333333%" } },
                    { "1of12", new StringKeyValueHolder(){ value = "8.333333%" } },
                    { "2of12", new StringKeyValueHolder(){ value = "16.666667%" } },
                    { "3of12", new StringKeyValueHolder(){ value = "25%" } },
                    { "4of12", new StringKeyValueHolder(){ value = "33.333333%" } },
                    { "5of12", new StringKeyValueHolder(){ value = "41.666667%" } },
                    { "6of12", new StringKeyValueHolder(){ value = "50%" } },
                    { "7of12", new StringKeyValueHolder(){ value = "58.333333%" } },
                    { "8of12", new StringKeyValueHolder(){ value = "66.666667%" } },
                    { "9of12", new StringKeyValueHolder(){ value = "75%" } },
                    { "10of12", new StringKeyValueHolder(){ value = "83.333333%" } },
                    { "11of12", new StringKeyValueHolder(){ value = "91.666667%" } },
                    { "full", new StringKeyValueHolder(){ value = "100%" } }
                },
            extendFields = new StringListValue(){
                    PropertyFormatter.FormatPropertyExtensionName(nameof(BaseCoreProperties.spacing), PropertyFormatter.PropertyExtensionContext.Core)
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "w", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "width" } } }
            }
        };

        [Tooltip("Values for utilities for setting the word spacing of an element.")]
        public UtilityConfigWithStringDictionary wordSpacing = new UtilityConfigWithStringDictionary()
        {
            enabled = true,
            data = new KeyValueDictionary(){
                    { "tighter", new StringKeyValueHolder(){ value = "-5px" } },
                    { "tight", new StringKeyValueHolder(){ value = "-2.5px" } },
                    { "normal", new StringKeyValueHolder(){ value = "0" } },
                    { "wide", new StringKeyValueHolder(){ value = "2.5px" } },
                    { "wider", new StringKeyValueHolder(){ value = "5px" } },
                    { "widest", new StringKeyValueHolder(){ value = "10px" } }
                },
            tagPropertyMap = new ClassTagToUssPropertyDictionary(){
                { "word-space", new ClassTagPropertyHolder(){ properties = new StringListValue(){ "word-spacing" } } }
            }
        };
    }





    /// <summary>
    /// The theme configuration object. it holds the data that is used to generate the theme. It should be serializable to and from JSON.
    /// </summary>
    [System.Serializable]
    public class ThemeConfig
    {
        // Reusable base configs
        public BaseCoreProperties core = new BaseCoreProperties();


        // Utilities

        public UtilityProperties utilities = new UtilityProperties();


        /// <summary>
        /// These control how the uss file is generated.
        /// </summary>
        public CompilationConfig compilation = new CompilationConfig();

        /// <summary>
        /// The version of the themeconfig schema used to generate this theme.
        /// </summary>
        public readonly string schemaVersion = "1.0.0";


        public static string ToJson(ThemeConfig config)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented);
        }

        public static ThemeConfig FromJson(string jsonString)
        {
            // We need to set the object creation handling to Replace otherwise it will potentially add duplicate values to the lists like extendFields and modifierVariations.
            return Newtonsoft.Json.JsonConvert.DeserializeObject<ThemeConfig>(jsonString, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
        }


    }


}



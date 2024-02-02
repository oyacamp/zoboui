using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoboUI.Core.Utils;
using ZoboUI.Editor;



namespace ZoboUI.Core.Plugins
{


    public enum RuleGenerationPriority
    {
        BeforeDefaultUtilities = 0,
        AfterDefaultUtilities = 1,
    }

    public interface IProcessThemeConfig
    {
        /// <summary>
        /// This is called when classes and selectors are being generated. Allows the plugin to perform an action based on the provided ThemeConfig. It also provides access to the UtilityRuleBag, which is used to add rules to the generated USS file. The key in the Bag is the rule selector e.g ".bg-red" and the value is a dictionary of the USS properties and values for those properties e.g "background-color" : "red". Selectors added here must be unique. If you add a selector that was already added by another utility plugin, it will overwrite the data for that selector in the bag.
        /// </summary>
        /// <param name="themeConfig"></param>
        /// <param name="bag"></param>
        /// <param name="logger"></param>
        void ProcessThemeConfig(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger = null);

        /// <summary>
        /// This is used to determine the order in which the plugin is run when when generating rules. This is useful if you need to override the behaviour of the themeConfig or if you want to ensure that the rules generated by your plugin are applied after the rules generated by the default utilities.
        /// </summary>
        RuleGenerationPriority RuleGenerationPriority { get; }

        /// <summary>
        /// This is called when the purge process is complete. Allows the plugin to perform an action based on the provided ThemeConfig. It also provides access to the UtilityRuleBag, which is used to see which rules remain. The key in the Bag is the rule selector e.g ".bg-red" and the value is a dictionary of the USS properties and values for those properties e.g "background-color" : "red". This is only called when the user requests a purge.
        /// </summary>
        /// <param name="themeConfig"></param>
        /// <param name="bag"></param>
        /// <param name="logger"></param>
        void OnPurgeComplete(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger = null);

    }

    public interface IUseCustomClassExtractor
    {
        IClassExtractor GetClassExtractor(ICustomLogger logger = null);
    }

    /// <summary>
    /// Base class for all utility plugins. Inherit from this class to create a utility plugin.
    /// </summary>
    public abstract class BaseUtilityPlugin : ScriptableObject, IProcessThemeConfig, IUseCustomClassExtractor
    {

        /// <summary>
        /// The namespace of the plugin. This is similar to a package namespace and is used to uniquely identify the plugin. We recommend using the format "com.yourcompany.yourpluginname".
        /// </summary>
        public abstract string PluginNamespace { get; }


        /// <summary>
        /// The name of the utility as it will appear in the inspector.
        /// </summary>
        public abstract string PluginUtilityName { get; }

        /// <summary>
        /// The description of the plugin. This is used to describe the plugin in the inspector.
        /// </summary>
        public abstract string PluginDescription { get; }




        public abstract RuleGenerationPriority RuleGenerationPriority { get; }


        public virtual void ProcessThemeConfig(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger = null)
        {

        }

        /// <summary>
        /// Returns the class extractor for the plugin. This is used to extract used classes from files in the project during purging. You only really need this if you're generating classes that don't follow the normal naming convention like "rounded-sm" or "bg-blue-200". If you do not need to extract classes, return null.
        /// </summary>
        /// <returns></returns>
        public virtual IClassExtractor GetClassExtractor(ICustomLogger logger = null)
        {
            return null;
        }

        /// <summary>
        /// Allows you to initialize the plugin from a json string. This is used when the user's theme config is imported from a json file. The format of the json string passed to this method is determined by you in the ToJson method.
        /// </summary>
        /// <param name="jsonString"></param>
        public abstract void FromJson(string jsonString);

        /// <summary>
        /// Allows you to store relevant plugin configuration data in a json string. This is used when the user's theme config is exported to a json file. The json string returned by this method will be stored in the exported json file.
        /// </summary>
        /// <returns></returns>
        public abstract string ToJson();

        public virtual void OnPurgeComplete(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger = null)
        {

        }
    }
}
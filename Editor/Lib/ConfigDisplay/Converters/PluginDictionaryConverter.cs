using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using ZoboUI.Core;
using ZoboUI.Core.Plugins;
using ZoboUI.Core.Utils;
using ZoboUI.Editor;


namespace ZoboUI.Editor
{

    [System.Serializable]
    public class PluginValueHolder
    {
        /// <summary>
        /// Serialized data for the plugin
        /// </summary>
        public string data;

        /// <summary>
        /// The path to the asset that holds the plugin
        /// </summary>
        public string assetPath;

        /// <summary>
        /// The type of the plugin class
        /// </summary>
        public string typeName;
    }

    /// <summary>
    /// The version of the plugin dictionary that is used in json version of the config.
    /// </summary>
    [System.Serializable]
    public class PluginDictionary : SimpleConfigValueDictionary<PluginValueHolder>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            PluginDictionaryConverter pluginDictionaryConverter = new PluginDictionaryConverter(logger);

            return pluginDictionaryConverter.ConvertFromDisplay(display.ValuesPluginDictionary);
        }


        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            PluginDictionaryConverter pluginDictionaryConverter = new PluginDictionaryConverter(logger);

            var display = new BaseCorePropertyValueDisplay
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };

            display.ValueType = CorePropertyValueType.PluginDictionary;
            display.ValuesPluginDictionary = pluginDictionaryConverter.ConvertToDisplay(this);

            return display;
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.PluginDictionary;
        }

        public override List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string key = item.Key;
                PluginValueHolder value = item.Value;

                ConfigValueResultItem resultItem = new ConfigValueResultItem();

                resultItem.Key = key;
                resultItem.Value = value.data;
                resultItem.UssDictionary = new USSPropertyToValueDictionary();

                requiredData.Add(resultItem);
            }

            return requiredData;

        }

        public override bool IsSameValueType(IConfigValue other)
        {
            return other is PluginDictionary;
        }
    }
}

/// <summary>
/// A representation of a plugin dictionary for the inspector.
/// </summary>
[System.Serializable]
public class InspectorPluginDictionaryDisplay : IPropertyDisplayItemValue, IWithUniqueKey
{

    public BaseUtilityPlugin Value;

    public void SetInitialValuesForNewItems()
    {
        Value = null;
    }


    public string GetUniqueKey()
    {
        return Value?.PluginNamespace;
    }
}

public class PluginDictionaryConverter : IConvertibleToDisplay<List<InspectorPluginDictionaryDisplay>, PluginDictionary>
{
    private Dictionary<string, Type> pluginTypes;

    private ICustomLogger logger;

    public PluginDictionaryConverter(ICustomLogger logger = null)
    {
        this.logger = logger;

        if (logger == null)
        {
            this.logger = new CustomLogger();
        }
    }

    private void InitializePluginTypes()
    {
        // Initialize the dictionary
        pluginTypes = new Dictionary<string, Type>();

        // Get all types in the assembly
        //Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        Type[] types = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => type.IsClass && !type.IsAbstract)
               .ToArray();
        // Add types that are subclasses of BaseUtilityPlugin to the dictionary
        foreach (var type in types)
        {

            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(BaseUtilityPlugin)))
            {
                // Create an instance of the type to get its namespace
                // BaseUtilityPlugin plugin = (BaseUtilityPlugin)Activator.CreateInstance(type);

                BaseUtilityPlugin plugin = (BaseUtilityPlugin)ScriptableObject.CreateInstance(type);

                // Add the type to the dictionary, keyed by its namespace
                pluginTypes[plugin.PluginNamespace] = type;
            }
        }

    }


    public PluginDictionary ConvertFromDisplay(List<InspectorPluginDictionaryDisplay> display)
    {
        var model = new PluginDictionary();

        foreach (var displayItem in display)
        {
            if (displayItem.Value == null)
            {
                continue;
            }

            PluginValueHolder valueHolder = new PluginValueHolder
            {
                data = displayItem.Value.ToJson(),
                assetPath = AssetDatabase.GetAssetPath(displayItem.Value),
                typeName = displayItem.Value.GetType().Name
            };
            model[displayItem.Value.PluginNamespace] = valueHolder;
        }
        return model;
    }



    /// <summary>
    /// This looks for classes that inherit from BaseUtilityPlugin in the project and returns an instance of the plugin.
    /// </summary>
    /// <param name="pluginNamespace"></param>
    /// <returns></returns>
    public BaseUtilityPlugin CreatePluginScriptableObjectFromNamespace(string pluginNamespace, string assetPath)
    {
        if (pluginTypes == null)
        {
            InitializePluginTypes();
        }

        // Check if the dictionary contains the namespace
        if (pluginTypes.TryGetValue(pluginNamespace, out Type type))
        {
            return this.CreatePluginScriptableObjectFromType(type, assetPath);
        }

        // If the namespace is not in the dictionary, return null
        return null;


    }

    public BaseUtilityPlugin CreatePluginScriptableObjectFromType(Type type, string pathToStoreAsset)
    {
        // Check if the type is a subclass of BaseUtilityPlugin
        if (!type.IsSubclassOf(typeof(BaseUtilityPlugin)))
        {
            return null;
        }

        // create an instance of the type
        BaseUtilityPlugin plugin = (BaseUtilityPlugin)ScriptableObject.CreateInstance(type);

        // Create an asset for the ScriptableObject
        AssetDatabase.CreateAsset(plugin, pathToStoreAsset);

        // Save the changes to the AssetDatabase
        AssetDatabase.SaveAssets();

        return plugin;
    }

    protected BaseUtilityPlugin LoadPluginScriptableObjectFromAssetPath(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<BaseUtilityPlugin>(assetPath);
    }



    public List<InspectorPluginDictionaryDisplay> ConvertToDisplay(PluginDictionary model)
    {
        List<InspectorPluginDictionaryDisplay> displayList = new List<InspectorPluginDictionaryDisplay>();

        foreach (var entry in model)
        {
            string assetPath = entry.Value.assetPath;
            BaseUtilityPlugin plugin = null;

            if (!string.IsNullOrEmpty(assetPath))
            {
                // Attempt to load the scriptable object from the asset path
                plugin = AssetDatabase.LoadAssetAtPath<BaseUtilityPlugin>(assetPath);
            }
            else
            {
                assetPath = $"Assets/{entry.Key}.asset";
            }
            string typeName = entry.Value.typeName;

            // Try to load the plugin from the type name
            if (plugin == null && !string.IsNullOrEmpty(typeName))
            {
                // Get the type from the type name
                Type type = Type.GetType(typeName);

                if (type != null)
                {
                    // Create an instance of the type
                    plugin = this.CreatePluginScriptableObjectFromType(type, assetPath);

                }
            }

            // Last resort: If the plugin is still null, check for the types in the project and create a new instance of the plugin if matching type is found
            if (plugin == null)
            {

                plugin = CreatePluginScriptableObjectFromNamespace(entry.Key, assetPath);
            }

            // If the plugin is still null, it means the the code for the plugin is not in the project
            if (plugin == null)
            {
                this.logger.LogError($"Could not find plugin with namespace {entry.Key} and type '{entry.Value}'. Please make sure the plugin is installed in the project.");
                continue;
            }

            // Initialize the plugin from the stored json data
            plugin.FromJson(entry.Value.data);

            displayList.Add(new InspectorPluginDictionaryDisplay { Value = plugin });
        }

        return displayList;

    }
}

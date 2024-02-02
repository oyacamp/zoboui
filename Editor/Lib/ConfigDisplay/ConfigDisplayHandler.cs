using System;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using System.Linq;
using ZoboUI.Editor.Utils;
using System.Reflection;
using ZoboUI.Core.Utils;
using System.Text.RegularExpressions;
using System.IO;
using ZoboUI.Editor.Attributes;


namespace ZoboUI.Editor
{

    /// <summary>
    /// Should be implemented by classes that can be converted to a BaseUtilityConfig. e.g. SpaceUtilityConfig, UtilityConfigWithStringDictionary, etc.
    /// </summary>
    public interface IConvertibleToBaseUtilityConfig
    {
        BaseUtilityConfig ConvertToBaseUtilityConfig(UtilityConfigDisplay configDisplay, ICustomLogger logger = null);


    }

    /// <summary>
    /// Should be implemented by classes that can be converted to a UtilityConfigDisplay. e.g. SpaceUtilityConfig, UtilityConfigWithStringDictionary, etc.
    /// </summary>
    public interface IConvertibleToUtilityConfigDisplay
    {
        UtilityConfigDisplay ConvertToUtilityConfigDisplay(ICustomLogger logger = null);
    }


    public interface IConvertibleToBaseCorePropertyValueDisplay
    {
        BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null);

        /// <summary>
        /// Returns the type of the value that the property should be set to in the display version of the config.
        /// </summary>
        /// <returns></returns>
        CorePropertyValueType GetBaseCorePropertyTypeValue();
    }

    /// <summary>
    /// Should be implemented by attributes that can be used to convert a property to a BaseCorePropertyValueDisplay. e.g string or booleans which can't directly implement IConvertibleToBaseCorePropertyValueDisplay
    /// </summary>
    public interface IAttributeWithBaseCorePropertyConverter
    {
        /// <summary>
        /// Converts a property to a BaseCorePropertyValueDisplay. This is useful for properties that can't directly implement IConvertibleToBaseCorePropertyValueDisplay. e.g. string or booleans
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <param name="tooltipText"></param>
        /// <returns></returns>
        BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, object propertyValue, string tooltipText = null, ICustomLogger logger = null);

        /// <summary>
        /// Converts a BaseCorePropertyValueDisplay to a value that can be used in the theme config. e.g. a string or boolean
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null);


        /// <summary>
        /// Returns the type of the value that the property should be set to in the display version of the config.
        /// </summary>
        /// <returns></returns>
        CorePropertyValueType GetBaseCorePropertyTypeValue();

    }

    /// <summary>
    /// Should be implemented by classes that can be converted from a BaseCorePropertyValueDisplay to a field value in the ThemeConfig
    /// </summary>
    public interface IConvertibleFromBaseCorePropertyValueDisplay
    {
        object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null);
    }


    /// <summary>
    /// Should be implemented by classes that need to display a value in the inspector based on a theme config value. e.g. KeyValueDictionaryDisplay, ColorPaletteDisplay, etc.
    /// </summary>
    public interface IPropertyDisplayItemValue
    {
        void SetInitialValuesForNewItems();

    }

    /// <summary>
    /// This interface is used to get a unique key for a class. This is useful for classes that are used in a list. e.g. If you have a list of colors, you need a way to identify each color. You can do this by implementing this interface and returning a unique key for each color.
    /// </summary>
    public interface IWithUniqueKey
    {
        string GetUniqueKey();

    }



    public interface IConvertibleToDisplay<TDisplay, TModel>
    {
        TDisplay ConvertToDisplay(TModel model);
        TModel ConvertFromDisplay(TDisplay display);
    }

    public interface IWithTooltipText
    {
        string TooltipText { get; set; }
    }


    /// <summary>
    /// This class is used for properties that should also allow adding custom uss properties. These will show up as a key value pair in the inspector.
    /// </summary>
    [System.Serializable]
    public class DisplayWithCustomUssProperties
    {
        public List<UssPropertyHolderDisplay> UssProperties;
    }



    /// <summary>
    /// This class can be inherited by classes that need to display a value with an asset field. e.g. FontAssetValueDictionaryDisplay or ImageValueDictionaryDisplay
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [System.Serializable]
    public class BaseInspectorDisplayWithAsset<T> : DisplayWithCustomUssProperties, IPropertyDisplayItemValue, IWithUniqueKey where T : UnityEngine.Object
    {
        public string Key;

        public UnityEngine.Object ValueObject;

        public string CustomStringValue;


        public string GetUniqueKey()
        {
            return Key;
        }

        public static string GetUniqueKeyPropertyName()
        {
            return nameof(Key);
        }


        public virtual void SetInitialValuesForNewItems()
        {
            Key = "newkey";
            ValueObject = null;
            CustomStringValue = "";
        }
    }






    // INTERFACE

    [System.Serializable]
    public enum CorePropertyValueType
    {
        KeyValueDictionary = 1,
        ColorPalette = 2,
        ImageValueDictionary = 3,
        FontAssetValueDictionary = 4,
        String = 5,

        StringList = 6,

        ClassModifierDictionary = 7,

        PluginDictionary = 8,
    }





    // CORE
    /// <summary>
    /// The version of the core properties (e.g spacing, breakpoints) we use to display the config in the inspector. The ideal way to do this would be to inherit the base class and add the display properties but Unity doesn't support that easily with property drawers, so we stuff the different display properties in one class for now. Learn more: https://forum.unity.com/threads/drawing-an-object-with-custompropertydrawer-that-has-a-child-class.977829/
    /// </summary>
    [System.Serializable]
    public class BaseCorePropertyValueDisplay
    {


        public string PropertyName;

        public string DisplayName;
        public string TooltipText;

        public bool HideUssDictionaryIfPresent = false;


        public CorePropertyValueType ValueType;

        public List<InspectorKeyValueDictionaryDisplay> ValuesKeyValueDictionary;

        public List<InspectorColorPaletteDisplay> ValuesColorPalette;

        public List<InspectorImageValueDictionaryDisplay> ValuesImageValueDictionary;

        public List<InspectorFontAssetValueDictionaryDisplay> ValuesFontAssetValueDictionary;

        public TextFieldStringData ValueString;

        public TextFieldDataList ValueStringList;

        public List<ClassModifierDisplay> ValuesClassModifierDictionary;


        public List<InspectorPluginDictionaryDisplay> ValuesPluginDictionary;




    }



    // UTILITIES

    /// <summary>
    /// The version of the utility config we use to display the config in the inspector. We've stuffed all the different display properties in one class for property drawer reasons. Learn more: https://forum.unity.com/threads/drawing-an-object-with-custompropertydrawer-that-has-a-child-class.977829/
    /// </summary>

    [System.Serializable]
    public class UtilityConfigDisplay : IWithTooltipText
    {
        /// <summary>
        /// Whether the utility is enabled or not.
        /// </summary>
        [Tooltip("Whether the utility is enabled or not. If disabled, no classes will be generated for this utility.")]
        public bool Enabled;


        [Tooltip("The types of modifer variations to generate for this utility. E.g ['hover', 'focus', 'active'] would generate classes like 'hover_bg-red', 'focus:bg-red', etc.")]
        public List<ClassModifierStringDropdownFieldValue> ModifierVariations;


        [Tooltip("The names of the properties to extend. This is useful for creating utilities that use the same values as other utilities. e.g. if you want to use the values from the 'spacing' config field for the 'margin' config field, you would set this to ['spacing']. If you wanted to extend another field, you would do the add the field name to the array. Keep in mind that the order of the array matters. The values will be applied in the order they are listed in the array so if you want to override a value, make sure to list it after the field you want to override.")]
        public List<ExtendFieldsStringDropdownFieldValue> ExtendFields;

        [Tooltip("A list of class tags to generate for this utility. e.g ['bg', 'text', 'border']. The class tags will be used as the class name. e.g 'bg-red', 'text-sm', etc. The key is the tag (e.g 'bg', or 'text'), and the property names are the USS property names to generate using the values in the provided value type's dictionary.")]
        public List<ClassTagToUssPropertyMapDisplay> ClassTagToUssPropertyMap;

        /// <summary>
        /// The name of the utility in the ThemeConfig object.
        /// </summary>
        public string PropertyName;

        /// <summary>
        /// The version of the PropertyDisplay we use to display the config in the inspector. This is used to display the config in the inspector.
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// This holds the display values for the utility config. This is used to display the config in the inspector.
        /// </summary>
        public BaseCorePropertyValueDisplay PropertyDisplay;

        public string TooltipText { get; set; }
    }

    [System.Serializable]
    public class CorePropertyValueTypeToExtensibleFieldsDictionary : Dictionary<CorePropertyValueType, List<string>> { }


    /// <summary>
    /// This class holds a list of fields that can be extended by other fields. We have a list version because Unity doesn't support dictionaries in property drawers. So we use a list because it serializes better
    /// </summary>
    [System.Serializable]
    public class CorePropertyValueTypeToExtensibleFieldsMapItem
    {
        public CorePropertyValueType ValueType;
        public List<string> ExtensibleFields;
    }



    /// <summary>
    /// The version of the theme config we use to display the config in the inspector.
    /// </summary>
    [System.Serializable]
    public class ThemeConfigDisplayVersion
    {
        [SerializeField] private string schemaVersion;

        public List<BaseCorePropertyValueDisplay> Core;

        public List<UtilityConfigDisplay> Utilities;

        public List<BaseCorePropertyValueDisplay> Compilation;


        public RequiredStringDropdownInfo RequiredStringDropdownInfoInstance;

        public string SchemaVersion
        {
            get
            {
                return schemaVersion;
            }
            set
            {
                schemaVersion = value;
            }
        }

        // Add a constructor that sets the version
        public ThemeConfigDisplayVersion(string schemaVersion)
        {
            this.schemaVersion = schemaVersion;
        }

    }



    /// <summary>
    /// This class is used to convert the theme config to something we can view in the inspector.
    /// </summary>
    public class ConfigDisplayHandler
    {

        private ICustomLogger m_logger;

        // Create a constructor that takes a logger config
        public ConfigDisplayHandler(ICustomLogger logger = null)
        {
            m_logger = logger == null ? new CustomLogger(nameof(ConfigDisplayHandler)) : logger;
        }



        /// <summary>
        /// This dictionary holds the names of fields that other fields can extend. Fields should only be able to extend fields of the same type. e.g. a StringValueDictionary can only extend other StringValueDictionaries.
        /// </summary>
        private readonly CorePropertyValueTypeToExtensibleFieldsDictionary valueTypeToFieldExtensionNames = new CorePropertyValueTypeToExtensibleFieldsDictionary();



        private void AddFieldNameToFieldExtensionDictionary(CorePropertyValueType valueType, string fieldName)
        {
            if (valueTypeToFieldExtensionNames.ContainsKey(valueType))
            {

                valueTypeToFieldExtensionNames[valueType].Add(fieldName);
            }
            else
            {
                valueTypeToFieldExtensionNames.Add(valueType, new List<string> { fieldName });
                //m_logger.LogError("No field extension list found for " + valueType);
            }
        }

        private string GetTooltipTextFromField(MemberInfo field)
        {
            return field.GetCustomAttribute<TooltipAttribute>()?.tooltip;
        }

        private TextFieldAttribute GetTextFieldAttributeFromField(MemberInfo field)
        {
            return field.GetCustomAttribute<TextFieldAttribute>();
        }

        private bool FieldCanBeExtended(MemberInfo field)
        {
            // Check if the field has a CanBeExtendedAttribute
            var canBeExtendedAttribute = field.GetCustomAttribute<CanBeExtendedAttribute>();

            return canBeExtendedAttribute != null;
        }



        private IAttributeWithBaseCorePropertyConverter GetAttributeWithBaseCorePropertyConverterFromField(MemberInfo field)
        {
            // Get all the attributes on the field
            var attributes = field.GetCustomAttributes();

            // Loop through the attributes and check if any of them implement IAttributeWithBaseCorePropertyConverter
            foreach (var attribute in attributes)
            {
                if (attribute is IAttributeWithBaseCorePropertyConverter attributeWithBaseCorePropertyConverter)
                {
                    return attributeWithBaseCorePropertyConverter;
                }
            }

            return null;
        }

        private string GetCustomDisplayNameFromField(MemberInfo field)
        {
            if (field.GetCustomAttribute<CustomDisplayNameAttribute>()?.DisplayName != null)
            {
                return field.GetCustomAttribute<CustomDisplayNameAttribute>().DisplayName;
            }
            else
            {
                return PropertyFormatter.FormatPropertyNameForDisplay(field.Name);
            }
        }

        private bool ShouldHideUssDictionaryIfPresent(MemberInfo field)
        {
            return field.GetCustomAttribute<HideUssDictionaryInDisplayAttribute>() != null;
        }


        private List<BaseCorePropertyValueDisplay> ConvertSimplePropertiesToDisplayVersion<T>(T core, PropertyFormatter.PropertyExtensionContext propertyContext)
        {
            var displayList = new List<BaseCorePropertyValueDisplay>();

            var coreFields = typeof(T).GetFields();

            foreach (var field in coreFields)
            {
                var value = field.GetValue(core);
                var fieldType = field.FieldType;
                var fieldName = field.Name;


                var tooltipText = GetTooltipTextFromField(field);

                BaseCorePropertyValueDisplay display;

                // Check if the field has a TextFieldAttribute
                var textFieldAttribute = GetTextFieldAttributeFromField(field);

                // Check if the field has an attribute that implements IAttributeWithBaseCorePropertyConverter
                var attributeWithBaseCorePropertyConverter = GetAttributeWithBaseCorePropertyConverterFromField(field);

                if (typeof(IConvertibleToBaseCorePropertyValueDisplay).IsAssignableFrom(fieldType))
                {
                    var convertible = (IConvertibleToBaseCorePropertyValueDisplay)value;

                    if (textFieldAttribute != null && convertible is StringListValue)
                    {
                        // We want to set the validator for the StringListValue so that it is applied to each item in the list
                        (convertible as StringListValue).Validator = textFieldAttribute.Validator;

                    }

                    display = convertible.ConvertToBaseCorePropertyValueDisplay(fieldName, tooltipText, m_logger);

                    if (convertible.GetBaseCorePropertyTypeValue() != display.ValueType)
                    {
                        throw new Exception(m_logger.FormatMessage($"The value type of the field {fieldName} does not match the value type of the display version. Make sure the value type of the field matches the value type of the display version"));
                    }

                }
                // If the field itself doesn't implement IConvertibleToBaseCorePropertyValueDisplay, e.g because its primitive like a string or bool, check if the field has an attribute that implements IAttributeWithBaseCorePropertyConverter   
                else if (attributeWithBaseCorePropertyConverter != null)
                {

                    display = attributeWithBaseCorePropertyConverter.ConvertToBaseCorePropertyValueDisplay(fieldName, value, tooltipText, m_logger);

                    if (attributeWithBaseCorePropertyConverter.GetBaseCorePropertyTypeValue() != display.ValueType)
                    {

                        throw new Exception(m_logger.FormatMessage($"The value type of the field {fieldName} from the {attributeWithBaseCorePropertyConverter.GetType()} attribute does not match the value type of the display version. Make sure the value type of the field matches the value type of the display version"));
                    }

                }
                else
                {
                    m_logger.LogError($"Could not convert field {fieldName} to BaseCorePropertyValueDisplay. Make sure the type inherits from IConvertibleToBaseCorePropertyValueDisplay or uses an attribute that implements IAttributeWithBaseCorePropertyConverter");
                    continue;
                }

                if (ShouldHideUssDictionaryIfPresent(field))
                {
                    display.HideUssDictionaryIfPresent = true;
                }

                displayList.Add(display);
                string extensionName = PropertyFormatter.FormatPropertyExtensionName(fieldName, propertyContext);


                if (FieldCanBeExtended(field))
                {
                    AddFieldNameToFieldExtensionDictionary(display.ValueType, extensionName);
                }

                if (string.IsNullOrEmpty(display.DisplayName))
                {
                    display.DisplayName = GetCustomDisplayNameFromField(field);
                }



            }

            return displayList;
        }

        private T ConvertDisplayVersionToSimpleProperties<T>(List<BaseCorePropertyValueDisplay> displayVersion) where T : new()
        {
            var coreFields = typeof(T).GetFields();
            // Convert the displayVersion list to a dictionary using the property name as the key
            var displayVersionDictionary = displayVersion.ToDictionary(x => x.PropertyName, x => x);

            var core = new T();

            foreach (var field in coreFields)
            {
                var fieldType = field.FieldType;
                var fieldName = field.Name;




                if (!displayVersionDictionary.ContainsKey(fieldName))
                {
                    this.m_logger.LogWarning("No display value found for " + fieldName);
                    continue;
                }

                BaseCorePropertyValueDisplay displayValue = displayVersionDictionary[fieldName];

                // Check if the field has an attribute that implements IAttributeWithBaseCorePropertyConverter
                var attributeWithBaseCorePropertyConverter = GetAttributeWithBaseCorePropertyConverterFromField(field);


                // Check if the  field type inherits from IConvertibleFromBaseCorePropertyValueDisplay and then create an instance of the type
                if (typeof(IConvertibleFromBaseCorePropertyValueDisplay).IsAssignableFrom(fieldType))
                {
                    var convertible = Activator.CreateInstance(fieldType) as IConvertibleFromBaseCorePropertyValueDisplay;

                    if (convertible == null)
                    {
                        throw new Exception($"Could not convert field {field.Name} to BaseCorePropertyValueDisplay as the value is null. Make sure the type inherits from IConvertibleFromBaseCorePropertyValueDisplay");
                    }

                    var display = displayVersionDictionary[fieldName];
                    var value = convertible.ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(display);
                    field.SetValue(core, value);
                }
                else if (attributeWithBaseCorePropertyConverter != null)
                {
                    var valueToStore = attributeWithBaseCorePropertyConverter.ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(displayValue);
                    field.SetValue(core, valueToStore);
                }
                else
                {
                    m_logger.LogError($"Could not convert field {fieldName} to BaseCorePropertyValueDisplay. Make sure the type inherits from IConvertibleFromBaseCorePropertyValueDisplay");
                    continue;
                }

            }

            return core;
        }



        private List<UtilityConfigDisplay> ConvertUtilitiesToDisplayVersion(UtilityProperties utilities)
        {
            var displayList = new List<UtilityConfigDisplay>();

            var utilityFields = typeof(UtilityProperties).GetFields();


            foreach (var field in utilityFields)
            {
                var value = field.GetValue(utilities);
                var fieldType = field.FieldType;
                var tooltipText = GetTooltipTextFromField(field);



                // Check if the field value inherits from IConvertibleToUtilityConfigDisplay
                /*
                                if (value is IConvertibleToUtilityConfigDisplay convertible)
                                {
                                    var display = convertible.ConvertToUtilityConfigDisplay();
                                    display.TooltipText = tooltipText;
                                    displayList.Add(display);
                                }*/

                if (typeof(IConvertibleToUtilityConfigDisplay).IsAssignableFrom(fieldType))
                {

                    var convertible = value as IConvertibleToUtilityConfigDisplay;

                    if (convertible == null)
                    {
                        throw new Exception($"Could not convert field {field.Name} to UtilityConfigDisplay as the value is null. Make sure the type inherits from IConvertibleToUtilityConfigDisplay");
                    }

                    var display = convertible.ConvertToUtilityConfigDisplay(m_logger);

                    display.PropertyName = field.Name;

                    display.TooltipText = tooltipText;

                    // Check if the ModifierVariations list has duplicates and remove them but keep the order
                    display.ModifierVariations = display.ModifierVariations.Distinct().ToList();
                    display.ExtendFields = display.ExtendFields.Distinct().ToList();

                    displayList.Add(display);


                    if (FieldCanBeExtended(field))
                    {
                        string extensionName = PropertyFormatter.FormatPropertyExtensionName(display.PropertyName, PropertyFormatter.PropertyExtensionContext.Utilities);
                        AddFieldNameToFieldExtensionDictionary(display.PropertyDisplay.ValueType, extensionName);
                    }

                    if (string.IsNullOrEmpty(display.DisplayName))
                    {
                        display.DisplayName = GetCustomDisplayNameFromField(field);

                    }
                    // Make sure the core property display has a display name as well just in case it was not set
                    if (string.IsNullOrEmpty(display.PropertyDisplay.DisplayName))
                    {
                        display.PropertyDisplay.DisplayName = "Values";

                    }



                }
                else
                {
                    m_logger.LogError($"Could not convert field {field.Name} to UtilityConfigDisplay. Make sure the type inherits from IConvertibleToUtilityConfigDisplay");
                    continue;
                }


            }

            return displayList;
        }





        /// <summary>
        /// This creates a UtilityConfigDisplay when provided with the type of the field in the ThemeConfig, and the display version of the field in the ConfigDisplayHandler. If the field type is not a valid BaseUtilityConfig, it returns null.
        /// </summary>
        /// <param name="fieldTypeInThemeConfig"></param>
        /// <param name="displayValue"></param>
        /// <returns></returns>
        public BaseUtilityConfig ConvertUtilityConfigDisplayItemToUtilityConfig(Type fieldTypeInThemeConfig, UtilityConfigDisplay displayValue)
        {
            // Check if the fieldType is assignable to IConvertibleToBaseUtilityConfig
            if (typeof(IConvertibleToBaseUtilityConfig).IsAssignableFrom(fieldTypeInThemeConfig))
            {
                // Use reflection to create an instance of the fieldType
                var instance = Activator.CreateInstance(fieldTypeInThemeConfig);
                if (instance is IConvertibleToBaseUtilityConfig convertible)
                {
                    // Call ConvertToBaseUtilityConfig on the instance
                    BaseUtilityConfig converted = convertible.ConvertToBaseUtilityConfig(displayValue, m_logger);

                    // Make sure the modifier variations list has no duplicates
                    var distinctModifierList = converted.modifierVariations.Distinct().ToList();
                    var distinctExtendFieldsList = converted.extendFields.Distinct().ToList();

                    if (distinctModifierList.Count != converted.modifierVariations.Count)
                    {
                        converted.modifierVariations.Clear();
                        converted.modifierVariations.AddRange(distinctModifierList);
                    }

                    if (distinctExtendFieldsList.Count != converted.extendFields.Count)
                    {
                        converted.extendFields.Clear();
                        converted.extendFields.AddRange(distinctExtendFieldsList);
                    }

                    return converted;
                }
            }


            return null;


        }



        private UtilityProperties ConvertUtilitiesDisplayVersionToUtilities(List<UtilityConfigDisplay> displayVersion)
        {
            var utilities = new UtilityProperties();

            var utilityFields = typeof(UtilityProperties).GetFields();
            // Convert the displayVersion list to a dictionary using the property name as the key
            Dictionary<string, UtilityConfigDisplay> displayVersionDictionary = new Dictionary<string, UtilityConfigDisplay>();

            foreach (var displayItem in displayVersion)
            {
                displayVersionDictionary.Add(displayItem.PropertyName, displayItem);
            }

            foreach (var field in utilityFields)
            {
                var fieldName = field.Name;

                if (!displayVersionDictionary.ContainsKey(fieldName))
                {
                    this.m_logger.LogWarning("No display value found for " + fieldName);
                    continue;
                }

                var displayValue = displayVersionDictionary[fieldName];

                if (displayValue == null)
                {
                    this.m_logger.Log("No display value found for " + fieldName);
                    continue;
                }

                var utilityConfig = ConvertUtilityConfigDisplayItemToUtilityConfig(field.FieldType, displayValue);

                if (utilityConfig == null)
                {
                    this.m_logger.LogWarning("Could not convert display value to utility config for field - " + field.Name);
                    continue;
                }

                // Add the converted utility config to the utilities object
                field.SetValue(utilities, utilityConfig);






            }

            return utilities;
        }

        public ThemeConfigDisplayVersion ConvertThemeConfigToDisplayVersion(ThemeConfig config)
        {
            var displayVersion = new ThemeConfigDisplayVersion(config.schemaVersion);

            //displayVersion.core = ConvertCoreToDisplayVersion(config.core);
            displayVersion.Core = ConvertSimplePropertiesToDisplayVersion(config.core, PropertyFormatter.PropertyExtensionContext.Core);

            displayVersion.Utilities = ConvertUtilitiesToDisplayVersion(config.utilities);

            displayVersion.Compilation = ConvertSimplePropertiesToDisplayVersion(config.compilation, PropertyFormatter.PropertyExtensionContext.Compilation);

            displayVersion.RequiredStringDropdownInfoInstance = new RequiredStringDropdownInfo
            {
                ValueTypeToFieldExtensionNamesList = new List<CorePropertyValueTypeToExtensibleFieldsMapItem>(),
            };

            // We convert the dictionary to a list so its easier to serialize for property drawers
            foreach (var entry in valueTypeToFieldExtensionNames)
            {

                displayVersion.RequiredStringDropdownInfoInstance.ValueTypeToFieldExtensionNamesList.Add(new CorePropertyValueTypeToExtensibleFieldsMapItem()
                {
                    ValueType = entry.Key,
                    ExtensibleFields = entry.Value,
                });
            }





            // We store the modifier property display separately so that we can easily access it in property drawers
            for (int i = 0; i < displayVersion.Core.Count; i++)
            {
                if (displayVersion.Core[i].PropertyName == nameof(config.core.modifiers))
                {
                    displayVersion.RequiredStringDropdownInfoInstance.ModifierPropertyDisplayPropertyName = displayVersion.Core[i].PropertyName;
                    break;
                }
            }

            return displayVersion;
        }

        public ThemeConfig ConvertThemeConfigDisplayVersionToThemeConfig(ThemeConfigDisplayVersion displayVersion)
        {
            var config = new ThemeConfig();

            config.core = ConvertDisplayVersionToSimpleProperties<BaseCoreProperties>(displayVersion.Core);
            config.utilities = ConvertUtilitiesDisplayVersionToUtilities(displayVersion.Utilities);
            config.compilation = ConvertDisplayVersionToSimpleProperties<CompilationConfig>(displayVersion.Compilation);

            return config;
        }
    }

}

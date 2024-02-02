using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEngine;
using ZoboUI.Core.Utils;


namespace ZoboUI.Editor
{



    /// <summary>
    /// Used to hold the values for the modifiers. e.g. hover, focus, child etc.
    /// </summary>
    [System.Serializable]
    public class ClassModifierDictionary : SerializableDictionary<string, ModifierValueHolder>, IConfigValue, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        /// <summary>
        /// The template string in a modifier value that will be replaced with the generated class name.
        /// </summary>
        public static readonly string GENERATED_CLASS_TEMPLATE_STRING = "{{generated_class}}";

        public List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string key = item.Key;
                ModifierValueHolder value = item.Value;

                ConfigValueResultItem resultItem = new ConfigValueResultItem();

                // This is the modifier key that will be preprended to the class tag. e.g. if the class tag is 'bg', the modifier value is 'hover', the separator is '_' and the key is 'red', the generated class name will be "hover_bg-red"
                resultItem.Key = key;
                resultItem.Value = value.value;

                requiredData.Add(resultItem);
            }

            return requiredData;
        }

        /// <summary>
        /// Generates the class name for the given modifier name and value.
        /// </summary>
        /// <param name="generatedClassName"></param>
        /// <param name="modifierSeparator"></param>
        /// <param name="modifierName"></param>
        /// <param name="valueHolder"></param>
        /// <returns></returns>
        public static string FormatClassNameWithModifier(string generatedClassName, string modifierSeparator, string modifierName, ModifierValueHolder valueHolder)
        {
            string modifierValue = valueHolder.value;

            // If the generated class name starts with a ., remove it
            if (generatedClassName.StartsWith("."))
            {
                generatedClassName = generatedClassName.Substring(1);
            }

            // First add the modifer name and the separator to the generated class name
            string formattedClassName = $".{modifierName}{modifierSeparator}{generatedClassName}";

            // Replace the template string in the modifer value with the formatted class name
            formattedClassName = modifierValue.Replace(GENERATED_CLASS_TEMPLATE_STRING, formattedClassName);


            return formattedClassName;
        }

        public bool IsSameValueType(IConfigValue other)
        {
            return other is ClassModifierDictionary;
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            ClassModifierConverter converter = new ClassModifierConverter(logger);

            var display = new BaseCorePropertyValueDisplay()
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };

            display.ValueType = CorePropertyValueType.ClassModifierDictionary;

            display.ValuesClassModifierDictionary = converter.ConvertToDisplay(this);

            return display;
        }

        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            ClassModifierConverter converter = new ClassModifierConverter(logger);

            return converter.ConvertFromDisplay(display.ValuesClassModifierDictionary);
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.ClassModifierDictionary;
        }
    }



    [System.Serializable]
    public class ClassModifierDisplay : IWithUniqueKey, IPropertyDisplayItemValue
    {
        public string ModifierName;


        public string Value;


        public string GetUniqueKey()
        {
            return ModifierName;
        }

        public static string GetUniqueKeyPropertyName()
        {
            return nameof(ModifierName);
        }

        public static string GetValuePropertyName()
        {
            return nameof(Value);
        }



        public void SetInitialValuesForNewItems()
        {
            ModifierName = "";
        }

    }
    public class ClassModifierConverter : IConvertibleToDisplay<List<ClassModifierDisplay>, ClassModifierDictionary>
    {

        private ICustomLogger customLogger;

        public ClassModifierConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }
        public List<ClassModifierDisplay> ConvertToDisplay(ClassModifierDictionary model)
        {
            var displayList = new List<ClassModifierDisplay>();

            foreach (var entry in model)
            {
                displayList.Add(new ClassModifierDisplay { ModifierName = entry.Key, Value = entry.Value.value });
            }

            return displayList;
        }

        public ClassModifierDictionary ConvertFromDisplay(List<ClassModifierDisplay> display)
        {
            var model = new ClassModifierDictionary();

            foreach (var displayItem in display)
            {
                ModifierValueHolder modifierValueHolder = new ModifierValueHolder
                {
                    value = displayItem.Value
                };
                model.Add(displayItem.ModifierName, modifierValueHolder);
            }

            return model;
        }
    }
}
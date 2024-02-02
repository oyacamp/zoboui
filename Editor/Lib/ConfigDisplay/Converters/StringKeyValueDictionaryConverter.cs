using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEngine;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor
{



    /// <summary>
    /// The version of the string key value pair that is used in json version of the config.
    /// </summary>
    [System.Serializable]
    public class KeyValueDictionary : DictionaryWithUssValueHolder<StringKeyValueHolder>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            StringKeyValueDictionaryConverter stringKeyValueDictionaryConverter = new StringKeyValueDictionaryConverter(logger);

            return stringKeyValueDictionaryConverter.ConvertFromDisplay(display.ValuesKeyValueDictionary);
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            StringKeyValueDictionaryConverter stringKeyValueDictionaryConverter = new StringKeyValueDictionaryConverter(logger);

            var display = new BaseCorePropertyValueDisplay
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };

            display.ValueType = CorePropertyValueType.KeyValueDictionary;
            display.ValuesKeyValueDictionary = stringKeyValueDictionaryConverter.ConvertToDisplay(this);

            return display;
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.KeyValueDictionary;
        }

        public override List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string key = item.Key;
                StringKeyValueHolder value = item.Value;

                ConfigValueResultItem resultItem = new ConfigValueResultItem();

                resultItem.Key = key;
                resultItem.Value = value.value;
                resultItem.UssDictionary = value.uss;

                requiredData.Add(resultItem);
            }

            return requiredData;

        }

        public override bool IsSameValueType(IConfigValue other)
        {
            return other is KeyValueDictionary;
        }
    }

    /// <summary>
    /// A representation of a string key value pair for the inspector.
    /// </summary>
    [System.Serializable]
    public class InspectorKeyValueDictionaryDisplay : DisplayWithCustomUssProperties, IPropertyDisplayItemValue, IWithUniqueKey
    {
        public string Key;

        public string Value;


        public void SetInitialValuesForNewItems()
        {
            Key = "newkey";
            Value = "";
        }

        public static string GetUniqueKeyPropertyName()
        {
            return nameof(Key);
        }

        public static string GetValuePropertyName()
        {
            return nameof(Value);
        }

        public string GetUniqueKey()
        {
            return Key;
        }
    }


    public class StringKeyValueDictionaryConverter : IConvertibleToDisplay<List<InspectorKeyValueDictionaryDisplay>, KeyValueDictionary>
    {
        private ICustomLogger customLogger;

        public StringKeyValueDictionaryConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }
        public List<InspectorKeyValueDictionaryDisplay> ConvertToDisplay(KeyValueDictionary model)
        {
            List<InspectorKeyValueDictionaryDisplay> displayList = new List<InspectorKeyValueDictionaryDisplay>();

            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();

            foreach (var entry in model)
            {
                List<UssPropertyHolderDisplay> ussPropertyHolder = ussDictionaryConverter.ConvertToDisplay(entry.Value.uss);

                displayList.Add(new InspectorKeyValueDictionaryDisplay { Key = entry.Key, Value = entry.Value.value, UssProperties = ussPropertyHolder });
            }


            return displayList;
        }

        public KeyValueDictionary ConvertFromDisplay(List<InspectorKeyValueDictionaryDisplay> display)
        {
            var model = new KeyValueDictionary();
            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();

            foreach (var displayItem in display)
            {
                StringKeyValueHolder stringValueHolder = new StringKeyValueHolder
                {
                    value = displayItem.Value,
                    uss = ussDictionaryConverter.ConvertFromDisplay(displayItem.UssProperties)
                };
                model[displayItem.Key] = stringValueHolder;
            }
            return model;
        }
    }
}
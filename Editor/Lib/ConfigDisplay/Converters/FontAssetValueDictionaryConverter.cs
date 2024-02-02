using System;
using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using ZoboUI.Core.Utils;


namespace ZoboUI.Editor
{


    /// <summary>
    /// The version of the string key value pair that is used in json version of the config.
    /// </summary>
    [System.Serializable]
    public class FontAssetDictionary : DictionaryWithUssValueHolder<FontAssetValueHolder>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            FontAssetValueDictionaryConverter converter = new FontAssetValueDictionaryConverter(logger);

            return converter.ConvertFromDisplay(display.ValuesFontAssetValueDictionary);
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            FontAssetValueDictionaryConverter converter = new FontAssetValueDictionaryConverter(logger);

            var display = new BaseCorePropertyValueDisplay
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };

            display.ValueType = CorePropertyValueType.FontAssetValueDictionary;

            display.ValuesFontAssetValueDictionary = converter.ConvertToDisplay(this);

            return display;
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.FontAssetValueDictionary;
        }

        public override List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string key = item.Key;
                FontAssetValueHolder value = item.Value;

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
            return other is FontAssetDictionary;
        }
    }

    [System.Serializable]
    public enum InspectorFontValueDisplayType
    {
        FontAsset = 0,

        Custom = 1,
    }

    [System.Serializable]
    public class InspectorFontAssetValueDictionaryDisplay : BaseInspectorDisplayWithAsset<FontAsset>
    {
        public InspectorFontValueDisplayType DisplayType = InspectorFontValueDisplayType.FontAsset;

        public override void SetInitialValuesForNewItems()
        {
            base.SetInitialValuesForNewItems();

            DisplayType = InspectorFontValueDisplayType.FontAsset;
        }

    }

    public class FontAssetValueDictionaryConverter : IConvertibleToDisplay<List<InspectorFontAssetValueDictionaryDisplay>, FontAssetDictionary>
    {
        private ICustomLogger customLogger;

        public FontAssetValueDictionaryConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }
        public List<InspectorFontAssetValueDictionaryDisplay> ConvertToDisplay(FontAssetDictionary model)
        {
            var displayList = new List<InspectorFontAssetValueDictionaryDisplay>();
            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();
            AssetValueValidator assetValueValidator = new AssetValueValidator();

            foreach (KeyValuePair<string, FontAssetValueHolder> entry in model)
            {
                string resourceValue = entry.Value.value;

                InspectorFontValueDisplayType displayTypeToSet = InspectorFontValueDisplayType.FontAsset;
                // load the font asset
                FontAsset fontAsset = null;

                if (!string.IsNullOrEmpty(resourceValue))
                {
                    (AssetValueValidator.AssetValueType, string) result = assetValueValidator.GetAssetValueTypeAndPath(entry.Value.value);
                    string path = result.Item2;
                    fontAsset = path == null ? null : AssetDatabase.LoadAssetAtPath<FontAsset>(path);

                    if (fontAsset == null)
                    {
                        displayTypeToSet = InspectorFontValueDisplayType.Custom;
                    }
                }

                var displayItem = new InspectorFontAssetValueDictionaryDisplay { Key = entry.Key, ValueObject = fontAsset, DisplayType = displayTypeToSet, UssProperties = ussDictionaryConverter.ConvertToDisplay(entry.Value.uss) };

                if (displayTypeToSet == InspectorFontValueDisplayType.Custom)
                {
                    displayItem.CustomStringValue = resourceValue;
                }

                displayList.Add(displayItem);
            }

            return displayList;
        }

        public FontAssetDictionary ConvertFromDisplay(List<InspectorFontAssetValueDictionaryDisplay> display)
        {
            var model = new FontAssetDictionary();
            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();
            AssetValueValidator assetValueValidator = new AssetValueValidator();

            foreach (var displayItem in display)
            {
                string valueToStore = String.Empty;
                if (displayItem.DisplayType == InspectorFontValueDisplayType.Custom)
                {
                    valueToStore = displayItem.CustomStringValue;
                }
                else if (displayItem.ValueObject != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(displayItem.ValueObject);
                    valueToStore = assetValueValidator.FormatAssetPathStringForStorage(assetPath);

                }


                FontAssetValueHolder fontAssetValueHolder = new FontAssetValueHolder
                {
                    value = valueToStore,
                    uss = ussDictionaryConverter.ConvertFromDisplay(displayItem.UssProperties)
                };
                model.Add(displayItem.Key, fontAssetValueHolder);
            }

            return model;
        }
    }

}
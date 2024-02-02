using System;
using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Core.Utils;


namespace ZoboUI.Editor
{


    [System.Serializable]
    public class StringImageDictionary : DictionaryWithUssValueHolder<ImageValueHolder>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            ImageValueDictionaryConverter converter = new ImageValueDictionaryConverter(logger);

            return converter.ConvertFromDisplay(display.ValuesImageValueDictionary);
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            ImageValueDictionaryConverter converter = new ImageValueDictionaryConverter(logger);

            var display = new BaseCorePropertyValueDisplay
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };
            display.ValueType = CorePropertyValueType.ImageValueDictionary;

            display.ValuesImageValueDictionary = converter.ConvertToDisplay(this);

            return display;
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.ImageValueDictionary;
        }

        public override List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string key = item.Key;
                ImageValueHolder value = item.Value;

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
            return other is StringImageDictionary;
        }
    }


    [System.Serializable]
    public enum InspectorImageValueDictionaryDisplayType
    {
        Texture2D = 0,
        Sprite = 1,

        RenderTexture = 2,

        //VectorImage = 3,

        Custom = 4,
    }

    [System.Serializable]
    public class InspectorImageValueDictionaryDisplay : BaseInspectorDisplayWithAsset<UnityEngine.Object>
    {
        public InspectorImageValueDictionaryDisplayType DisplayType = InspectorImageValueDictionaryDisplayType.Texture2D;

        public override void SetInitialValuesForNewItems()
        {
            base.SetInitialValuesForNewItems();

            DisplayType = InspectorImageValueDictionaryDisplayType.Texture2D;
        }

    }


    public class ImageValueDictionaryConverter : IConvertibleToDisplay<List<InspectorImageValueDictionaryDisplay>, StringImageDictionary>
    {
        private ICustomLogger customLogger;

        public ImageValueDictionaryConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }

        public (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) GetDisplayTypeAndAsset(UnityEngine.Object obj)
        {
            if (obj is Sprite)
            {
                return (InspectorImageValueDictionaryDisplayType.Sprite, obj);
            }
            else if (obj is Texture2D)
            {
                return (InspectorImageValueDictionaryDisplayType.Texture2D, obj);
            }
            else if (obj is RenderTexture)
            {
                return (InspectorImageValueDictionaryDisplayType.RenderTexture, obj);
            }
            /*
                        else if (obj is VectorImage)
                        {
                            return (InspectorImageValueDictionaryDisplayType.VectorImage, obj);
                        }*/
            else
            {
                return (InspectorImageValueDictionaryDisplayType.Custom, null);
            }
        }


        public (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) LoadAssetAtPath(string objectPath)
        {
            InspectorImageValueDictionaryDisplayType displayTypeToSet = InspectorImageValueDictionaryDisplayType.Texture2D;

            // We do this instead of AssetDatabase.LoadAssetAtPath because we want to be able to detect sprites vs textures
            // If we use AssetDatabase.LoadAssetAtPath, and try to use obj is Sprite, it will always return false, as we have to look for the sprite in the data array separately
            UnityEngine.Object[] data = AssetDatabase.LoadAllAssetsAtPath(objectPath);

            UnityEngine.Object asset = null;

            if (data != null)
            {
                foreach (UnityEngine.Object obj in data)
                {


                    (displayTypeToSet, asset) = GetDisplayTypeAndAsset(obj);

                }
            }


            // If there was an object path but no asset was found, set the valueType to Custom and initialize the custom value text field
            if (asset == null && !string.IsNullOrEmpty(objectPath))
            {
                displayTypeToSet = InspectorImageValueDictionaryDisplayType.Custom;
            }

            return (displayTypeToSet, asset);

        }
        public List<InspectorImageValueDictionaryDisplay> ConvertToDisplay(StringImageDictionary model)
        {
            var displayList = new List<InspectorImageValueDictionaryDisplay>();
            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();
            AssetValueValidator assetValueValidator = new AssetValueValidator();

            foreach (KeyValuePair<string, ImageValueHolder> entry in model)
            {

                var displayItem = new InspectorImageValueDictionaryDisplay { Key = entry.Key };

                UnityEngine.Object asset;
                InspectorImageValueDictionaryDisplayType displayTypeToSet;


                (AssetValueValidator.AssetValueType, string) result = assetValueValidator.GetAssetValueTypeAndPath(entry.Value.value);
                string path = result.Item2;
                (displayTypeToSet, asset) = LoadAssetAtPath(path);



                displayItem.DisplayType = displayTypeToSet;
                displayItem.ValueObject = asset;
                displayItem.CustomStringValue = displayTypeToSet == InspectorImageValueDictionaryDisplayType.Custom ? entry.Value.value : "";


                displayItem.UssProperties = ussDictionaryConverter.ConvertToDisplay(entry.Value.uss);


                displayList.Add(displayItem);
            }

            return displayList;
        }

        public StringImageDictionary ConvertFromDisplay(List<InspectorImageValueDictionaryDisplay> displayList)
        {
            var model = new StringImageDictionary();

            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();

            AssetValueValidator assetValueValidator = new AssetValueValidator();


            foreach (var displayItem in displayList)
            {

                string valueToStore = String.Empty;

                if (displayItem.DisplayType == InspectorImageValueDictionaryDisplayType.Custom)
                {
                    valueToStore = displayItem.CustomStringValue;
                }
                else if (displayItem.ValueObject != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(displayItem.ValueObject);
                    valueToStore = assetValueValidator.FormatAssetPathStringForStorage(assetPath);
                }



                ImageValueHolder imageValueHolder = new ImageValueHolder
                {
                    value = valueToStore,
                    uss = ussDictionaryConverter.ConvertFromDisplay(displayItem.UssProperties),
                };

                model.Add(displayItem.Key, imageValueHolder);
            }

            return model;
        }
    }
}
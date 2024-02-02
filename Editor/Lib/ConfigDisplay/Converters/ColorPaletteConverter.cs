using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ZoboUI.Core;
using UnityEngine;
using ZoboUI.Core.Utils;


namespace ZoboUI.Editor
{


    [System.Serializable]
    public class SwatchDictionary : SerializableDictionary<string, string> { }



    [System.Serializable]
    public class ColorPaletteDictionary : DictionaryWithUssValueHolder<ColorValueHolder>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            ColorPaletteConverter converter = new ColorPaletteConverter(logger);

            return converter.ConvertFromDisplay(display.ValuesColorPalette);
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            ColorPaletteConverter converter = new ColorPaletteConverter(logger);

            var display = new BaseCorePropertyValueDisplay
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };
            display.ValueType = CorePropertyValueType.ColorPalette;

            display.ValuesColorPalette = converter.ConvertToDisplay(this);

            return display;

        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.ColorPalette;
        }

        public override List<ConfigValueResultItem> GetRequiredConfigValuesForUssGeneration()
        {
            List<ConfigValueResultItem> requiredData = new List<ConfigValueResultItem>();

            foreach (var item in this)
            {
                string paletteName = item.Key;
                ColorValueHolder value = item.Value;
                // Colors are a bit different. We need to create a key for each color in the palette. e.g. if the palette is named "slate", we need to create a key for each color in the palette. e.g. "slate-50", "slate-100", "slate-200", etc.

                foreach (var color in value.value)
                {
                    string colorKey = ThemeUssGenerator.GenerateClassName(paletteName, color.Key, color.Key.Equals(ThemeUssGenerator.DEFAULT_VALUE_KEY_NAME));
                    string colorValue = color.Value;

                    ConfigValueResultItem resultItem = new ConfigValueResultItem();

                    resultItem.Key = colorKey;
                    resultItem.Value = colorValue;
                    resultItem.UssDictionary = value.uss;

                    requiredData.Add(resultItem);
                }


            }

            return requiredData;
        }


        public override bool IsSameValueType(IConfigValue other)
        {
            return other is ColorPaletteDictionary;
        }
    }


    // DISPLAYS



    public enum ColorStringType
    {
        Hex,
        RGB,
        RGBA,
        Custom
    }


    // COLORS
    [System.Serializable]
    public class InspectorColorSwatchDisplay
    {
        public string Name;

        public Color Color;

        public string CustomStringValue;

        public ColorStringType ColorStringType;

    }

    [System.Serializable]
    public class InspectorColorPaletteDisplay : DisplayWithCustomUssProperties, IPropertyDisplayItemValue, IWithUniqueKey
    {
        [Tooltip("The name of the palette. This will be used as the name of the generated class. e.g 'red' will will allow to use something like 'bg-red' as a class name.")]
        public string PaletteName;

        [Tooltip("The list of swatches in this palette. e.g '500' will allow to use something like 'bg-red-500' as a class name. If you want to use a particular swatch as the default, make sure to name it 'DEFAULT' so that it can be used without a swatch name e.g 'bg-red'")]
        public List<InspectorColorSwatchDisplay> Swatches;

        public string GetUniqueKey()
        {
            return PaletteName;
        }

        public static string GetUniqueKeyPropertyName()
        {
            return nameof(PaletteName);
        }

        public void SetInitialValuesForNewItems()
        {
            PaletteName = "newpalette";
            Swatches = new List<InspectorColorSwatchDisplay>();
        }
    }
    public class ColorPaletteConverter : IConvertibleToDisplay<List<InspectorColorPaletteDisplay>, ColorPaletteDictionary>
    {
        private ICustomLogger customLogger;

        public ColorPaletteConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }
        public ColorStringType GetColorStringType(string colorString)
        {
            // We need to check if it is a HEX color string or an RGB color string
            // If it is a HEX color string, we need to convert it to an RGB color string
            // If it is an RGB color string, we need to convert it to a HEX color string

            // Remove spaces at the start and end of the string
            colorString = colorString.Trim();

            // If the string starts with a #, it is a HEX color string
            if (colorString.StartsWith("#"))
            {
                return ColorStringType.Hex;
            }
            else
            {
                // If the string starts with a rgba or rgb, it is an RGB color string

                // If the string starts with rgba, it is an RGBA color string
                if (colorString.StartsWith("rgba("))
                {
                    return ColorStringType.RGBA;
                }
                else if (colorString.StartsWith("rgb("))
                {
                    // If the string starts with rgb, it is an RGB color string
                    return ColorStringType.RGB;
                }
                else
                {
                    // If the string doesn't start with a #, rgba or rgb, it is a custom color string
                    return ColorStringType.Custom;
                }
            }
        }

        public Color ConvertRGBToColor(string colorString)
        {
            string pattern = @"rgb\((\d+),\s*(\d+),\s*(\d+)\)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(colorString);

            // Parse and normalize RGB values to the range of 0 to 1 as required by Unity's Color struct
            if (match.Success)
            {
                float r = float.Parse(match.Groups[1].Value) / 255;
                float g = float.Parse(match.Groups[2].Value) / 255;
                float b = float.Parse(match.Groups[3].Value) / 255;

                // Alpha is set to 1 for full opacity
                return new Color(r, g, b, 1);
            }
            else
            {
                throw new ArgumentException("Invalid color string format");
            }
        }

        public Color ConvertRGBAToColor(string colorString)
        {
            string pattern = @"rgba\((\d+),\s*(\d+),\s*(\d+),\s*(\d+(\.\d+)?)\)";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(colorString);

            if (match.Success)
            {
                // Parse and normalize RGB values to the range of 0 to 1 as required by Unity's Color struct
                float r = float.Parse(match.Groups[1].Value) / 255;
                float g = float.Parse(match.Groups[2].Value) / 255;
                float b = float.Parse(match.Groups[3].Value) / 255;

                // Alpha value in RGBA format is already normalized (between 0 and 1), no need to divide by 255
                float a = float.Parse(match.Groups[4].Value);

                return new Color(r, g, b, a);
            }
            else
            {
                throw new ArgumentException("Invalid color string format");
            }
        }

        public Color ConvertHexStringToColor(string colorString)
        {
            Color color = new Color();
            if (ColorUtility.TryParseHtmlString(colorString, out color))
            {
                return color;
            }
            else
            {
                throw new Exception("Invalid color string");
            }
        }

        /// <summary>
        /// Converts a color to a string representation. If the color doesn't have transparency, it will return a hex string. If it does have transparency, it will return an rgba string e.g. rgba(255, 255, 255, 0.5) 
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public string ConvertColorToStringRepresentation(Color color)
        {
            // If the color is fully opaque, return an RGB hex code
            if (color.a == 1)
            {
                return "#" + ColorUtility.ToHtmlStringRGB(color);
            }

            // Calculate alpha, rounding to 3 decimal places
            float alpha = (float)Math.Round(color.a, 3);

            // Return a rgba() string for colors with transparency
            // We multiply the color values by 255 because the Color struct stores color values between 0 and 1, but USS rgba() values are between 0 and 255
            return $"rgba({color.r * 255}, {color.g * 255}, {color.b * 255}, {alpha})";
        }

        public List<InspectorColorPaletteDisplay> ConvertToDisplay(ColorPaletteDictionary model)
        {
            var displayList = new List<InspectorColorPaletteDisplay>();

            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();


            foreach (KeyValuePair<string, ColorValueHolder> entry in model)
            {
                var displayItem = new InspectorColorPaletteDisplay { PaletteName = entry.Key, UssProperties = ussDictionaryConverter.ConvertToDisplay(entry.Value.uss) };

                var swatchDisplayList = new List<InspectorColorSwatchDisplay>();

                foreach (KeyValuePair<string, string> swatchEntry in entry.Value.value)
                {
                    Color colorValue = Color.white;

                    // Get the color string type
                    ColorStringType colorStringType = GetColorStringType(swatchEntry.Value);
                    string customStringValue = "";

                    // If the color string type is HEX, convert it to a color
                    switch (colorStringType)
                    {
                        case ColorStringType.Hex:
                            colorValue = ConvertHexStringToColor(swatchEntry.Value);
                            break;
                        case ColorStringType.RGB:
                            colorValue = ConvertRGBToColor(swatchEntry.Value);
                            break;
                        case ColorStringType.RGBA:
                            colorValue = ConvertRGBAToColor(swatchEntry.Value);
                            break;
                        case ColorStringType.Custom:
                            customStringValue = swatchEntry.Value;
                            break;
                        default:
                            throw new Exception("Invalid color string type");
                    }

                    swatchDisplayList.Add(new InspectorColorSwatchDisplay { Name = swatchEntry.Key, Color = colorValue, CustomStringValue = customStringValue, ColorStringType = colorStringType });

                }

                displayItem.Swatches = swatchDisplayList;

                displayList.Add(displayItem);
            }


            return displayList;
        }

        public ColorPaletteDictionary ConvertFromDisplay(List<InspectorColorPaletteDisplay> display)
        {
            var colorPaletteDict = new ColorPaletteDictionary();
            USSDictionaryConverter ussDictionaryConverter = new USSDictionaryConverter();

            foreach (var displayItem in display)
            {
                SwatchDictionary swatchDictionary = new SwatchDictionary();

                foreach (var swatchDisplayItem in displayItem.Swatches)
                {
                    string colorString = swatchDisplayItem.ColorStringType == ColorStringType.Custom ? swatchDisplayItem.CustomStringValue : ConvertColorToStringRepresentation(swatchDisplayItem.Color);

                    swatchDictionary[swatchDisplayItem.Name] = colorString;
                }

                ColorValueHolder colorValueHolder = new ColorValueHolder
                {
                    value = swatchDictionary,
                    uss = ussDictionaryConverter.ConvertFromDisplay(displayItem.UssProperties)

                };


                colorPaletteDict[displayItem.PaletteName] = colorValueHolder;
            }

            return colorPaletteDict;


        }
    }
}
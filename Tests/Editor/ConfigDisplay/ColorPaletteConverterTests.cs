using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEngine;

namespace ZoboUI.Editor.Tests
{

    public class ColorPaletteConverterTests
    {

        ColorPaletteConverter converter;
        [SetUp]
        public void SetUp()
        {
            converter = new ColorPaletteConverter();
        }
        // CONVERTERS
        [Test]
        public void GetColorStringType_ReturnsCorrectColorStringType()
        {
            string hexColorString = "#FFFFFF";
            string rgbColorString = "rgb(255, 255, 255)";
            string rgbaColorString = "rgba(255, 255, 255, 0.4)";
            string customColorString = "customColor";

            // This checks if the converter can handle spaces at the start of the string
            string rgbColorStringWithSpacesAtStart = "  rgb(255, 255, 255)";
            string hexColorStringWithSpacesAtStartEnd = "  #FFFFFF  ";

            ColorStringType hexResult = converter.GetColorStringType(hexColorString);
            ColorStringType rgbResult = converter.GetColorStringType(rgbColorString);
            ColorStringType rgbaResult = converter.GetColorStringType(rgbaColorString);
            ColorStringType customResult = converter.GetColorStringType(customColorString);

            ColorStringType rgbResultWithSpacesAtStart = converter.GetColorStringType(rgbColorStringWithSpacesAtStart);
            ColorStringType hexResultWithSpacesAtStartEnd = converter.GetColorStringType(hexColorStringWithSpacesAtStartEnd);

            Assert.AreEqual(ColorStringType.Hex, hexResult);
            Assert.AreEqual(ColorStringType.RGB, rgbResult);
            Assert.AreEqual(ColorStringType.RGBA, rgbaResult);
            Assert.AreEqual(ColorStringType.Custom, customResult);

            Assert.AreEqual(ColorStringType.RGB, rgbResultWithSpacesAtStart);
            Assert.AreEqual(ColorStringType.Hex, hexResultWithSpacesAtStartEnd);
        }

        [Test]
        public void ConvertRGBToColor_ReturnsCorrectColor()
        {
            string whiteRgbColorString = "rgb(255, 255, 255)";
            Color expectedWhiteColor = new Color(1, 1, 1, 1);

            string greenRgbColorString = "rgb(0, 255, 0)";
            Color expectedGreenColor = new Color(0, 1, 0, 1);


            Color whiteResult = converter.ConvertRGBToColor(whiteRgbColorString);
            Color greenResult = converter.ConvertRGBToColor(greenRgbColorString);

            Assert.AreEqual(expectedWhiteColor, whiteResult);
            Assert.AreEqual(expectedGreenColor, greenResult);
        }

        public void ConvertRGBAToColor_ReturnsCorrectColor()
        {
            string whiteRgbaColorString = "rgba(255, 255, 255, 1)";
            Color expectedWhiteColor = new Color(1, 1, 1, 1);

            string greenRgbaColorString = "rgba(0, 255, 0, 0.5)";
            Color expectedGreenColor = new Color(0, 1, 0, 0.5f);

            Assert.AreEqual(expectedWhiteColor, converter.ConvertRGBAToColor(whiteRgbaColorString));
            Assert.AreEqual(expectedGreenColor, converter.ConvertRGBAToColor(greenRgbaColorString));
        }

        [Test]
        public void ConvertHexStringToColor_ReturnsCorrectColor_WhenValidHexStringProvided()
        {
            string hexColorString = "#FFFFFF";
            Color expectedColor = new Color(1, 1, 1); // Assuming Color values are normalized between 0 and 1

            Color result = converter.ConvertHexStringToColor(hexColorString);

            Assert.AreEqual(expectedColor, result);
        }

        [Test]
        public void ConvertHexStringToColor_ThrowsException_WhenInvalidHexStringProvided()
        {
            string invalidColorString = "#ZZZZZZ";

            Assert.Throws<Exception>(() => converter.ConvertHexStringToColor(invalidColorString));
        }

        [Test]
        public void ConvertColorToStringRepresentation_ReturnsHex_WhenAlphaIsOne()
        {
            Color color = new Color(1, 1, 1, 1); // Assuming normalized color values
            string expected = "#FFFFFF";

            string result = converter.ConvertColorToStringRepresentation(color);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ConvertColorToStringRepresentation_ReturnsRgba_WhenAlphaIsLessThanOne()
        {
            Color color = new Color(1, 1, 1, 0.5f); // Assuming normalized color values
            string expected = "rgba(255, 255, 255, 0.5)";

            string result = converter.ConvertColorToStringRepresentation(color);

            Assert.AreEqual(expected, result);
        }



        [Test]
        public void ConvertToDisplay_ReturnsCorrectDisplayList()
        {
            ColorPaletteDictionary model = new ColorPaletteDictionary();

            string colorString = "rgba(0, 255, 0, 0.5)";
            Color colorValue = new Color(0, 1, 0, 0.5f);
            string paletteName = "mycolor";
            string swatchName = "200";

            ColorValueHolder colorValueHolder = new ColorValueHolder();
            colorValueHolder.value = new SwatchDictionary() { { swatchName, colorString } };
            model.Add(paletteName, colorValueHolder);


            int expectedCount = model.Count;

            List<InspectorColorPaletteDisplay> result = converter.ConvertToDisplay(model);

            Assert.AreEqual(expectedCount, result.Count);

            // Assert that the result contains the correct data
            Assert.AreEqual(paletteName, result[0].PaletteName);
            Assert.AreEqual(swatchName, result[0].Swatches[0].Name);
            Assert.AreEqual(colorValue, result[0].Swatches[0].Color);

        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectDictionary()
        {
            List<InspectorColorPaletteDisplay> display = new List<InspectorColorPaletteDisplay>();


            string colorString = "rgba(0, 255, 0, 0.5)";
            Color colorValue = new Color(0, 1, 0, 0.5f);
            string paletteName = "mycolor";
            string swatchName = "200";

            display.Add(new InspectorColorPaletteDisplay()
            {
                PaletteName = paletteName,
                Swatches = new List<InspectorColorSwatchDisplay>(){
            new InspectorColorSwatchDisplay(){
                Name = swatchName,
                Color = colorValue
            }
        }
            });

            int expectedCount = display.Count;

            ColorPaletteDictionary result = converter.ConvertFromDisplay(display);

            Assert.AreEqual(expectedCount, result.Count);

            Assert.IsTrue(result[paletteName].value.ContainsKey(swatchName));
            Assert.AreEqual(colorString, result[paletteName].value[swatchName]);

        }

        [Test]
        public void ConvertFromDisplay_ReturnsCustomValue_BackAndForth()
        {
            ColorPaletteDictionary model = new ColorPaletteDictionary();

            string colorString = "--mycustomcolorvariable";
            string paletteName = "mycustomcolor";
            string swatchName = "200";

            ColorValueHolder colorValueHolder = new ColorValueHolder();
            colorValueHolder.value = new SwatchDictionary() { { swatchName, colorString } };
            model.Add(paletteName, colorValueHolder);


            int expectedCount = model.Count;

            List<InspectorColorPaletteDisplay> result = converter.ConvertToDisplay(model);

            Assert.AreEqual(expectedCount, result.Count);

            // Assert that the result contains the correct data
            Assert.AreEqual(paletteName, result[0].PaletteName);
            Assert.AreEqual(swatchName, result[0].Swatches[0].Name);
            Assert.AreEqual(colorString, result[0].Swatches[0].CustomStringValue);
        }

    }

}

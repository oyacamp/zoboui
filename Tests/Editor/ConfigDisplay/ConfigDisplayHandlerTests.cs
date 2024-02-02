using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.Tests
{

    [TestFixture]
    public class ConfigDisplayHandlerTests
    {
        private ConfigDisplayHandler handler;

        [SetUp]
        public void SetUp()
        {
            handler = new ConfigDisplayHandler();
        }

        [Test]
        public void ConvertUtilityConfigDisplayItemToUtilityConfig_ReturnsCorrectType_WhenFieldTypeIsConvertible()
        {
            var displayValue = new UtilityConfigDisplay()
            {
                ExtendFields = new List<ExtendFieldsStringDropdownFieldValue> { new ExtendFieldsStringDropdownFieldValue(){
                    Value = "core.breakpoints"
                } },
                ClassTagToUssPropertyMap = new List<ClassTagToUssPropertyMapDisplay> { new ClassTagToUssPropertyMapDisplay(){
                    ClassTag = "TestTag",
                    UssPropertyNames = new TextFieldDataList {Values = new List<TextFieldStringData>(){ new TextFieldStringData { Value = "property1" }, new TextFieldStringData { Value = "property2" } }}
                } },
                Enabled = false,
                ModifierVariations = new List<ClassModifierStringDropdownFieldValue> { new ClassModifierStringDropdownFieldValue(){
                    Value = "TestValue"
                } },
                PropertyDisplay = new BaseCorePropertyValueDisplay
                {
                    PropertyName = "PropertyName",
                    ValueType = CorePropertyValueType.KeyValueDictionary,
                    ValuesKeyValueDictionary = new List<InspectorKeyValueDictionaryDisplay> { new InspectorKeyValueDictionaryDisplay(){
                    Key = "TestDataKey",
                    Value = "TestDataValue"
                } },

                }
            };



            Type fieldType = typeof(UtilityConfigWithStringDictionary);

            BaseUtilityConfig result = handler.ConvertUtilityConfigDisplayItemToUtilityConfig(fieldType, displayValue);



            Assert.IsInstanceOf<BaseUtilityConfig>(result);

            Assert.IsInstanceOf<UtilityConfigWithStringDictionary>(result);

            UtilityConfigWithStringDictionary utilityConfigWithStringDictionary = result as UtilityConfigWithStringDictionary;

            Assert.IsNotNull(utilityConfigWithStringDictionary);

            Assert.AreEqual(displayValue.Enabled, result.enabled);

            Assert.AreEqual(displayValue.ModifierVariations.Count, result.modifierVariations.Count);

            Assert.AreEqual(displayValue.ModifierVariations[0].Value, result.modifierVariations[0]);

            Assert.AreEqual(displayValue.ExtendFields.Count, result.extendFields.Count);

            Assert.AreEqual(displayValue.ExtendFields[0].Value, result.extendFields[0]);

            Assert.AreEqual(displayValue.ClassTagToUssPropertyMap.Count, result.tagPropertyMap.Count);

            Assert.AreEqual("property1", result.tagPropertyMap[displayValue.ClassTagToUssPropertyMap[0].ClassTag].properties[0]);

            // Check that the data is correctly converted as well
            Assert.AreEqual("TestDataValue", utilityConfigWithStringDictionary.data["TestDataKey"].value);


        }

        [Test]
        public void ConvertUtilityConfigDisplayItemToUtilityConfig_ReturnsNull_WhenFieldTypeIsNotConvertible()
        {
            var displayValue = new UtilityConfigDisplay()
            {
                ExtendFields = new List<ExtendFieldsStringDropdownFieldValue> { new ExtendFieldsStringDropdownFieldValue() },
                ClassTagToUssPropertyMap = new List<ClassTagToUssPropertyMapDisplay> { new ClassTagToUssPropertyMapDisplay() },
                Enabled = false,
                ModifierVariations = new List<ClassModifierStringDropdownFieldValue> { new ClassModifierStringDropdownFieldValue() },
                PropertyDisplay = new BaseCorePropertyValueDisplay
                {
                    PropertyName = "PropertyName",
                    ValueType = CorePropertyValueType.KeyValueDictionary,
                    ValuesKeyValueDictionary = new List<InspectorKeyValueDictionaryDisplay> { new InspectorKeyValueDictionaryDisplay(){
                    Key = "Key",
                    Value = "Value",


                } },

                }
            };

            Type fieldType = typeof(UtilityConfigDisplay);

            BaseUtilityConfig result = handler.ConvertUtilityConfigDisplayItemToUtilityConfig(fieldType, displayValue);

            Assert.IsNull(result);

            Assert.IsTrue(displayValue.PropertyDisplay.ValueType == CorePropertyValueType.KeyValueDictionary);


        }

        [Test]
        public void ConvertUtilityConfigDisplayItemToUtilityConfig_ConvertsToThemeConfigAndBack()
        {
            string loadedText = File.ReadAllText(ThemeConfigTests.testJsonFilePath);
            var testConfig = ThemeConfig.FromJson(loadedText);

            Assert.IsNotNull(testConfig.utilities.backgroundImage);

            ConfigDisplayHandler handler = new ConfigDisplayHandler();

            ThemeConfigDisplayVersion themeConfigDisplayVersion = handler.ConvertThemeConfigToDisplayVersion(testConfig);

            ThemeConfig reconvertedThemeConfig = handler.ConvertThemeConfigDisplayVersionToThemeConfig(themeConfigDisplayVersion);



            Assert.Greater(themeConfigDisplayVersion.Utilities.Count, 0);

            // Check that all the base properties have a display name

            foreach (var item in themeConfigDisplayVersion.Core)
            {
                Assert.IsNotNull(item.PropertyName);

                Assert.IsNotNull(item.DisplayName);
            }
            foreach (var item in themeConfigDisplayVersion.Utilities)
            {
                Assert.IsNotNull(item.PropertyDisplay.PropertyName);

                Assert.IsNotNull(item.PropertyDisplay.DisplayName);

            }

            Assert.Greater(themeConfigDisplayVersion.Core.Count, 0);

            Assert.Greater(themeConfigDisplayVersion.Compilation.Count, 0);


            Assert.IsTrue(ThemeConfigTests.AreThemeConfigsEqual(testConfig, reconvertedThemeConfig));




        }

    }
}

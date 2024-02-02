using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TextCore.Text;


namespace ZoboUI.Editor.Tests
{




    public class FontAssetValueDictionaryConverterTests
    {
        private FontAssetValueDictionaryConverter converter;

        private readonly string fontAssetPath = "Packages/com.oyacamp.zoboui/Tests/Editor/Assets/Fonts/LiberationSans SDF.asset";

        // These are the values that are stored in the format expected by the themeconfig or USS file

        string storedFontAssetPath { get { return $"url('project://database/{fontAssetPath}')"; } }


        [SetUp]
        public void SetUp()
        {
            converter = new FontAssetValueDictionaryConverter();
        }


        [Test]
        public void ConvertToDisplay_ReturnsCorrectDisplayList()
        {
            var model = new FontAssetDictionary
        {
            { "customValueKey", new FontAssetValueHolder { value = "path1", uss = new USSPropertyToValueDictionary() } },
            { "fontKey", new FontAssetValueHolder { value = storedFontAssetPath,  uss = new USSPropertyToValueDictionary() } },
        };

            var result = converter.ConvertToDisplay(model);

            Assert.AreEqual(model.Count, result.Count);
            Assert.AreEqual("customValueKey", result[0].Key);
            Assert.AreEqual("fontKey", result[1].Key);

            Assert.AreEqual(InspectorFontValueDisplayType.Custom, result[0].DisplayType);
            Assert.AreEqual(InspectorFontValueDisplayType.FontAsset, result[1].DisplayType);



            Assert.AreEqual("path1", result[0].CustomStringValue);
            Assert.AreEqual(fontAssetPath, AssetDatabase.GetAssetPath(result[1].ValueObject));

        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectModel()
        {
            string customValueKey = "customValueKey";
            string fontKey = "fontKey";




            var displayList = new List<InspectorFontAssetValueDictionaryDisplay>
        {
            new InspectorFontAssetValueDictionaryDisplay { Key = customValueKey, DisplayType= InspectorFontValueDisplayType.Custom, CustomStringValue = "path1", UssProperties = new List<UssPropertyHolderDisplay>() },
            new InspectorFontAssetValueDictionaryDisplay { Key = fontKey, DisplayType= InspectorFontValueDisplayType.FontAsset, ValueObject = AssetDatabase.LoadAssetAtPath(fontAssetPath, typeof(FontAsset)), UssProperties = new List<UssPropertyHolderDisplay>() },

        };

            var result = converter.ConvertFromDisplay(displayList);

            Assert.AreEqual(displayList.Count, result.Count);
            Assert.IsTrue(result.ContainsKey(customValueKey));
            Assert.IsTrue(result.ContainsKey(fontKey));

            Assert.AreEqual("path1", result[customValueKey].value);
            Assert.AreEqual(storedFontAssetPath, result[fontKey].value);


        }


    }
}
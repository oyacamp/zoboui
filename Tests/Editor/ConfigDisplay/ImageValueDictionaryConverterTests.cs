using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;


namespace ZoboUI.Editor.Tests
{




    public class ImageValueDictionaryConverterTests
    {
        private ImageValueDictionaryConverter converter;

        private readonly string spriteIconPath = "Packages/com.oyacamp.zoboui/Tests/Editor/Assets/packageicon.png";
        private readonly string texture2dPath = "Packages/com.oyacamp.zoboui/Tests/Editor/Assets/packagetexture.jpg";

        private readonly string renderTexturePath = "Packages/com.oyacamp.zoboui/Tests/Editor/Assets/testrendertexture.renderTexture";

        //private readonly string vectorImagePath = "Packages/com.oyacamp.zoboui/Tests/Editor/Assets/packagevectorimage.svg";


        // These are the values that are stored in the format expected by the themeconfig or USS file
        string storedSpriteIconPath { get { return $"url('project://database/{spriteIconPath}')"; } }
        string storedTexture2dPath { get { return $"url('project://database/{texture2dPath}')"; } }
        string storedRenderTexturePath { get { return $"url('project://database/{renderTexturePath}')"; } }
        //string storedVectorImagePath { get { return $"url('project://database/{vectorImagePath}')"; } }


        [SetUp]
        public void SetUp()
        {
            converter = new ImageValueDictionaryConverter();
        }

        [Test]
        public void LoadAssetAtPath_ReturnsCorrectValueAndTypeForSprite()
        {

            (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) result = converter.LoadAssetAtPath(spriteIconPath);

            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.Sprite, result.Item1);
            Assert.AreEqual(typeof(Sprite), result.Item2.GetType());
            Assert.IsNotNull(result.Item2);

        }

        [Test]
        public void LoadAssetAtPath_ReturnsCorrectValueAndTypeForTexture2D()
        {

            (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) result = converter.LoadAssetAtPath(texture2dPath);

            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.Texture2D, result.Item1);
            Assert.AreEqual(typeof(Texture2D), result.Item2.GetType());
            Assert.IsNotNull(result.Item2);

        }

        [Test]
        public void LoadAssetAtPath_ReturnsCorrectValueAndTypeForRenderTexture()
        {

            (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) result = converter.LoadAssetAtPath(renderTexturePath);

            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.RenderTexture, result.Item1);
            Assert.AreEqual(typeof(RenderTexture), result.Item2.GetType());
            Assert.IsNotNull(result.Item2);

        }
        /*
                [Test]
                public void LoadAssetAtPath_ReturnsCorrectValueAndTypeForVectorImage()
                {

                    (InspectorImageValueDictionaryDisplayType, UnityEngine.Object) result = converter.LoadAssetAtPath(vectorImagePath);


                    Assert.AreEqual(InspectorImageValueDictionaryDisplayType.VectorImage, result.Item1);
                    Assert.AreEqual(typeof(VectorImage), result.Item2.GetType());
                    Assert.IsNotNull(result.Item2);

                }*/

        [Test]
        public void ConvertToDisplay_ReturnsCorrectDisplayList()
        {
            var model = new StringImageDictionary
        {
            { "customValueKey", new ImageValueHolder { value = "path1", uss = new USSPropertyToValueDictionary() } },
            { "spriteKey", new ImageValueHolder { value = storedSpriteIconPath,  uss = new USSPropertyToValueDictionary() } },
            { "texture2dKey", new ImageValueHolder { value = storedTexture2dPath,  uss = new USSPropertyToValueDictionary() } },
            { "renderTextureKey", new ImageValueHolder { value = storedRenderTexturePath,  uss = new USSPropertyToValueDictionary() } },
            //{ "vectorImageKey", new ImageValueHolder { value = storedVectorImagePath,  uss = new USSPropertyToValueDictionary() } }
        };

            var result = converter.ConvertToDisplay(model);

            Assert.AreEqual(model.Count, result.Count);
            Assert.AreEqual("customValueKey", result[0].Key);
            Assert.AreEqual("spriteKey", result[1].Key);
            Assert.AreEqual("texture2dKey", result[2].Key);
            Assert.AreEqual("renderTextureKey", result[3].Key);
            //Assert.AreEqual("vectorImageKey", result[4].Key);

            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.Custom, result[0].DisplayType);
            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.Sprite, result[1].DisplayType);
            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.Texture2D, result[2].DisplayType);
            Assert.AreEqual(InspectorImageValueDictionaryDisplayType.RenderTexture, result[3].DisplayType);
            //Assert.AreEqual(InspectorImageValueDictionaryDisplayType.VectorImage, result[4].DisplayType);


            Assert.AreEqual("path1", result[0].CustomStringValue);
            Assert.AreEqual(spriteIconPath, AssetDatabase.GetAssetPath(result[1].ValueObject));
            Assert.AreEqual(texture2dPath, AssetDatabase.GetAssetPath(result[2].ValueObject));
            Assert.AreEqual(renderTexturePath, AssetDatabase.GetAssetPath(result[3].ValueObject));
            //Assert.AreEqual(vectorImagePath, AssetDatabase.GetAssetPath(result[4].ValueObject));

        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectModel()
        {
            string customValueKey = "customValueKey";
            string spriteKey = "spriteKey";
            string texture2dKey = "texture2dKey";
            string renderTextureKey = "renderTextureKey";
            //string vectorImageKey = "vectorImageKey";




            var displayList = new List<InspectorImageValueDictionaryDisplay>
        {
            new InspectorImageValueDictionaryDisplay { Key = customValueKey, DisplayType= InspectorImageValueDictionaryDisplayType.Custom, CustomStringValue = "path1", UssProperties = new List<UssPropertyHolderDisplay>() },
            new InspectorImageValueDictionaryDisplay { Key = spriteKey, DisplayType= InspectorImageValueDictionaryDisplayType.Sprite, ValueObject = AssetDatabase.LoadAssetAtPath(spriteIconPath, typeof(Sprite)), UssProperties = new List<UssPropertyHolderDisplay>() },
            new InspectorImageValueDictionaryDisplay { Key = texture2dKey, DisplayType= InspectorImageValueDictionaryDisplayType.Texture2D, ValueObject = AssetDatabase.LoadAssetAtPath(texture2dPath, typeof(Texture2D)), UssProperties = new List<UssPropertyHolderDisplay>() },
            new InspectorImageValueDictionaryDisplay { Key = renderTextureKey, DisplayType= InspectorImageValueDictionaryDisplayType.RenderTexture, ValueObject = AssetDatabase.LoadAssetAtPath(renderTexturePath, typeof(RenderTexture)), UssProperties = new List<UssPropertyHolderDisplay>() },
            //new InspectorImageValueDictionaryDisplay { Key = vectorImageKey, DisplayType= InspectorImageValueDictionaryDisplayType.VectorImage, ValueObject = AssetDatabase.LoadAssetAtPath(vectorImagePath, typeof(VectorImage)), UssProperties = new List<UssPropertyHolderDisplay>() }

        };

            var result = converter.ConvertFromDisplay(displayList);

            Assert.AreEqual(displayList.Count, result.Count);
            Assert.IsTrue(result.ContainsKey(customValueKey));
            Assert.IsTrue(result.ContainsKey(spriteKey));
            Assert.IsTrue(result.ContainsKey(texture2dKey));
            Assert.IsTrue(result.ContainsKey(renderTextureKey));
            //Assert.IsTrue(result.ContainsKey(vectorImageKey));

            Assert.AreEqual("path1", result[customValueKey].value);
            Assert.AreEqual(storedSpriteIconPath, result[spriteKey].value);
            Assert.AreEqual(storedTexture2dPath, result[texture2dKey].value);
            Assert.AreEqual(storedRenderTexturePath, result[renderTextureKey].value);
            //Assert.AreEqual(storedVectorImagePath, result[vectorImageKey].value);


        }


    }
}
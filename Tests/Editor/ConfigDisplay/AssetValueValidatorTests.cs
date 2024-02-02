using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ZoboUI.Editor.Tests
{



    [TestFixture]
    public class AssetValueValidatorTests
    {
        private AssetValueValidator validator;

        [SetUp]
        public void SetUp()
        {
            validator = new AssetValueValidator();
        }

        [Test]
        public void GetAssetValueTypeAndPath_ReturnsCorrectTypeAndPath_WhenAssetValueIsUrl()
        {
            string assetValueWithSingleQuotes = "url('path/to/asset')";
            string assetValueWithDoubleQuotes = "url(\"path/to/asset\")";

            var (singleQuotesType, singleQuotesPath) = validator.GetAssetValueTypeAndPath(assetValueWithSingleQuotes);
            var (doubleQuotesType, doubleQuotesPath) = validator.GetAssetValueTypeAndPath(assetValueWithDoubleQuotes);


            string expectedPath = "path/to/asset";

            Assert.AreEqual(AssetValueValidator.AssetValueType.Url, singleQuotesType);
            Assert.AreEqual(expectedPath, singleQuotesPath);

            Assert.AreEqual(AssetValueValidator.AssetValueType.Url, doubleQuotesType);
            Assert.AreEqual(expectedPath, doubleQuotesPath);
        }

        [Test]
        public void GetAssetValueTypeAndPath_ReturnsCorrectTypeAndPath_WhenAssetValueIsResource()
        {
            string assetValueWithSingleQuotes = "resource('path/to/asset')";
            string assetValueWithDoubleQuotes = "resource(\"path/to/asset\")";

            var (singleQuotesType, singleQuotesPath) = validator.GetAssetValueTypeAndPath(assetValueWithSingleQuotes);
            var (doubleQuotesType, doubleQuotesPath) = validator.GetAssetValueTypeAndPath(assetValueWithDoubleQuotes);

            string expectedPath = "path/to/asset";

            Assert.AreEqual(AssetValueValidator.AssetValueType.Resource, singleQuotesType);
            Assert.AreEqual(expectedPath, singleQuotesPath);

            Assert.AreEqual(AssetValueValidator.AssetValueType.Resource, doubleQuotesType);
            Assert.AreEqual(expectedPath, doubleQuotesPath);

        }

        [Test]
        public void GetAssetValueTypeAndPath_ReturnsCustomTypeAndOriginalValue_WhenAssetValueDoesNotMatchPattern()
        {
            string assetValue = "customValue";

            var (type, path) = validator.GetAssetValueTypeAndPath(assetValue);

            Assert.AreEqual(AssetValueValidator.AssetValueType.None, type);
            Assert.AreEqual("customValue", path);
        }

        [Test]
        public void GetAssetValueTypeAndPath_ReturnsCustomTypeAndEmptyString_WhenAssetValueIsEmpty()
        {
            string assetValue = "";

            var (type, path) = validator.GetAssetValueTypeAndPath(assetValue);

            Assert.AreEqual(AssetValueValidator.AssetValueType.None, type);
            Assert.AreEqual("", path);
        }

        [Test]
        public void FormatAssetPathStringForStorage_FormatsUrlCorrectly()
        {
            string pathWithStartingSlash = "/Assets/Editor/thumb.png";
            string pathWithNoStartingSlash = "Assets/Editor/thumb.png";

            string expected = "url('project://database/Assets/Editor/thumb.png')";


            string resultWithStartingSlash = validator.FormatAssetPathStringForStorage(pathWithStartingSlash);
            string resultWithNoStartingSlash = validator.FormatAssetPathStringForStorage(pathWithNoStartingSlash);

            Assert.AreEqual(expected, resultWithStartingSlash);
            Assert.AreEqual(expected, resultWithNoStartingSlash);
        }

        [Test]
        public void FormatAssetPathStringForStorage_FormatsResourceInResourcesFolderCorrectly()
        {
            string path = "Assets/Resources/Images/my-image.png";
            string expected = "resource('Assets/Resources/Images/my-image')";

            string result = validator.FormatAssetPathStringForStorage(path);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void FormatAssetPathStringForStorage_FormatsResourceInEditorDefaultResourcesFolderCorrectly()
        {
            string path = "Assets/Editor Default Resources/Images/my-image.png";
            string expected = "resource('Assets/Editor Default Resources/Images/my-image.png')";

            string result = validator.FormatAssetPathStringForStorage(path);

            Assert.AreEqual(expected, result);
        }


        [Test]
        public void FormatAssetPathStringForStorage_ThrowsExceptionForEmptyPath()
        {
            string path = "";

            Assert.Throws<ArgumentException>(() => validator.FormatAssetPathStringForStorage(path));
        }


    }
}

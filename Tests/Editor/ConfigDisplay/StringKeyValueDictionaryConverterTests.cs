using NUnit.Framework;
using ZoboUI.Core;
using ZoboUI.Editor;
using System.Collections.Generic;

namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class KeyValueDictionaryTests
    {
        [Test]
        public void GetRequiredConfigValuesForUssGeneration_ReturnsCorrectValues()
        {
            // Arrange
            var keyValueDictionary = new KeyValueDictionary();
            var ussDictionary1 = new USSPropertyToValueDictionary() { { "property1", "value1" }, { "property2", "value2" } };
            var ussDictionary2 = new USSPropertyToValueDictionary() { { "property3", "value3" }, { "property4", "value4" } };
            keyValueDictionary.Add("key1", new StringKeyValueHolder { value = "value1", uss = ussDictionary1 });
            keyValueDictionary.Add("key2", new StringKeyValueHolder { value = "value2", uss = ussDictionary2 });

            // Act
            var result = keyValueDictionary.GetRequiredConfigValuesForUssGeneration();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("key1", result[0].Key);
            Assert.AreEqual("value1", result[0].Value);
            Assert.AreEqual("value1", result[0].UssDictionary["property1"]);
            Assert.AreEqual("value2", result[0].UssDictionary["property2"]);
            Assert.AreEqual("key2", result[1].Key);
            Assert.AreEqual("value2", result[1].Value);
            Assert.AreEqual("value3", result[1].UssDictionary["property3"]);
            Assert.AreEqual("value4", result[1].UssDictionary["property4"]);
        }

        [Test]
        public void IsSameValueType_ReturnsTrueForSameType()
        {
            // Arrange
            var keyValueDictionary = new KeyValueDictionary();

            // Act
            var result = keyValueDictionary.IsSameValueType(new KeyValueDictionary());

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IsSameValueType_ReturnsFalseForDifferentType()
        {
            // Arrange
            var keyValueDictionary = new KeyValueDictionary();

            // Act
            var result = keyValueDictionary.IsSameValueType(new StringImageDictionary());

            // Assert
            Assert.IsFalse(result);
        }
    }

    public class StringKeyValueDictionaryConverterTests
    {
        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var model = new KeyValueDictionary
            {
                { "key1", new StringKeyValueHolder { value = "value1", uss = new USSPropertyToValueDictionary() } },
                { "key2", new StringKeyValueHolder { value = "value2", uss = new USSPropertyToValueDictionary() } }
            };
            var converter = new StringKeyValueDictionaryConverter();

            // Act
            var result = converter.ConvertToDisplay(model);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("key1", result[0].Key);
            Assert.AreEqual("value1", result[0].Value);
            Assert.AreEqual("key2", result[1].Key);
            Assert.AreEqual("value2", result[1].Value);
        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var display = new List<InspectorKeyValueDictionaryDisplay>
            {
                new InspectorKeyValueDictionaryDisplay { Key = "key1", Value = "value1", UssProperties = new List<UssPropertyHolderDisplay>() },
                new InspectorKeyValueDictionaryDisplay { Key = "key2", Value = "value2", UssProperties = new List<UssPropertyHolderDisplay>() }
            };
            var converter = new StringKeyValueDictionaryConverter();

            // Act
            var result = converter.ConvertFromDisplay(display);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"].value);
            Assert.AreEqual("value2", result["key2"].value);
        }
    }
}
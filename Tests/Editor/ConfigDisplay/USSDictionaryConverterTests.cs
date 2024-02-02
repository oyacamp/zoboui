using NUnit.Framework;
using ZoboUI.Core;
using ZoboUI.Editor;
using System.Collections.Generic;

namespace ZoboUI.Editor.Tests
{
    public class USSDictionaryConverterTests
    {
        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var model = new USSPropertyToValueDictionary
            {
                { "property1", "value1" },
                { "property2", "value2" }
            };
            var converter = new USSDictionaryConverter();

            // Act
            var result = converter.ConvertToDisplay(model);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("property1", result[0].PropertyName);
            Assert.AreEqual("value1", result[0].PropertyValue);
            Assert.AreEqual("property2", result[1].PropertyName);
            Assert.AreEqual("value2", result[1].PropertyValue);
        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var display = new List<UssPropertyHolderDisplay>
            {
                new UssPropertyHolderDisplay { PropertyName = "property1", PropertyValue = "value1" },
                new UssPropertyHolderDisplay { PropertyName = "property2", PropertyValue = "value2" }
            };
            var converter = new USSDictionaryConverter();

            // Act
            var result = converter.ConvertFromDisplay(display);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["property1"]);
            Assert.AreEqual("value2", result["property2"]);
        }
    }
}
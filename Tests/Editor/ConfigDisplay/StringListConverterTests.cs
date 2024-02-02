using NUnit.Framework;
using ZoboUI.Core;
using ZoboUI.Editor;
using System.Collections.Generic;

namespace ZoboUI.Editor.Tests
{
    public class StringListConverterTests
    {
        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var model = new TextFieldDataList();
            model.Values = new List<TextFieldStringData>() {
                new TextFieldStringData { Value = "item1" },
                new TextFieldStringData { Value = "item2" }
            };
            var converter = new StringListConverter();

            // Act
            var result = converter.ConvertFromDisplay(model);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("item1", result[0]);
            Assert.AreEqual("item2", result[1]);
        }

        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var display = new StringListValue { "item1", "item2" };
            var converter = new StringListConverter();

            // Act
            var result = converter.ConvertToDisplay(display);

            // Assert
            Assert.AreEqual(2, result.Values.Count);
            Assert.AreEqual("item1", result.Values[0].Value);
            Assert.AreEqual("item2", result.Values[1].Value);
        }
    }
}
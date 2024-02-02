using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEngine;
namespace ZoboUI.Editor.Tests
{
    public class StringDropdownFieldValueListConverterTests
    {
        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var model = new StringListValue { "item1", "item2" };
            var converter = new StringDropdownFieldValueListConverter<StringDropdownFieldValue>();

            // Act
            var result = converter.ConvertToDisplay(model);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("item1", result[0].Value);
            Assert.AreEqual("item2", result[1].Value);
        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var display = new List<StringDropdownFieldValue>
            {
                new StringDropdownFieldValue { Value = "item1" },
                new StringDropdownFieldValue { Value = "item2" }
            };
            var converter = new StringDropdownFieldValueListConverter<StringDropdownFieldValue>();

            // Act
            var result = converter.ConvertFromDisplay(display);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("item1", result[0]);
            Assert.AreEqual("item2", result[1]);
        }
    }
}
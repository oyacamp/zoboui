using NUnit.Framework;
using ZoboUI.Core;
using System.Collections.Generic;

namespace ZoboUI.Editor.Tests
{
    public class ClassModifierDisplayTests
    {
        [Test]
        public void SetInitialValuesForNewItems_SetsCorrectValues()
        {
            // Arrange
            var classModifierDisplay = new ClassModifierDisplay();

            // Act
            classModifierDisplay.SetInitialValuesForNewItems();

            // Assert
            Assert.AreEqual("", classModifierDisplay.ModifierName);
        }
    }

    public class ClassModifierConverterTests
    {
        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var model = new ClassModifierDictionary
            {
                { "key1", new ModifierValueHolder { value = "value1" } },
                { "key2", new ModifierValueHolder { value = "value2" } }
            };
            var converter = new ClassModifierConverter();

            // Act
            var result = converter.ConvertToDisplay(model);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("key1", result[0].ModifierName);
            Assert.AreEqual("value1", result[0].Value);
            Assert.AreEqual("key2", result[1].ModifierName);
            Assert.AreEqual("value2", result[1].Value);
        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            // Arrange
            var display = new List<ClassModifierDisplay>
            {
                new ClassModifierDisplay { ModifierName = "key1", Value = "value1" },
                new ClassModifierDisplay { ModifierName = "key2", Value = "value2" }
            };
            var converter = new ClassModifierConverter();

            // Act
            var result = converter.ConvertFromDisplay(display);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("value1", result["key1"].value);
            Assert.AreEqual("value2", result["key2"].value);
        }
    }
}
using NUnit.Framework;
using ZoboUI.Core;
using ZoboUI.Editor;
using System.Collections.Generic;

namespace ZoboUI.Editor.Tests
{
    public class ClassTagToUssPropertyMapConverterTests
    {
        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            ClassTagPropertyHolder classTagPropertyHolder1 = new ClassTagPropertyHolder()
            {
                properties = new StringListValue { "property1", "property2" }
            };


            ClassTagPropertyHolder classTagPropertyHolder2 = new ClassTagPropertyHolder()
            {
                properties = new StringListValue { "property3", "property4" }
            };


            var model = new ClassTagToUssPropertyDictionary
            {
                { "class1", classTagPropertyHolder1},
                { "class2", classTagPropertyHolder2 }
            };
            var converter = new ClassTagToUssPropertyMapConverter();

            var result = converter.ConvertToDisplay(model);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("class1", result[0].ClassTag);
            Assert.AreEqual(2, result[0].UssPropertyNames.Values.Count);
            Assert.AreEqual("class2", result[1].ClassTag);
            Assert.AreEqual(2, result[1].UssPropertyNames.Values.Count);
        }

        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            var display = new List<ClassTagToUssPropertyMapDisplay>
            {
                new ClassTagToUssPropertyMapDisplay { ClassTag = "class1", UssPropertyNames = new TextFieldDataList { Values = new List<TextFieldStringData>(){ new TextFieldStringData { Value = "property1" }, new TextFieldStringData { Value = "property2" } }} },
                new ClassTagToUssPropertyMapDisplay { ClassTag = "class2", UssPropertyNames = new TextFieldDataList { Values = new List<TextFieldStringData>(){new TextFieldStringData { Value = "property3" }, new TextFieldStringData { Value = "property4" } }} }
            };
            var converter = new ClassTagToUssPropertyMapConverter();

            var result = converter.ConvertFromDisplay(display);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2, result["class1"].properties.Count);
            Assert.AreEqual(2, result["class2"].properties.Count);
        }
    }
}
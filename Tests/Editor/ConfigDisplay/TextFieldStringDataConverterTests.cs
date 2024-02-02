using NUnit.Framework;
using ZoboUI.Editor;

namespace ZoboUI.Editor.Tests
{
    public class TextFieldStringDataTests
    {
        [Test]
        public void SetInitialValuesForNewItems_SetsCorrectValues()
        {
            var textFieldStringData = new TextFieldStringData();

            textFieldStringData.SetInitialValuesForNewItems();

            Assert.AreEqual("", textFieldStringData.Value);
        }
    }

    public class TextFieldStringDataConverterTests
    {
        [Test]
        public void ConvertFromDisplay_ReturnsCorrectValue()
        {
            var textFieldStringData = new TextFieldStringData { Value = "test" };
            var converter = new TextFieldStringDataConverter();

            var result = converter.ConvertFromDisplay(textFieldStringData);

            Assert.AreEqual("test", result);
        }

        [Test]
        public void ConvertToDisplay_ReturnsCorrectValue()
        {
            var converter = new TextFieldStringDataConverter();

            var result = converter.ConvertToDisplay("test");

            Assert.AreEqual("test", result.Value);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Core;


namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class ThemeUssGeneratorTests
    {
        private ThemeUssGenerator generator;

        /// <summary>
        /// These are the types of files/locations that the generator will look for when generating uss files. You would usually pass this to the compilation.content property in a themeconfig
        /// </summary>
        public static List<string> compilationContent = new List<string>{
            "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/*.uxml",
            "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/*.cs",
        };
        public static readonly string expectedGeneratedUSSFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Expected/generated.uss";
        public static readonly string expectedPurgedUSSFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Expected/purged.uss";

        public static readonly string testGeneratedUssOutputFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Generated/generated.uss";
        public static readonly string testPurgedUssOutputFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Generated/purged.uss";

        [SetUp]
        public void SetUp()
        {
            generator = new ThemeUssGenerator();


        }

        [TearDown]
        public void TearDown()
        {
            // If the test generated uss file exists, delete it
            AssetDatabase.DeleteAsset(testGeneratedUssOutputFilePath);

            // If the test purged uss file exists, delete it
            AssetDatabase.DeleteAsset(testPurgedUssOutputFilePath);


        }



        [Test]
        public void MergeCustomUssWithGeneratedUss_TemplateStringNotInCustomUss_AppendsGeneratedUssToCustomUss()
        {
            string customUss = ".customStyle { color: white; }";
            string generatedUss = ".generatedStyle { color: black; }";

            string result = generator.MergeCustomUssWithGeneratedUss(customUss, generatedUss, ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING);

            Assert.That(result, Is.EqualTo(customUss + "\n" + generatedUss + "\n"));
        }

        [Test]
        public void MergeCustomUssWithGeneratedUss_TemplateStringAppearsMultipleTimes_ThrowsException()
        {
            string customUss = ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING + ".customStyle { color: red } .anotherStyle { color: green; }" + ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING;
            string generatedUss = ".generatedStyle { color: black; }";

            Assert.Throws<System.Exception>(() => generator.MergeCustomUssWithGeneratedUss(customUss, generatedUss, ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING));
        }

        [Test]
        public void MergeCustomUssWithGeneratedUss_TemplateStringAppearsOnce_ReplacesTemplateStringWithGeneratedUss()
        {
            string customUss = ".customStyle { color: red;} " + ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING;
            string generatedUss = ".generatedStyle { color: black; }";

            string result = generator.MergeCustomUssWithGeneratedUss(customUss, generatedUss, ThemeUssGenerator.GENERATED_USS_STYLES_TEMPLATE_STRING);

            string expectedResult = ".customStyle { color: red;} " + generatedUss;

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        /// <summary>
        /// This uses a regular expression to match any sequence of one or more whitespace characters (\s+) and replace it with a single space. The Trim method is then used to remove any leading or trailing whitespace
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private string NormalizeUssString(string input)
        {
            return Regex.Replace(input, @"\s+", " ").Trim();
        }


        [Test]
        public void GenerateUssRuleString_GivenValidInput_ReturnsCorrectUssRule()
        {
            var ussPropertyToValueDictionary = new USSPropertyToValueDictionary
    {
        { "color", "white" },
        { "background-color", "black" }
    };

            string result = NormalizeUssString(generator.GenerateUssRuleString(".myClass", ussPropertyToValueDictionary));

            string expected = NormalizeUssString(@".myClass {
                color: white;
                background-color: black;
                }");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GenerateUssFromBag_GivenValidInput_ReturnsCorrectUss()
        {
            var bag = new UtilityRuleBag
    {
        { ".myClass", new USSPropertyToValueDictionary { { "color", "white" } } },
        { ".myOtherClass", new USSPropertyToValueDictionary { { "background-color", "black" } } }
    };

            string result = NormalizeUssString(generator.GenerateUssFileContentFromBag(bag));

            string expected = NormalizeUssString(@"
            .myClass {
                color: white;
                }
                
                .myOtherClass {
                    background-color: black;
                    }");

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GenerateClassName_WhenIsDefaultKey_ReturnsClassTag()
        {
            string result = ThemeUssGenerator.GenerateClassName("rounded", "DEFAULT", true);
            Assert.That(result, Is.EqualTo("rounded"));
        }

        [Test]
        public void GenerateClassName_WhenClassTagIsEmpty_ReturnsKey()
        {
            string result = ThemeUssGenerator.GenerateClassName("", "hidden", false);
            Assert.That(result, Is.EqualTo("hidden"));
        }

        [Test]
        public void GenerateClassName_WhenClassTagIsNull_ReturnsKey()
        {
            string result = ThemeUssGenerator.GenerateClassName(null, "hidden", false);
            Assert.That(result, Is.EqualTo("hidden"));
        }

        [Test]
        public void GenerateClassName_WhenIsNotDefaultKey_ReturnsClassTagAndKey()
        {
            string result = ThemeUssGenerator.GenerateClassName("flex", "none", false);
            Assert.That(result, Is.EqualTo("flex" + ThemeUssGenerator.BASE_SEPARATOR + "none"));
        }

        [Test]
        public void AddCustomPrefixToClassTag_WhenCustomPrefixIsEmpty_ReturnsClassTag()
        {
            string result = ThemeUssGenerator.AddCustomPrefixToClassTag("bg", "");
            Assert.That(result, Is.EqualTo("bg"));
        }

        [Test]
        public void AddCustomPrefixToClassTag_WhenCustomPrefixIsNotEmpty_ReturnsMergedClassTag()
        {
            string result = ThemeUssGenerator.AddCustomPrefixToClassTag("bg", "mycustomprefix-");
            Assert.That(result, Is.EqualTo("mycustomprefix-bg"));
        }

        [Test]
        public void AddCustomPrefixToClassTag_WhenClassTagIsEmpty_ReturnsCustomPrefix()
        {
            string result = ThemeUssGenerator.AddCustomPrefixToClassTag("", "mycustomprefix");
            Assert.That(result, Is.EqualTo("mycustomprefix"));
        }

        [Test]
        public void AddCustomPrefixToClassTag_WhenClassTagAndCustomPrefixAreEmpty_ReturnsEmptyString()
        {
            string result = ThemeUssGenerator.AddCustomPrefixToClassTag("", "");
            Assert.That(result, Is.EqualTo(""));
        }




    }
}
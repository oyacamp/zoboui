using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZoboUI.Editor.Tests
{


    [TestFixture]
    public class UssPurgerTests
    {
        private UssPurger purger;
        private readonly string sampleCSharpFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/TestScriptWithUss.cs";

        private readonly string sampleUxmlFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/UXML/TestUXML.uxml";

        private readonly string sampleUssFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/TestUss.uss";

        private readonly string testDataRootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData";

        private readonly string testOutputUssFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/TestOutput.uss";
        /// <summary>
        /// The classes that are expected to be extracted from the test data. These are the classes used in the sampleUxmlFilePath file and the sampleCSharpFilePath file
        /// </summary>
        List<string> expectedClassesInTestData = new List<string> { "space-y-4", "mt-minus-3", "hover_text-red-800", "pl-6", "bold", "bg-blue-300", "hover_bg-blue-700", "hidden", "bg-transparent", "transition", "transition-duration-300", "text-slate-900", "hover_text-gray-200" };

        DefaultClassExtractor defaultExtractor = new DefaultClassExtractor(new DefaultClassExtractor.Config
        {
            ModifierSeparator = "_",
            CustomPrefix = ""
        });

        List<IClassExtractor> extractors;

        [SetUp]
        public void SetUp()
        {

            List<string> patterns = new List<string> { "*.uxml", "*.cs" };

            extractors = new List<IClassExtractor> { defaultExtractor };

            purger = new UssPurger(extractors, patterns);

        }
        [TearDown]
        public void TearDown()
        {
            if (File.Exists(testOutputUssFilePath))
            {
                File.Delete(testOutputUssFilePath);
                // Also delete the meta file if it exists

                if (File.Exists(testOutputUssFilePath + ".meta"))
                {
                    File.Delete(testOutputUssFilePath + ".meta");
                }

                AssetDatabase.Refresh();
            }

        }

        [Test]
        public void GetMatchingFiles_ValidRootPath_ReturnsExpectedFiles()
        {
            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData";
            var expectedFiles = new List<string> { sampleCSharpFilePath, sampleUxmlFilePath };

            var actualFiles = purger.GetMatchingFiles(rootPath);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            CollectionAssert.DoesNotContain(actualFiles, sampleUssFilePath);
        }

        [Test]
        public void GetMatchingFiles_ValidRootPath_UXMLOnlyPattern_ReturnsExpectedFiles()
        {
            List<string> patterns = new List<string> { "*.uxml" };

            var uxmlOnlyPurger = new UssPurger(extractors, patterns);
            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData";
            var expectedFiles = new List<string> { sampleUxmlFilePath };

            var actualFiles = uxmlOnlyPurger.GetMatchingFiles(rootPath);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            CollectionAssert.DoesNotContain(actualFiles, sampleCSharpFilePath);
            CollectionAssert.DoesNotContain(actualFiles, sampleUssFilePath);

        }

        [Test]
        public void GetMatchingFiles_ValidRootPath_CSharpOnlyPattern_ReturnsExpectedFiles()
        {
            List<string> patterns = new List<string> { "*.cs" };

            var cSharpOnlyPurger = new UssPurger(extractors, patterns);
            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData";
            var expectedFiles = new List<string> { sampleCSharpFilePath };

            var actualFiles = cSharpOnlyPurger.GetMatchingFiles(rootPath);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            CollectionAssert.DoesNotContain(actualFiles, sampleUxmlFilePath);
            CollectionAssert.DoesNotContain(actualFiles, sampleUssFilePath);

        }

        [Test]
        public void GetMatchingFiles_ValidRootPath_InvalidPattern_ReturnsExpectedFiles()
        {
            List<string> patterns = new List<string> { "*.txt" };

            var textOnlyPurger = new UssPurger(extractors, patterns);
            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData";
            var expectedFiles = new List<string> { };

            var actualFiles = textOnlyPurger.GetMatchingFiles(rootPath);

            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            CollectionAssert.DoesNotContain(actualFiles, sampleUxmlFilePath);
            CollectionAssert.DoesNotContain(actualFiles, sampleCSharpFilePath);
            CollectionAssert.DoesNotContain(actualFiles, sampleUssFilePath);

        }

        [Test]
        public void GetMatchingFiles_InvalidValidRootPath_ThrowsException()
        {

            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/InvalidPath";

            // ToList() forces the GetMatchingFiles method to execute immediately. If we don't do this, no exception will be thrown because the method is an iterator method 
            Assert.Throws<System.IO.DirectoryNotFoundException>(() => purger.GetMatchingFiles(rootPath).ToList());
        }

        [Test]
        public void GetMatchingFiles_FilePathProvidedAsRoot_ThrowsException()
        {

            var rootPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssPurging/TestData/InvalidPath/myfile.cs";

            // ToList() forces the GetMatchingFiles method to execute immediately. If we don't do this, no exception will be thrown because the method is an iterator method 
            Assert.Throws<System.IO.DirectoryNotFoundException>(() => purger.GetMatchingFiles(rootPath).ToList());
        }

        [Test]
        public void SplitUssContentIntoSections_WithIgnoreSignifiers_CorrectlyIdentifiesSections()
        {
            string ussContent = @"
        .some-class {
            color: red;
        }
        /* purge start ignore */
        .ignored-class {
            color: blue;
        }

        .second-ignored-class {
            color: blue;
        }
        /* purge end ignore */
        .another-class {
            color: green;
        }
    ";

            var sections = purger.SplitUssContentIntoSections(ussContent);


            Assert.AreEqual(3, sections.Count);

            Assert.IsFalse(sections[0].IsIgnored);
            Assert.IsTrue(sections[0].Section.Contains(".some-class"));

            Assert.IsTrue(sections[1].IsIgnored);
            Assert.IsTrue(sections[1].Section.Contains(".ignored-class"));
            Assert.IsTrue(sections[1].Section.Contains(".second-ignored-class"));

            //Loop through the sections and make sure they don't contain the ignore comments
            foreach (var section in sections)
            {
                Assert.IsFalse(section.Section.Contains("/* purge start ignore */"));
                Assert.IsFalse(section.Section.Contains("/* purge end ignore */"));
            }






            Assert.IsFalse(sections[2].IsIgnored);
            Assert.IsTrue(sections[2].Section.Contains(".another-class"));


        }

        [Test]
        public void SplitUssContentIntoSections_WithIgnoreSignifiersAtTop_CorrectlyIdentifiesSections()
        {
            string ussContent = @"
     
        /* purge start ignore */
        .ignored-class {
            color: blue;
        }

        .second-ignored-class {
            color: blue;
        }
        /* purge end ignore */

           .some-class {
            color: red;
        }
        .another-class {
            color: green;
        }
    ";

            var sections = purger.SplitUssContentIntoSections(ussContent);


            Assert.AreEqual(2, sections.Count);

            Assert.IsTrue(sections[0].IsIgnored);
            Assert.IsTrue(sections[0].Section.Contains(".ignored-class"));
            Assert.IsTrue(sections[0].Section.Contains(".second-ignored-class"));

            Assert.IsFalse(sections[1].IsIgnored);
            Assert.IsTrue(sections[1].Section.Contains(".some-class"));

            //Loop through the sections and make sure they don't contain the ignore comments
            foreach (var section in sections)
            {
                Assert.IsFalse(section.Section.Contains("/* purge start ignore */"));
                Assert.IsFalse(section.Section.Contains("/* purge end ignore */"));
            }



        }

        [Test]
        public void SplitUssContentIntoSections_WithMultipleIgnoreSignifiers_CorrectlyIdentifiesSections()
        {
            string ussContent = @"
             
                /* purge start ignore */
                .ignored-class {
                    color: blue;
                }

                .second-ignored-class {
                    color: blue;
                }
                /* purge end ignore */

                .some-class {
                    color: red;
                }
                .another-class {
                    color: green;
                }

                /* purge start ignore */
                .third-ignored-class {
                    color: yellow;
                }
                /* purge end ignore */

                .final-class {
                    color: purple;
                }
            ";

            var sections = purger.SplitUssContentIntoSections(ussContent);

            Assert.AreEqual(4, sections.Count);

            Assert.IsTrue(sections[0].IsIgnored);
            Assert.IsTrue(sections[0].Section.Contains(".ignored-class"));
            Assert.IsTrue(sections[0].Section.Contains(".second-ignored-class"));

            Assert.IsFalse(sections[1].IsIgnored);
            Assert.IsTrue(sections[1].Section.Contains(".some-class"));

            Assert.IsTrue(sections[2].IsIgnored);
            Assert.IsTrue(sections[2].Section.Contains(".third-ignored-class"));

            Assert.IsFalse(sections[3].IsIgnored);
            Assert.IsTrue(sections[3].Section.Contains(".final-class"));

            //Loop through the sections and make sure they don't contain the ignore comments
            foreach (var section in sections)
            {
                Assert.IsFalse(section.Section.Contains("/* purge start ignore */"));
                Assert.IsFalse(section.Section.Contains("/* purge end ignore */"));
            }

        }


        [Test]
        public void GetUsedClasses_FromLoadedFiles_ReturnsCorrectContent()
        {

            // These are just a few classes that are not used in the sampleUxmlFilePath file and the sampleCSharpFilePath file
            List<string> excludedClasses = new List<string> { "rounded", "rounded-full", "rounded-sm", "bg-red", "font-bold", "hover_text-red-500", "hover_text-red-600", "mt-20", "inset-40", "inset" };

            var classes = purger.GetUsedClassLikeStrings(testDataRootPath);


            foreach (var item in expectedClassesInTestData)
            {
                CollectionAssert.Contains(classes, item);
            }

            foreach (var item in excludedClasses)
            {
                CollectionAssert.DoesNotContain(classes, item);
            }



        }


        [Test]
        public void GetUsedClassLikeStrings_FromLoadedFiles_ReturnsExpectedClasses()
        {



            var usedClassLikeList = purger.GetUsedClassLikeStrings(testDataRootPath);


            // Check that the other classes are present in the output file
            foreach (var item in expectedClassesInTestData)
            {
                Assert.IsTrue(usedClassLikeList.Contains(item));
            }


        }
        /* Unity Test Framework doesn't support async tests unless you use the experimental package 2.0.1-pre.18
                [Test]
                public async Task GeneratePurgedUssFileAsync_FromLoadedFiles_ReturnsCorrectContentAsync()
                {


                    await purger.GeneratePurgedUssFileAsync(sampleUssFilePath, testOutputUssFilePath, testDataRootPath);

                    Assert.IsTrue(File.Exists(testOutputUssFilePath));


                    var purgedUssContent = await File.ReadAllTextAsync(testOutputUssFilePath);


                    var styleSheet = parser.Parse(purgedUssContent);

                    var ruleCount = styleSheet.StyleRules.ToList().Count;

                    Assert.AreEqual(expectedClassesInTestData.Count, ruleCount);


                }
        */
        [Test]
        public void GetUsedClassLikeStrings_FromLoadedFiles_ExcludesBlockListedClasses()
        {
            List<string> blockList = new List<string> { "bg-blue-300", "hover_bg-blue-700" };
            List<string> patterns = new List<string> { "*.uxml", "*.cs" };

            List<string> expectedClassesInPurgedUssFile = expectedClassesInTestData.Except(blockList).ToList();

            var purgerWithBlocklist = new UssPurger(extractors, patterns, blocklist: blockList);

            var sampleUssContent = File.ReadAllText(sampleUssFilePath);

            var usedClassLikeList = purgerWithBlocklist.GetUsedClassLikeStrings(testDataRootPath);


            // Check that the blocklisted classes are not present in the usedClassLikeList
            foreach (var item in blockList)
            {
                // Assert that the uss file in the project contains the blocklisted classes
                Assert.IsTrue(sampleUssContent.Contains(item));
                Assert.IsFalse(usedClassLikeList.Contains(item));
            }

            // Check that the other classes are present in the output file
            foreach (var item in expectedClassesInPurgedUssFile)
            {
                Assert.IsTrue(usedClassLikeList.Contains(item));
            }


        }

        [Test]
        public void GetUsedClassLikeStrings_FromLoadedFiles_IncludesSafeListedClasses()
        {
            List<string> safeList = new List<string> { "bg-madeupcolor-800", "focus_text-madeupcolor-100" };
            List<string> patterns = new List<string> { "*.uxml", "*.cs" };

            var purgerWithSafelist = new UssPurger(extractors, patterns, safelist: safeList);

            var sampleUssContent = File.ReadAllText(sampleUssFilePath);

            var usedClassLikeList = purgerWithSafelist.GetUsedClassLikeStrings(testDataRootPath);

            // Check that the safelisted classes are present in the usedClassLikeList

            foreach (var item in safeList)
            {
                // Assert that the uss file in the project doesn't contain the safelisted classes as we want to make sure that classes that aren't used in the project are included in the purged file if they are in the safelist
                Assert.IsFalse(sampleUssContent.Contains(item));
                Assert.IsTrue(usedClassLikeList.Contains(item));
            }

            // Check that the other classes are present in the output file
            foreach (var item in expectedClassesInTestData)
            {
                Assert.IsTrue(usedClassLikeList.Contains(item));
            }

        }

        [Test]
        public void GetClassSelectorsFromRuleWithoutPseudoClasses_ReturnsCorrectContent_WithSimpleClasses()
        {
            string rule = ".pt-3 { padding-top: 12px; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);

            Assert.AreEqual(1, selectors.Count);
            Assert.AreEqual(".pt-3", selectors[0]);
        }

        [Test]
        public void GetClassSelectorsFromRuleWithoutPseudoClasses_ReturnsEmptyList_IfRuleDoesntHaveDotPrefix()
        {
            string rule = "pt-3 { padding-top: 12px; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);

            Assert.AreEqual(0, selectors.Count);
        }


        [Test]
        public void GetClassSelectorsFromRuleWithoutPseudoClasses_ReturnsCorrectContent_WithPseudoClasses()
        {
            string rule = ".bg-red-300:hover { background-color: #FED7D7; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);

            Assert.AreEqual(1, selectors.Count);
            Assert.AreEqual(".bg-red-300", selectors[0]);
        }

        [Test]
        public void GetClassSelectorsFromRule_ReturnsCorrectContent_WithChainedClass()
        {
            string rule = ".dark-theme.bg-red-300:hover { background-color: #FED7D7; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);


            Assert.AreEqual(2, selectors.Count);
            Assert.AreEqual(".dark-theme", selectors[0]);
            Assert.AreEqual(".bg-red-300", selectors[1]);
        }

        [Test]
        public void GetClassSelectorsFromRule_ReturnsCorrectContent_WithDescendants()
        {
            string rule = ".space-y-4 > *  { margin-top: 1px; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);

            Assert.AreEqual(1, selectors.Count);

            Assert.AreEqual(".space-y-4", selectors[0]);

        }

        [Test]
        public void GetClassSelectorsFromRuleWithoutPseudoClasses_ReturnsCorrectContent_WithMultipleClasses()
        {
            string rule = "#MyElement .pt-3 { padding-top: 12px; } .bg-red-300:hover { background-color: #FED7D7; }";

            var selectors = UssPurger.GetClassSelectorsFromRuleWithoutPseudoClasses(rule);

            Assert.AreEqual(2, selectors.Count);
            Assert.AreEqual(".pt-3", selectors[0]);
            Assert.AreEqual(".bg-red-300", selectors[1]);

            Assert.IsFalse(selectors.Contains("#MyElement"));
        }


        [Test]
        public void GetStyleBracesForSelector_WithSimpleClass_ReturnsCorrectContent()
        {
            string rule = ".pt-3 { padding-top: 12px; }";

            var styleBraces = purger.GetStyleBracesForSelector(rule, ".pt-3");


            Assert.AreEqual(1, styleBraces.Count);

            Assert.IsTrue(styleBraces[0].Contains("padding-top"));
            Assert.IsTrue(styleBraces[0].Contains("12px"));
        }

        [Test]
        public void GetStyleBracesForSelector_WithPseudoClass_ReturnsCorrectContent()
        {
            string rule = ".hover_bg-red-300:hover { background-color: #FED7D7; }";

            var styleBraces = purger.GetStyleBracesForSelector(rule, ".hover_bg-red-300:hover");


            Assert.AreEqual(1, styleBraces.Count);

            Assert.IsTrue(styleBraces[0].Contains("background-color"));
            Assert.IsTrue(styleBraces[0].Contains("#FED7D7"));
        }




        [Test]
        public void GetStyleBracesForSelector_WithMultipleClasses_ReturnsCorrectContent()
        {
            string rule = "#MyElement .pt-3 { padding-top: 12px; }";

            var styleBraces = purger.GetStyleBracesForSelector(rule, "#MyElement .pt-3");


            Assert.AreEqual(1, styleBraces.Count);

            Assert.IsTrue(styleBraces[0].Contains("padding-top"));
            Assert.IsTrue(styleBraces[0].Contains("12px"));
        }

        [Test]
        public void GetStyleBracesForSelector_WithComplexSelector_ReturnsCorrectContent()
        {
            string rule = @".space-x-minus-96 > * {
	margin-left: -384px;
	margin-right: -384px;
}";

            var styleBraces = purger.GetStyleBracesForSelector(rule, ".space-x-minus-96 > *");
            var styleBracesWithStrippedSpaces = purger.GetStyleBracesForSelector(rule, ".space-x-minus-96>*");


            Assert.AreEqual(1, styleBraces.Count);
            Assert.AreEqual(1, styleBracesWithStrippedSpaces.Count);

            Assert.IsTrue(styleBraces[0].Contains("margin-left"));
            Assert.IsTrue(styleBraces[0].Contains("-384px"));
            Assert.IsTrue(styleBraces[0].Contains("margin-right"));
            Assert.IsTrue(styleBraces[0].Contains("-384px"));
        }

        [Test]
        public void GetStyleBracesForSelector_WithMoreComplexSelector_ReturnsCorrectContent()
        {
            string rule = @".space-x-0 > * + * { --zb-space-x-reverse: 0; margin-left: calc(0px * calc(1 - var(--zb-space-x-reverse))); margin-right: calc(0px * var(--zb-space-x-reverse)); }";

            var styleBraces = purger.GetStyleBracesForSelector(rule, ".space-x-0 > * + *");
            var styleBracesWithStrippedSpaces = purger.GetStyleBracesForSelector(rule, ".space-x-0>*+*");


            Assert.AreEqual(1, styleBraces.Count);
            Assert.AreEqual(1, styleBracesWithStrippedSpaces.Count);

            Assert.IsTrue(styleBraces[0].Contains("margin-left"));
            Assert.IsTrue(styleBraces[0].Contains("calc(0px * calc(1 - var(--zb-space-x-reverse)))"));
            Assert.IsTrue(styleBraces[0].Contains("margin-right"));
            Assert.IsTrue(styleBraces[0].Contains("calc(0px * var(--zb-space-x-reverse))"));
        }

        [Test]
        public void GetStyleBracesForSelector_DoesntCrashWithLargeUSSFile()
        {
            // Load the USS file
            string ussContent = File.ReadAllText(sampleUssFilePath);

            var styleBraces = purger.GetStyleBracesForSelector(ussContent, ".bg-transparent");

            Assert.AreEqual(1, styleBraces.Count);

            Assert.IsTrue(styleBraces[0].Contains("background-color"));


        }




    }
}

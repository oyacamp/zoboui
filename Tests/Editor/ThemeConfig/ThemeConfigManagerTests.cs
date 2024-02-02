using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Core;
using ZoboUI.Core.Plugins;


namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class ThemeConfigManagerTests
    {
        private readonly string expectedGeneratedUSSFilePath = ThemeUssGeneratorTests.expectedGeneratedUSSFilePath;
        private readonly string expectedPurgedUSSFilePath = ThemeUssGeneratorTests.expectedPurgedUSSFilePath;

        private readonly string testGeneratedUssOutputFilePath = ThemeUssGeneratorTests.testGeneratedUssOutputFilePath;
        private readonly string testPurgedUssOutputFilePath = ThemeUssGeneratorTests.testPurgedUssOutputFilePath;

        private readonly string testGeneratedThemeConfigAssetPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Generated/TestThemeConfig.asset";

        private readonly string testExportedConfigJsonPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Generated/TestExportThemeConfig.json";


        private readonly string testPluginAssetPath = "Packages/com.oyacamp.zoboui/Tests/Editor/UssGeneration/TestData/Generated/TestPlugin.asset";



        [SetUp]
        public void SetUp()
        {
            // Create the generated asset directory if it doesn't exist
            string directory = System.IO.Path.GetDirectoryName(testGeneratedThemeConfigAssetPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

        }

        protected ThemeConfigManager GetThemeConfigManagerWithTestPackageLocationsAndDefaultConfig()
        {
            ThemeConfigManager asset = ScriptableObject.CreateInstance<ThemeConfigManager>();
            AssetDatabase.CreateAsset(asset, testGeneratedThemeConfigAssetPath);
            AssetDatabase.SaveAssets();

            ThemeConfigManager loadedAsset = AssetDatabase.LoadAssetAtPath<ThemeConfigManager>(testGeneratedThemeConfigAssetPath);
            Assert.IsNotNull(loadedAsset);

            loadedAsset.LoadDefaultThemeConfig();

            // We need to ensure that the config only looks for files in the test package locations, otherwise it will look for files in the project root
            ThemeConfig themeConfigWithTestPackageLocations = loadedAsset.ThemeConfig;

            themeConfigWithTestPackageLocations.compilation.content.Clear();
            foreach (var content in ThemeUssGeneratorTests.compilationContent)
            {
                themeConfigWithTestPackageLocations.compilation.content.Add(content);
            }


            loadedAsset.GeneratedUssFilePath = testGeneratedUssOutputFilePath;
            loadedAsset.PurgedUssFilePath = testPurgedUssOutputFilePath;

            return loadedAsset;
        }

        [TearDown]
        public void TearDown()
        {
            // If the test generated uss file exists, delete it
            AssetDatabase.DeleteAsset(testGeneratedUssOutputFilePath);

            // If the test purged uss file exists, delete it
            AssetDatabase.DeleteAsset(testPurgedUssOutputFilePath);

            // If the test generated theme config asset exists, delete it  
            AssetDatabase.DeleteAsset(testGeneratedThemeConfigAssetPath);

            // If the test exported theme config json file exists, delete it

            AssetDatabase.DeleteAsset(testExportedConfigJsonPath);

            // If the test plugin asset exists, delete it
            AssetDatabase.DeleteAsset(testPluginAssetPath);

            // Delete the generated asset directory if it's empty
            string directory = System.IO.Path.GetDirectoryName(testGeneratedThemeConfigAssetPath);

            if (System.IO.Directory.Exists(directory))
            {
                AssetDatabase.DeleteAsset(directory);
            }


        }

        public void TestThemeConfigManagerAsset_CreatedSuccessfully()
        {
            ThemeConfigManager asset = ScriptableObject.CreateInstance<ThemeConfigManager>();
            AssetDatabase.CreateAsset(asset, testGeneratedThemeConfigAssetPath);
            AssetDatabase.SaveAssets();

            ThemeConfigManager loadedAsset = AssetDatabase.LoadAssetAtPath<ThemeConfigManager>(testGeneratedThemeConfigAssetPath);
            Assert.IsNotNull(loadedAsset);

            loadedAsset.LoadDefaultThemeConfig();

            Assert.Greater(loadedAsset.ThemeConfigDisplay.Core.Count, 0);
            Assert.Greater(loadedAsset.ThemeConfigDisplay.Compilation.Count, 0);
            Assert.Greater(loadedAsset.ThemeConfigDisplay.Utilities.Count, 0);
        }

        /// <summary>
        /// Tests whether the ThemeConfigManager asset can be created and whether it outputs the expected uss files by default
        /// </summary>
        [Test]
        public void TestThemeConfigManagerAsset_CreatesUssFilesSuccessfully()
        {


            ThemeConfigManager loadedAsset = GetThemeConfigManagerWithTestPackageLocationsAndDefaultConfig();
            Assert.IsNotNull(loadedAsset);

            loadedAsset.LoadDefaultThemeConfig();


            // We need to ensure that the config only looks for files in the test package locations, otherwise it will look for files in the project root
            ThemeConfig themeConfigWithTestPackageLocations = loadedAsset.ThemeConfig;

            themeConfigWithTestPackageLocations.compilation.content.Clear();
            foreach (var content in ThemeUssGeneratorTests.compilationContent)
            {
                themeConfigWithTestPackageLocations.compilation.content.Add(content);
            }



            loadedAsset.LoadFromThemeConfig(themeConfigWithTestPackageLocations);

            Assert.IsTrue(ThemeUssGeneratorTests.compilationContent.Count == loadedAsset.ThemeConfig.compilation.content.Count);
            // CHeck that the values are the same
            for (int i = 0; i < ThemeUssGeneratorTests.compilationContent.Count; i++)
            {
                Assert.IsTrue(ThemeUssGeneratorTests.compilationContent[i].Equals(loadedAsset.ThemeConfig.compilation.content[i]));
            }


            loadedAsset.GeneratedUssFilePath = testGeneratedUssOutputFilePath;
            loadedAsset.PurgedUssFilePath = testPurgedUssOutputFilePath;

            // Test that the asset has the correct uss file paths after updating them
            Assert.IsTrue(loadedAsset.GeneratedUssFilePath.Equals(testGeneratedUssOutputFilePath));
            Assert.IsTrue(loadedAsset.PurgedUssFilePath.Equals(testPurgedUssOutputFilePath));



            loadedAsset.GenerateUSSFile();
            loadedAsset.GeneratePurgedUSSFile();

            // Test that the generated uss is loaded without errors
            StyleSheet generatedUssStylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(testGeneratedUssOutputFilePath);
            Assert.IsFalse(generatedUssStylesheet.importedWithErrors);
            Assert.IsFalse(generatedUssStylesheet.importedWithWarnings);

            // Assert that the generated uss file matches the expected uss file
            int generatedUssHash = AssetDatabase.LoadAssetAtPath<StyleSheet>(expectedGeneratedUSSFilePath).contentHash;
            int expectedGeneratedUssHash = generatedUssStylesheet.contentHash;

            Assert.That(generatedUssHash, Is.EqualTo(expectedGeneratedUssHash));

            // Test that the purged uss is loaded without errors
            StyleSheet generatedPurgedUssStylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(testPurgedUssOutputFilePath);
            Assert.IsFalse(generatedPurgedUssStylesheet.importedWithErrors);
            Assert.IsFalse(generatedPurgedUssStylesheet.importedWithWarnings);

            // Assert that the purged uss file matches the expected uss file
            int generatedPurgedUssHash = generatedPurgedUssStylesheet.contentHash;
            int expectedPurgedUssHash = AssetDatabase.LoadAssetAtPath<StyleSheet>(expectedPurgedUSSFilePath).contentHash;


            Assert.That(generatedPurgedUssHash, Is.EqualTo(expectedPurgedUssHash));
        }



        [Test]
        public void TestThemeConfigManagerAsset_ImportsAndExportsCustomConfigs()
        {

            ThemeConfigManager loadedAsset = GetThemeConfigManagerWithTestPackageLocationsAndDefaultConfig();
            Assert.IsNotNull(loadedAsset);

            loadedAsset.LoadDefaultThemeConfig();

            // Update the prefix

            ThemeConfig themeConfig = loadedAsset.ThemeConfig;

            string prefixToSet = "TestPrefix";

            themeConfig.core.prefix = prefixToSet;


            string jsonStringWithPrefix = ThemeConfig.ToJson(themeConfig);

            // The theme config is generated from the display version, so you'll get a different instance of the theme config when you reference the ThemeConfig property
            // This means the prefix will be the original prefix, not the one we just set
            Assert.IsFalse(loadedAsset.ThemeConfig.core.prefix.Equals(prefixToSet));

            loadedAsset.LoadThemeConfigDisplayFromJsonString(jsonStringWithPrefix);

            Assert.IsTrue(loadedAsset.ThemeConfig.core.prefix.Equals(prefixToSet));

            loadedAsset.ExportConfigJsonFilePath = testExportedConfigJsonPath;
            loadedAsset.ExportThemeConfigToJson();

            // Test that the exported config file exists
            Assert.IsTrue(System.IO.File.Exists(testExportedConfigJsonPath));

            // Load it as a TextAsset so we can use the json utility
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(testExportedConfigJsonPath);
            string jsonString = textAsset.text;


            loadedAsset.LoadThemeConfigDisplayFromJsonString(jsonString);

            Assert.IsTrue(loadedAsset.ThemeConfig.core.prefix.Equals(prefixToSet));

        }

        [Test]
        public void TestThemeConfigManagerAsset_ProcessesCustomPlugins()
        {

            ThemeConfigManager loadedAsset = GetThemeConfigManagerWithTestPackageLocationsAndDefaultConfig();

            // Create an instance of the sample plugin 
            PriorityTestingUtlityPlugin samplePlugin = ScriptableObject.CreateInstance<PriorityTestingUtlityPlugin>();

            samplePlugin.UpdatePriority(RuleGenerationPriority.BeforeDefaultUtilities);

            // Store the plugin in the project 
            AssetDatabase.CreateAsset(samplePlugin, testPluginAssetPath);

            InspectorPluginDictionaryDisplay displayItem = new InspectorPluginDictionaryDisplay()
            {
                Value = samplePlugin
            };

            PluginDictionaryConverter converter = new PluginDictionaryConverter();

            PluginDictionary pluginDictionary = converter.ConvertFromDisplay(new List<InspectorPluginDictionaryDisplay>() { displayItem });


            ThemeConfig themeConfig = loadedAsset.ThemeConfig;

            themeConfig.core.plugins = pluginDictionary;

            loadedAsset.LoadFromThemeConfig(themeConfig);

            TempLogger logger = new TempLogger();

            loadedAsset.GenerateUSSFile(logger);

            // We expect the bag item count to be 0 because the plugin priority is set to before default utilities so no rules should be in the bag
            Assert.IsTrue(samplePlugin.LastBagItemCount == 0);


            // Set the priority to after default utilities
            samplePlugin.UpdatePriority(RuleGenerationPriority.AfterDefaultUtilities);

            loadedAsset.GenerateUSSFile(logger);

            // We expect the bag item count to be greater than 0 because the plugin priority is set to after default utilities so rules should be in the bag
            Assert.IsTrue(samplePlugin.LastBagItemCount > 0);

            // The class extractor should not have run yet because we haven't generated the purged uss file
            Assert.IsFalse(samplePlugin.ClassExtractorRan);
            // The on purge complete method should not have run yet because we haven't generated the purged uss file
            Assert.IsFalse(samplePlugin.OnPurgeCompleteRan);

            loadedAsset.GeneratePurgedUSSFile();

            // The class extractor should have run because we generated the purged uss file
            Assert.IsTrue(samplePlugin.ClassExtractorRan);

            // The on purge complete method should have run because we generated the purged uss file
            Assert.IsTrue(samplePlugin.OnPurgeCompleteRan);






        }


    }
}
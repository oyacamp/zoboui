using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using ZoboUI.Core.Plugins;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor.Tests
{
    public class TempLogger : ICustomLogger
    {
        public string Prefix { get; set; }
        public LogLevel LogLevel { get; set; }

        // Store the last logged message and its log level
        public string LastMessage { get; private set; }
        public LogLevel LastLogLevel { get; private set; }

        public string FormatMessage(string message)
        {
            return string.IsNullOrEmpty(Prefix) ? message : $"{Prefix} : {message}";
        }

        public void Log(string message)
        {
            if (LogLevel <= LogLevel.Info)
            {
                LastMessage = FormatMessage(message);
                LastLogLevel = LogLevel.Info;
            }
        }

        public void LogWarning(string message)
        {
            if (LogLevel <= LogLevel.Warning)
            {
                LastMessage = FormatMessage(message);
                LastLogLevel = LogLevel.Warning;
            }
        }

        public void LogError(string message)
        {
            if (LogLevel <= LogLevel.Error)
            {
                LastMessage = FormatMessage(message);
                LastLogLevel = LogLevel.Error;
            }
        }

        public void LogProgress(string message)
        {
            if (LogLevel <= LogLevel.Progress)
            {
                LastMessage = FormatMessage(message);
                LastLogLevel = LogLevel.Progress;
            }
        }
    }


    [TestFixture]
    public class PluginDictionaryConverterTests
    {
        private PluginDictionaryConverter converter;
        private TempLogger tempLogger = new TempLogger();

        private readonly string testPluginAssetPath = "Packages/com.oyacamp.zoboui/Tests/Editor/ConfigDisplay/TestData/TestPlugin.asset";

        private readonly string testPluginNamespace = SampleUtilityPlugin.pluginNamespace;

        [SetUp]
        public void SetUp()
        {
            converter = new PluginDictionaryConverter(tempLogger);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(testPluginAssetPath);
        }

        [Test]
        public void TestCreatePluginFromNamespace_ValidNamespace_ReturnsPlugin()
        {
            // Arrange
            string validNamespace = testPluginNamespace;

            // Act
            BaseUtilityPlugin plugin = converter.CreatePluginScriptableObjectFromNamespace(validNamespace, testPluginAssetPath);

            // Assert
            Assert.IsNotNull(plugin);
            Assert.AreEqual(validNamespace, plugin.PluginNamespace);
            Assert.AreEqual(testPluginAssetPath, AssetDatabase.GetAssetPath(plugin));

        }

        [Test]
        public void TestCreatePluginScriptableObjectFromType_ValidType_ReturnsPluginInstance()
        {
            string validNamespace = testPluginNamespace;
            System.Type pluginType = typeof(SampleUtilityPlugin);

            BaseUtilityPlugin plugin = converter.CreatePluginScriptableObjectFromType(pluginType, testPluginAssetPath);

            Assert.IsNotNull(plugin);
            Assert.AreEqual(validNamespace, plugin.PluginNamespace);
            Assert.AreEqual(testPluginAssetPath, AssetDatabase.GetAssetPath(plugin));

        }

        [Test]
        public void TestCreatePluginScriptableObjectFromType_InvalidType_ReturnsNull()
        {
            System.Type pluginType = typeof(string);

            BaseUtilityPlugin plugin = converter.CreatePluginScriptableObjectFromType(pluginType, testPluginAssetPath);

            Assert.IsNull(plugin);

        }

        [Test]
        public void TestCreatePluginFromNamespace_InvalidNamespace_ReturnsNull()
        {
            string invalidNamespace = "InvalidPluginNamespace";

            BaseUtilityPlugin plugin = converter.CreatePluginScriptableObjectFromNamespace(invalidNamespace, testPluginAssetPath);

            Assert.IsNull(plugin);
        }


        [Test]
        public void TestConvertToDisplay_ValidPluginDictionary_ReturnsDisplayList()
        {
            // Arrange
            PluginDictionary pluginDictionary = new PluginDictionary();

            string sampleData = "Test Data";

            pluginDictionary.Add(testPluginNamespace, new PluginValueHolder()
            {
                data = sampleData,
                assetPath = testPluginAssetPath,
                typeName = nameof(SampleUtilityPlugin)

            });

            // Act
            List<InspectorPluginDictionaryDisplay> displayList = converter.ConvertToDisplay(pluginDictionary);

            // Assert
            Assert.IsNotNull(displayList);
            Assert.AreEqual(1, displayList.Count);

            var displayItem = displayList[0];

            Assert.AreEqual(testPluginNamespace, displayItem.Value.PluginNamespace);

            // Check that it uses the data from the original plugin
            Assert.AreEqual(sampleData, ((SampleUtilityPlugin)displayItem.Value).GeneratedFileName);
            Assert.AreEqual(testPluginAssetPath, AssetDatabase.GetAssetPath(displayItem.Value));

        }

        [Test]
        public void TestConvertFromDisplay_ValidDisplayList_ReturnsPluginDictionary()
        {
            // Arrange
            List<InspectorPluginDictionaryDisplay> displayList = new List<InspectorPluginDictionaryDisplay>();

            string sampleData = "Test Data";

            SampleUtilityPlugin samplePlugin = converter.CreatePluginScriptableObjectFromType(typeof(SampleUtilityPlugin), testPluginAssetPath) as SampleUtilityPlugin;

            samplePlugin.GeneratedFileName = sampleData;


            displayList.Add(new InspectorPluginDictionaryDisplay()
            {
                Value = samplePlugin
            });

            // Act
            PluginDictionary pluginDictionary = converter.ConvertFromDisplay(displayList);

            // Assert
            Assert.IsNotNull(pluginDictionary);
            Assert.AreEqual(1, pluginDictionary.Count);

            PluginValueHolder pluginValueHolder = pluginDictionary[testPluginNamespace];

            Assert.IsNotNull(pluginValueHolder);

            Assert.AreEqual(sampleData, pluginValueHolder.data);

            Assert.AreEqual(testPluginAssetPath, pluginValueHolder.assetPath);

            Assert.AreEqual(nameof(SampleUtilityPlugin), pluginValueHolder.typeName);

        }

        /// <summary>
        /// This checks that the converter doesn't convert displays that have null values
        /// </summary>
        [Test]
        public void TestConvertFromDisplay_InvalidDisplayList_ReturnsEmptyPluginDictionary()
        {
            List<InspectorPluginDictionaryDisplay> displayList = new List<InspectorPluginDictionaryDisplay>();

            displayList.Add(new InspectorPluginDictionaryDisplay()
            {
                Value = null
            });

            PluginDictionary pluginDictionary = converter.ConvertFromDisplay(displayList);

            Assert.IsNotNull(pluginDictionary);
            Assert.AreEqual(0, pluginDictionary.Count);

        }

        [Test]
        public void TestConvertToDisplay_InvalidPluginDictionary_ReturnsEmptyDisplayList()
        {
            PluginDictionary pluginDictionary = new PluginDictionary();

            pluginDictionary.Add("com.invalid.namespace", new PluginValueHolder()
            {
                data = "Test Data",
                assetPath = "",
                typeName = "NONEXISTENTTYPE"

            });

            List<InspectorPluginDictionaryDisplay> displayList = converter.ConvertToDisplay(pluginDictionary);

            Assert.IsNotNull(displayList);
            Assert.AreEqual(0, displayList.Count);

            Assert.AreEqual("Could not find plugin with namespace com.invalid.namespace and type 'ZoboUI.Editor.PluginValueHolder'. Please make sure the plugin is installed in the project.", tempLogger.LastMessage);


        }

        [Test]
        public void TestPluginDictionary_ConvertedToBaseCoreProperty()
        {
            PluginDictionary pluginDictionary = new PluginDictionary();

            pluginDictionary.Add(testPluginNamespace, new PluginValueHolder()
            {
                data = "Test Data",
                assetPath = testPluginAssetPath,
                typeName = nameof(SampleUtilityPlugin)

            });

            var baseCoreProperty = pluginDictionary.ConvertToBaseCorePropertyValueDisplay("Plugins");

            Assert.IsNotNull(baseCoreProperty);
            Assert.AreEqual("Plugins", baseCoreProperty.PropertyName);
            Assert.AreEqual(CorePropertyValueType.PluginDictionary, baseCoreProperty.ValueType);
            Assert.IsNotNull(baseCoreProperty.ValuesPluginDictionary);
            Assert.AreEqual(1, baseCoreProperty.ValuesPluginDictionary.Count);
            Assert.AreEqual(testPluginAssetPath, AssetDatabase.GetAssetPath(baseCoreProperty.ValuesPluginDictionary[0].Value));

        }
    }
}
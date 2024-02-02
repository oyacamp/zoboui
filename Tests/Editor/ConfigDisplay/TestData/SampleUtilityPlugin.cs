using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoboUI.Core;
using ZoboUI.Core.Plugins;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor.Tests
{
    public class SampleUtilityPlugin : BaseUtilityPlugin
    {
        public static readonly string pluginNamespace = "com.oyacamp.zoboui.tests.sampleutilityplugin";

        public override string PluginUtilityName => "Sample Utility Plugin";

        public override string PluginDescription => "This is a sample utility plugin for testing";

        public override RuleGenerationPriority RuleGenerationPriority => RuleGenerationPriority.AfterDefaultUtilities;

        public override string PluginNamespace => pluginNamespace;

        [SerializeField] private string generatedFileName = "SampleUtilityPluginTestFile";

        public string GeneratedFileName { get => generatedFileName; set => generatedFileName = value; }

        public override void FromJson(string jsonString)
        {
            generatedFileName = jsonString;
        }

        public override string ToJson()
        {
            return generatedFileName;
        }

        private IProcessThemeConfig customThemeConfigProcessor;
        private IUseCustomClassExtractor customClassProvider;

        // Add a constructor that takes IProcessThemeConfig as a parameter and IUseCustomClassExtractor as a parameter
        public SampleUtilityPlugin(IProcessThemeConfig customThemeConfigProcessor = null, IUseCustomClassExtractor customClassProvider = null)
        {
            // Do something with the parameters
            this.customThemeConfigProcessor = customThemeConfigProcessor;
            this.customClassProvider = customClassProvider;
        }

        override public void ProcessThemeConfig(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger)
        {
            this.customThemeConfigProcessor?.ProcessThemeConfig(themeConfig, bag, logger);
        }

        public override IClassExtractor GetClassExtractor(ICustomLogger logger = null)
        {
            return this.customClassProvider?.GetClassExtractor(logger);
        }
    }

}
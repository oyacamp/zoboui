using UnityEngine;
using ZoboUI.Core;
using ZoboUI.Core.Plugins;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor.Tests
{
    /// <summary>
    /// This is a utility plugin for testing that the priority system works as expected.
    /// </summary>
    public class PriorityTestingUtlityPlugin : BaseUtilityPlugin
    {
        public static readonly string pluginNamespace = "com.oyacamp.zoboui.tests.prioritytestingutilityplugin";

        public override string PluginUtilityName => "Priority Testiing Utility Plugin";

        public override string PluginDescription => "This is a priority utility plugin for testing";

        public override RuleGenerationPriority RuleGenerationPriority { get => ruleGenerationPriority; }

        public override string PluginNamespace => pluginNamespace;


        [SerializeField] private RuleGenerationPriority ruleGenerationPriority = RuleGenerationPriority.BeforeDefaultUtilities;

        [SerializeField] private int m_LastBagItemCount = -1;

        [SerializeField] private bool classExtractorRan = false;


        [SerializeField] private bool onPurgeCompleteRan = false;

        public int LastBagItemCount { get => m_LastBagItemCount; }

        public bool ClassExtractorRan { get => classExtractorRan; }

        public bool OnPurgeCompleteRan { get => onPurgeCompleteRan; }


        public override void FromJson(string jsonString)
        {

        }

        public override string ToJson()
        {
            return "";
        }



        public void UpdatePriority(RuleGenerationPriority priority)
        {
            ruleGenerationPriority = priority;
        }


        override public void ProcessThemeConfig(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger)
        {
            m_LastBagItemCount = bag.Count;
        }

        public override IClassExtractor GetClassExtractor(ICustomLogger logger = null)
        {
            classExtractorRan = true;
            return null;
        }

        public override void OnPurgeComplete(ThemeConfig themeConfig, UtilityRuleBag bag, ICustomLogger logger = null)
        {
            onPurgeCompleteRan = true;
        }
    }
}
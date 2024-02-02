using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ZoboUI.Editor.Attributes
{

    /// <summary>
    /// Use this attribute to signify that the utility's classes should be generated before or after the classes of another utility. By default, the classes are generated based on the order of the utilities in the ThemeConfig's properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UtilityGenerationOrderAttribute : PropertyAttribute
    {
        public enum GenerationOrder { BeforeOtherUtilites = 0, AfterOtherUtilities = 1 }

        public GenerationOrder Order { get; private set; }

        /// <summary>
        /// This determines the order in which the utility's classes will be generated alongside other utilities with the same order. The lower the number, the earlier the generation.
        /// </summary>
        public int GenerationIndex { get; private set; }

        public UtilityGenerationOrderAttribute(GenerationOrder order, int generationIndex = 0)
        {
            Order = order;
            GenerationIndex = generationIndex;
        }

    }
}
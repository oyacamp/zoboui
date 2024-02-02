using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZoboUI.Editor.Attributes
{
    /// <summary>
    /// Use this attribute to signify that the field or property in a ThemeConfig should have its uss dictionary hidden in the inspector. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HideUssDictionaryInDisplayAttribute : PropertyAttribute
    {
        public HideUssDictionaryInDisplayAttribute()
        { }
    }
}
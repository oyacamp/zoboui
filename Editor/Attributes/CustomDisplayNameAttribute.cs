using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZoboUI.Editor.Attributes
{

    /// <summary>
    /// Use this attribute to signify that the field or property in a ThemeConfig should be displayed with a custom name in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CustomDisplayNameAttribute : PropertyAttribute
    {
        public string DisplayName { get; private set; }

        public CustomDisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
    }
}
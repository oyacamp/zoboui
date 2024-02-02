using System;
using UnityEngine;

namespace ZoboUI.Editor.Attributes
{

    /// <summary>
    /// Use this attribute to signify that the values of field or property in a ThemeConfig can be extended by utilities. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class CanBeExtendedAttribute : PropertyAttribute
    {

    }
}
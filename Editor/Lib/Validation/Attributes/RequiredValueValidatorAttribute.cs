using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoboUI.Core.Utils;
using ZoboUI.Editor.Utils;

namespace ZoboUI.Editor.Attributes
{
    /// <summary>
    /// A custom attribute that signifies that the value of a field is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredValueValidatorAttribute : PropertyAttribute, IConfigValueValidator
    {
        public string GetErrorMessage()
        {
            return "This field is required.";
        }

        public bool ValueIsValid(Type type, object value, ICustomLogger logger = null)
        {
            if (value == null)
            {
                return false;
            }

            return true;
        }
    }
}
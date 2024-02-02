using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoboUI.Core.Utils;
using ZoboUI.Editor.Utils;


namespace ZoboUI.Editor.Attributes
{

    /// <summary>
    /// A custom attribute that signifies that a string value field is required.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequiredStringValueValidatorAttribute : PropertyAttribute, IConfigValueValidator
    {


        public string GetErrorMessage()
        {
            return "This field is required.";
        }

        public bool ValueIsValid(Type type, object value, ICustomLogger logger = null)
        {
            if (type == typeof(string))
            {
                string stringValue = value as string;


                return !string.IsNullOrEmpty(stringValue);
            }
            else
            {
                logger?.LogWarning($"{nameof(RequiredStringValueValidatorAttribute)} should only be used on string fields.");
            }

            return true;
        }


    }

}
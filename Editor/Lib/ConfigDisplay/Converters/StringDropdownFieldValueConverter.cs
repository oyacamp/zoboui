using System;
using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEngine;

namespace ZoboUI.Editor
{

    /// <summary>
    /// A custom class that holds a string value and signifies that the value should be displayed as a dropdown in the inspector.
    /// </summary>
    [System.Serializable]
    public class StringDropdownFieldValue
    {

        /// <summary>
        /// The string value 
        /// </summary>
        public string Value;



    }

    /// <summary>
    /// A custom class that holds a string value and signifies that the value should be displayed as a dropdown of utilities and core properties to extend.
    /// </summary>
    [System.Serializable]
    public class ExtendFieldsStringDropdownFieldValue : StringDropdownFieldValue
    {
    }

    /// <summary>
    /// A custom class that holds a string value and signifies that the value should be displayed as a dropdown of class modifiers.
    /// </summary>
    [System.Serializable]
    public class ClassModifierStringDropdownFieldValue : StringDropdownFieldValue
    {
    }

    public class StringDropdownFieldValueListConverter<T> : IConvertibleToDisplay<List<T>, StringListValue> where T : StringDropdownFieldValue, new()
    {


        public List<T> ConvertToDisplay(StringListValue model)
        {
            var displayList = new List<T>();

            foreach (var item in model)
            {
                displayList.Add(new T { Value = item });
            }

            return displayList;
        }

        public StringListValue ConvertFromDisplay(List<T> display)
        {
            var model = new StringListValue();

            foreach (var item in display)
            {
                model.Add(item.Value);
            }

            return model;
        }
    }


    [System.Serializable]
    public class RequiredStringDropdownInfo
    {


        /// <summary>
        /// Allows property drawers to access the modifier names as choices for the dropdown
        /// </summary>
        public string ModifierPropertyDisplayPropertyName;

        // Add an event delegate to notify the property drawer when the modifier names have been updated
        public event Action OnModifierValuesUpdated;

        /// <summary>
        /// This method is called when the modifier values are updated. It notifies the property drawer that the modifier values have been updated.
        /// </summary>
        public void NotifyModifierValuesUpdated()
        {
            OnModifierValuesUpdated?.Invoke();
        }


        /// <summary>
        /// This list holds the names of fields that other fields can extend. Fields should only be able to extend fields of the same type. e.g. a StringValueDictionary can only extend other StringValueDictionaries.
        /// </summary>
        public List<CorePropertyValueTypeToExtensibleFieldsMapItem> ValueTypeToFieldExtensionNamesList;



    }

}

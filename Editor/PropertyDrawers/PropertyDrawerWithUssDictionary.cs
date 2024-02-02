using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Editor.Utils;


namespace ZoboUI.Editor.PropertyDrawers
{
    /// <summary>
    /// A property drawer that hides the uss dictionary if the parent property requires it.
    /// </summary>
    public abstract class PropertyDrawerWithUssDictionary : PropertyDrawerWithUniqueKey
    {

        protected abstract VisualElement RenderPropertyGUI(SerializedProperty property);

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            VisualElement renderedPropertyGUI = RenderPropertyGUI(property);

            CustomUssPropertyDisplayListView customUssPropertyDisplayListView = renderedPropertyGUI.Q<CustomUssPropertyDisplayListView>();

            // Loop through the parent properties to find if this is under the BaseCorePropertyDisplay class
            SerializedProperty parentProperty = property;
            while (parentProperty != null)
            {
                if (parentProperty.type == nameof(BaseCorePropertyValueDisplay))
                {
                    break;
                }

                parentProperty = SerializedPropertyHelper.GetParentProperty(parentProperty);
            }

            if (parentProperty == null)
            {
                throw new Exception("Could not find parent property of type BaseCorePropertyValueDisplay");
            }

            BaseCorePropertyValueDisplay baseCorePropertyValueDisplay = SerializedPropertyHelper.GetArrayFieldValue<BaseCorePropertyValueDisplay>(parentProperty);
            // Check if the parent property requires that we hide the uss dictionary
            bool hideUssDictionary = baseCorePropertyValueDisplay != null ? baseCorePropertyValueDisplay.HideUssDictionaryIfPresent : false;

            if (hideUssDictionary)
            {
                customUssPropertyDisplayListView.style.display = DisplayStyle.None;
            }

            // Return the finished UI
            return renderedPropertyGUI;
        }
    }
}

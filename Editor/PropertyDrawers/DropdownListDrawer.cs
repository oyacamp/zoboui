using System;
using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using ZoboUI.Editor.Attributes;
using UnityEditor;
using UnityEngine;


namespace ZoboUI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(DropdownListAttribute))]
    public class DropdownListDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DropdownListAttribute dropdownListAttribute = (DropdownListAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.String)
            {
                int selectedIndex = Array.IndexOf(dropdownListAttribute.choices, property.stringValue);
                selectedIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, dropdownListAttribute.choices);
                property.stringValue = dropdownListAttribute.choices[selectedIndex];
            }
            else if (property.propertyType == SerializedPropertyType.ArraySize)
            {
                EditorGUI.PropertyField(position, property, label);
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use DropdownList with string or string[].");
            }
        }


    }
}
using System;
using System.Collections.Generic;
using ZoboUI.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{
    public abstract class BaseDisplayDrawerWithAssetValue : PropertyDrawerWithUssDictionary
    {




        /// <summary>
        /// Checks if the display type is a custom value. If it is, the custom value field is displayed and the object value field is hidden. If it is not, the object value field is displayed and the custom value field is hidden.
        /// </summary>
        /// <param name="displayType"></param>
        /// <returns></returns>
        protected abstract bool DisplayTypeisCustomValue(System.Enum displayType);

        /// <summary>
        /// Returns the type that the object value field should be set to based on the display type
        /// </summary>
        /// <param name="displayType"></param>
        /// <returns></returns>
        protected abstract Type GetTypeValueForDisplayType(System.Enum displayType);

        public void SetDisplay(System.Enum displayType, ObjectField objectValueField, TextField customValueField)
        {
            bool displayTypeIsCustomValue = DisplayTypeisCustomValue(displayType);

            if (displayTypeIsCustomValue)
            {
                customValueField.style.display = DisplayStyle.Flex;
                objectValueField.style.display = DisplayStyle.None;
                return;
            }
            else
            {
                customValueField.style.display = DisplayStyle.None;
                objectValueField.style.display = DisplayStyle.Flex;

                //Set the value field object type
                objectValueField.objectType = GetTypeValueForDisplayType(displayType);

            }




        }

        /// <summary>
        /// Returns the name of the property that holds the display type on the property
        /// </summary>
        /// <returns></returns>
        protected abstract string GetDisplayTypePropertyName();


        /// <summary>
        /// Converts the int value of the display type property to an enum
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected abstract Enum ConvertPropertyIntValueToEnum(int value);



        protected override VisualElement RenderPropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/AssetValueDictionaryDisplay_UXML.uxml");
            visualTree.CloneTree(container);



            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");



            TextField keyField = container.Q<TextField>("KeyField");
            keyField.bindingPath = nameof(InspectorImageValueDictionaryDisplay.Key);

            ObjectField valueField = container.Q<ObjectField>("ObjectValueField");
            valueField.bindingPath = nameof(InspectorImageValueDictionaryDisplay.ValueObject);
            TextField customValueTextField = container.Q<TextField>("CustomValueField");
            customValueTextField.bindingPath = nameof(InspectorImageValueDictionaryDisplay.CustomStringValue);
            EnumField displayTypeEnumField = container.Q<EnumField>("DisplayTypeEnumField");
            displayTypeEnumField.bindingPath = nameof(InspectorImageValueDictionaryDisplay.DisplayType);


            if (customValueTextField == null)
            {
                throw new Exception("Custom value text field is null");
            }

            if (keyField == null)
            {
                throw new Exception("Key field is null");

            }

            if (valueField == null)
            {
                throw new Exception("Value field is null");
            }

            if (displayTypeEnumField == null)
            {
                throw new Exception("Display type enum field is null");
            }


            string displayTypePropertyName = GetDisplayTypePropertyName();

            // Get the DisplayType property
            SerializedProperty displayTypeProperty = property.FindPropertyRelative(displayTypePropertyName);



            Enum currentDisplayType = ConvertPropertyIntValueToEnum(displayTypeProperty.intValue);



            SetDisplay(currentDisplayType, valueField, customValueTextField);

            container.style.flexDirection = FlexDirection.Column;







            HandleKeyValidation(property, keyField, alertElement);


            displayTypeEnumField.RegisterValueChangedCallback((evt) =>
            {
                if (displayTypeEnumField != null && evt.newValue != null)
                {
                    SetDisplay(evt.newValue, valueField, customValueTextField);

                    if (evt.newValue != evt.previousValue)
                    {
                        // If the display type changed, set the value field to null
                        valueField.value = null;
                        customValueTextField.value = "";
                    }
                }
            });



            // Return the finished UI
            return container;
        }
    }


}


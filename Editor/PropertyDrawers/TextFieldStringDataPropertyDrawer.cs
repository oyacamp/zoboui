using ZoboUI.Editor.Attributes;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(TextFieldStringData))]
    public class TextFieldStringDataPropertyDrawer : PropertyDrawer
    {


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            var container = new VisualElement();

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/TextFieldStringDataDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            TextField textField = container.Q<TextField>("TextField");

            bool IsMultiline = property.FindPropertyRelative(nameof(TextFieldStringData.isMultiline)).boolValue;

            textField.multiline = IsMultiline;

            textField.bindingPath = nameof(TextFieldStringData.Value);
            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");
            PropertyDrawerWithUniqueKey.HideErrorIndicator(alertElement);

            // Check if the parent is a list 
            SerializedProperty parentProperty = SerializedPropertyHelper.GetParentProperty(property);



            // Property drawers that reference this property as a PropertyField are not able to use Q to get the nested elements in order to set the header title, so we have them set the label field on the PropertyField and reference the value as preferred label. Read more: https://forum.unity.com/threads/how-can-i-gets-the-label-from-propertydrawer-createpropertygui.1184404/
            // We check if the parent is a list because we don't want to set the label on the text field if it's a list because it will be set to the value of the item in the list
            if (this.preferredLabel != null && parentProperty != null && !parentProperty.isArray)
            {

                textField.label = this.preferredLabel;
            }


            // Subscribe to the change event on the text field
            textField.RegisterValueChangedCallback(evt =>
            {
                TextFieldStringData textFieldStringData = SerializedPropertyHelper.GetArrayFieldValue<TextFieldStringData>(property);

                if (textFieldStringData == null)
                {
                    Debug.LogError("Could not find the TextFieldStringData at the property path");
                    return;
                }

                BaseTextFieldValidator validator = textFieldStringData.Validator;

                if (validator == null)
                {
                    return;
                }

                var validationResult = validator.Validate(evt.newValue);

                // If the string isn't valid, show an error indicator

                if (!validationResult.IsValid)
                {
                    PropertyDrawerWithUniqueKey.ShowErrorIndicator(alertElement, validationResult.ErrorMessage);
                    return;
                }
                else
                {
                    PropertyDrawerWithUniqueKey.HideErrorIndicator(alertElement);
                }


            });

            return container;
        }
    }
}
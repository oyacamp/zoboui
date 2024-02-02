using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Core.Plugins;
using ZoboUI.Editor.Utils;

namespace ZoboUI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(InspectorPluginDictionaryDisplay))]
    public class PluginDictionaryDisplayDrawer : PropertyDrawer
    {


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            var container = new VisualElement();

            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/PluginDictionaryDisplayDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            UnityEditor.UIElements.ObjectField objectField = container.Q<UnityEditor.UIElements.ObjectField>("PluginObjectField");

            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            Label displayNameLabel = container.Q<Label>();

            string DisplayName = SerializedPropertyHelper.GetArrayFieldValue<BaseUtilityPlugin>(property)?.PluginUtilityName;

            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = "Please select a Plugin from the project";
            }

            if (objectField == null)
            {
                Debug.LogError("Could not find the ObjectField in the UXML");
            }

            if (alertElement == null)
            {
                Debug.LogError("Could not find the AlertElement in the UXML");
            }


            objectField.bindingPath = nameof(InspectorPluginDictionaryDisplay.Value);
            PropertyDrawerWithUniqueKey.HideErrorIndicator(alertElement);
            displayNameLabel.text = DisplayName;


            // Subscribe to the change event on the text field
            objectField.RegisterValueChangedCallback(evt =>
            {

                if (evt.newValue != null)
                {
                    BaseUtilityPlugin plugin = (BaseUtilityPlugin)evt.newValue;

                    if (plugin == null)
                    {
                        Debug.LogError("Could not find the Plugin at the property path");
                        return;
                    }
                    displayNameLabel.text = plugin.PluginUtilityName;
                }


                if (!evt.newValue)
                {
                    PropertyDrawerWithUniqueKey.ShowErrorIndicator(alertElement, "No plugin object selected");
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
using ZoboUI.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(ClassModifierDisplay))]
    public class ClassModifierDictionaryDisplayPropertyDrawer : PropertyDrawerWithUniqueKey
    {



        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();



            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/ClassModifierDictionaryDrawer_Display_Inspector_UXML.uxml");
            visualTree.CloneTree(container);

            //alertElement = new AlertElement();
            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            if (alertElement == null)
            {
                Debug.LogError("Could not find the AlertElement in the UXML");
            }

            TextField keyField = container.Q<TextField>("KeyField");

            keyField.bindingPath = nameof(ClassModifierDisplay.ModifierName);

            TextField valueField = container.Q<TextField>("ValueField");

            valueField.bindingPath = nameof(ClassModifierDisplay.Value);

            ThemeConfigManager themeConfigManager = property.serializedObject.targetObject as ThemeConfigManager;

            ThemeConfigDisplayVersion configDisplayVersion = themeConfigManager.ThemeConfigDisplay;


            // When an item is removed or added to the parent list and rebound, the key field and value field values can get out of sync with the property values if the item removed or added is not the last item in the list. 
            // So we need to make sure that the key field and value field values are always in sync with the property values.
            keyField.RegisterValueChangedCallback(evt =>
            {
                string keyStringValue = property.FindPropertyRelative(ClassModifierDisplay.GetUniqueKeyPropertyName()).stringValue;

                if (keyField.value != keyStringValue)
                {
                    keyField.value = keyStringValue;
                }

                // We need to notify the config display version that the modifier names have been updated so that the dropdowns can be updated
                /*
                if (configDisplayVersion?.RequiredStringDropdownInfoInstance != null)
                {
                    configDisplayVersion.RequiredStringDropdownInfoInstance.NotifyModifierValuesUpdated();
                }*/
            });

            valueField.RegisterValueChangedCallback(evt =>
            {
                string valueStringValue = property.FindPropertyRelative(ClassModifierDisplay.GetValuePropertyName()).stringValue;

                if (valueField.value != valueStringValue)
                {
                    valueField.value = valueStringValue;
                }
            });





            HandleKeyValidation(property, keyField, alertElement);


            // Return the finished UI
            return container;
        }

    }
}
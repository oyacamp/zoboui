using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(InspectorKeyValueDictionaryDisplay))]
    public class KeyValueDictionaryDisplayPropertyDrawer : PropertyDrawerWithUssDictionary
    {





        protected override VisualElement RenderPropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();



            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/KeyValueDictionaryDrawer_Display_Inspector_UXML.uxml");
            visualTree.CloneTree(container);

            //alertElement = new AlertElement();
            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            if (alertElement == null)
            {
                Debug.LogError("Could not find the AlertElement in the UXML");
            }

            TextField keyField = container.Q<TextField>("KeyField");
            keyField.bindingPath = nameof(InspectorKeyValueDictionaryDisplay.Key);

            TextField valueField = container.Q<TextField>("ValueField");
            valueField.bindingPath = nameof(InspectorKeyValueDictionaryDisplay.Value);


            // When an item is removed or added to the parent list and rebound, the key field and value field values can get out of sync with the property values if the item removed or added is not the last item in the list. 
            // So we need to make sure that the key field and value field values are always in sync with the property values.
            keyField.RegisterValueChangedCallback(evt =>
            {
                string keyStringValue = property.FindPropertyRelative(InspectorKeyValueDictionaryDisplay.GetUniqueKeyPropertyName()).stringValue;

                if (keyField.value != keyStringValue)
                {
                    keyField.value = keyStringValue;
                }
            });

            valueField.RegisterValueChangedCallback(evt =>
            {
                string valueStringValue = property.FindPropertyRelative(InspectorKeyValueDictionaryDisplay.GetValuePropertyName()).stringValue;

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
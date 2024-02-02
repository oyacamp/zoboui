using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(ClassTagToUssPropertyMapDisplay))]
    public class TagPropertyMapDrawer : PropertyDrawerWithUniqueKey
    {



        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();



            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/TagPropertyMapDrawer_Display_UXML.uxml");
            visualTree.CloneTree(container);

            //alertElement = new AlertElement();
            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            if (alertElement == null)
            {
                Debug.LogError("Could not find the AlertElement in the UXML");
            }

            TextField keyField = container.Q<TextField>("KeyField");
            keyField.bindingPath = nameof(ClassTagToUssPropertyMapDisplay.ClassTag);

            PropertyField PropertyNamesListView = container.Q<PropertyField>("PropertyNamesListView");
            PropertyNamesListView.bindingPath = nameof(ClassTagToUssPropertyMapDisplay.UssPropertyNames);



            // When an item is removed or added to the parent list and rebound, the key field and value field values can get out of sync with the property values if the item removed or added is not the last item in the list. 
            // So we need to make sure that the key field and value field values are always in sync with the property values.
            keyField.RegisterValueChangedCallback(evt =>
            {
                string keyInProperty = property.FindPropertyRelative("ClassTag").stringValue;
                if (keyField.value != keyInProperty)
                {
                    keyField.value = keyInProperty;
                }
            });





            // We want to allow empty strings for the key field, so we specify that here
            HandleKeyValidation(property, keyField, alertElement, ValidationMode.ConflictingKeys);


            // Return the finished UI
            return container;
        }

    }
}
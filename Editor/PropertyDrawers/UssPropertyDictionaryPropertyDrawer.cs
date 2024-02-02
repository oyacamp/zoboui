using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(UssPropertyHolderDisplay))]
    public class UssPropertyHolderDrawer : PropertyDrawerWithUniqueKey
    {



        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();



            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/UssPropertyDictionaryDrawer_Display_Inspector_UXML.uxml");
            visualTree.CloneTree(container);

            //alertElement = new AlertElement();
            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            if (alertElement == null)
            {
                Debug.LogError("Could not find the AlertElement in the UXML");
            }

            TextField keyField = container.Q<TextField>("KeyField");
            keyField.bindingPath = nameof(UssPropertyHolderDisplay.PropertyName);

            TextField valueField = container.Q<TextField>("ValueField");
            valueField.bindingPath = nameof(UssPropertyHolderDisplay.PropertyValue);


            // When an item is removed or added to the parent list and rebound, the key field and value field values can get out of sync with the property values if the item removed or added is not the last item in the list. 
            // So we need to make sure that the key field and value field values are always in sync with the property values.
            keyField.RegisterValueChangedCallback(evt =>
            {
                string keyInProperty = property.FindPropertyRelative("PropertyName").stringValue;
                if (keyField.value != keyInProperty)
                {
                    keyField.value = keyInProperty;
                }
            });

            valueField.RegisterValueChangedCallback(evt =>
            {
                string valueInProperty = property.FindPropertyRelative("PropertyValue").stringValue;

                if (valueField.value != valueInProperty)
                {
                    valueField.value = valueInProperty;
                }
            });





            HandleKeyValidation(property, keyField, alertElement);


            // Return the finished UI
            return container;
        }

    }
}
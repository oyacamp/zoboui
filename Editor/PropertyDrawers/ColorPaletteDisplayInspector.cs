using ZoboUI.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{

    [CustomPropertyDrawer(typeof(InspectorColorPaletteDisplay))]
    public class ColorPaletteDisplayInspector : PropertyDrawerWithUssDictionary
    {



        protected override VisualElement RenderPropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/ColorPalette_Display_Inspector_UXML.uxml");
            visualTree.CloneTree(container);

            AlertElement alertElement = container.Q<AlertElement>("ErrorAlertElement");

            TextField keyField = container.Q<TextField>("PaletteNameField");

            keyField.bindingPath = nameof(InspectorColorPaletteDisplay.PaletteName);

            ListView swatchListView = container.Q<ListView>("SwatchListView");

            swatchListView.bindingPath = nameof(InspectorColorPaletteDisplay.Swatches);




            HandleKeyValidation(property, keyField, alertElement);



            // Return the finished UI
            return container;
        }
    }

    [CustomPropertyDrawer(typeof(InspectorColorSwatchDisplay))]
    public class ColorSwatchDisplayInspector : PropertyDrawer
    {



        private void HideVisualElement(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.None;
        }

        private void ShowVisualElement(VisualElement visualElement)
        {
            visualElement.style.display = DisplayStyle.Flex;
        }

        private void ShowFieldsBasedOnCustomValueMode(bool useCustomColorString, TextField colorStringField, ColorField swatchColorField)
        {
            if (useCustomColorString)
            {
                ShowVisualElement(colorStringField);
                HideVisualElement(swatchColorField);
            }
            else
            {
                HideVisualElement(colorStringField);
                ShowVisualElement(swatchColorField);
            }
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/ColorSwatchDisplay_Display_Inspector_UXML.uxml");
            visualTree.CloneTree(container);

            ColorField swatchColorField = container.Q<ColorField>("SwatchColorField");
            swatchColorField.bindingPath = nameof(InspectorColorSwatchDisplay.Color);
            TextField colorStringField = container.Q<TextField>("ColorStringField");
            colorStringField.bindingPath = nameof(InspectorColorSwatchDisplay.CustomStringValue);

            TextField swatchNameField = container.Q<TextField>("SwatchNameField");

            swatchNameField.bindingPath = nameof(InspectorColorSwatchDisplay.Name);
            UnityEngine.UIElements.Toggle useCustomColorStringToggle = container.Q<UnityEngine.UIElements.Toggle>("UseCustomValueToggle");

            // Get the color string type property
            SerializedProperty colorStringTypeProperty = property.FindPropertyRelative("ColorStringType");

            // Log an error if the property is null
            if (colorStringTypeProperty == null)
            {
                Debug.LogError("ColorStringType property is null");
            }

            ColorStringType colorStringType = (ColorStringType)colorStringTypeProperty.intValue;

            useCustomColorStringToggle.value = colorStringType == ColorStringType.Custom;

            ShowFieldsBasedOnCustomValueMode(colorStringType == ColorStringType.Custom, colorStringField, swatchColorField);

            useCustomColorStringToggle.RegisterValueChangedCallback(evt =>
            {
                bool useCustomColorString = evt.newValue;

                ShowFieldsBasedOnCustomValueMode(useCustomColorString, colorStringField, swatchColorField);

                if (useCustomColorString)
                {
                    colorStringTypeProperty.intValue = (int)ColorStringType.Custom;
                }
                else
                {
                    colorStringTypeProperty.intValue = (int)ColorStringType.Hex;
                }

                // Apply the changes to the serialized property
                colorStringTypeProperty.serializedObject.ApplyModifiedProperties();

            });


            // Return the finished UI
            return container;
        }
    }
}
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ZoboUI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(UtilityConfigDisplay), true)]
    public class UtilityConfigDisplayPropertyDrawer : PropertyDrawer
    {

        private bool isUserInteractingWithFoldout = false;
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/UtilityConfigDisplayPropertyDrawer_UXML.uxml");
            visualTree.CloneTree(container);
            var foldout = container.Q<Foldout>();
            foldout.focusable = true;
            string foldoutPropertyName = property.FindPropertyRelative(nameof(UtilityConfigDisplay.DisplayName)).stringValue;

            ListView ClassVariationsListView = container.Q<ListView>("ClassVariations");
            ClassVariationsListView.bindingPath = nameof(UtilityConfigDisplay.ModifierVariations);

            ListView ExtendFieldsListView = container.Q<ListView>("ExtendFields");
            ExtendFieldsListView.bindingPath = nameof(UtilityConfigDisplay.ExtendFields);

            ListView ClassTagToUssPropertyMapListView = container.Q<ListView>("ClassTagToUssPropertyMap");

            ClassTagToUssPropertyMapListView.bindingPath = nameof(UtilityConfigDisplay.ClassTagToUssPropertyMap);

            PropertyField ValuesPropertyField = container.Q<PropertyField>("Values");
            ValuesPropertyField.bindingPath = nameof(UtilityConfigDisplay.PropertyDisplay);





            // For some reason the viewDataKey is not working as expected for this property drawer, so we're using the SessionState instead to save the foldout state


            string viewDataKey = "foldout-utilprop-" + property.propertyPath + "-" + foldoutPropertyName;
            foldout.viewDataKey = viewDataKey;
            // Register focus events
            foldout.RegisterCallback<FocusInEvent>(e => isUserInteractingWithFoldout = true);
            foldout.RegisterCallback<BlurEvent>(e => isUserInteractingWithFoldout = false);


            // Register callback for foldout state change
            foldout.RegisterValueChangedCallback(evt =>
            {


                bool isFocused = foldout.focusController.focusedElement == foldout || foldout.focusController.focusedElement == container || foldout.focusController.focusedElement == container.parent || isUserInteractingWithFoldout;

                if (isUserInteractingWithFoldout)
                {
                    SessionState.SetBool(viewDataKey, evt.newValue);
                }
            });



            // Restore foldout state

            bool savedState = SessionState.GetBool(viewDataKey, false);
            if (savedState != foldout.value)
            {
                foldout.value = savedState;

            }

            foldout.text = foldoutPropertyName;


            // We can use the following code to copy the property name to the clipboard so that users can extend the property
            container.RegisterCallback<ContextClickEvent>(evt =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Theme Property Name"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = PropertyFormatter.FormatPropertyExtensionName(foldoutPropertyName, PropertyFormatter.PropertyExtensionContext.Utilities);
            });
            menu.ShowAsContext();
        });




            // Return the finished UI
            return container;
        }
    }




}
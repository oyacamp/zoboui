using UnityEditor;
using UnityEngine.UIElements;


namespace ZoboUI.Editor.PropertyDrawers
{


    [CustomPropertyDrawer(typeof(ThemeConfigDisplayVersion))]
    public class ThemeConfigDisplayVersionProprtyDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/Theme_Config_Display_PropertyDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            ListView CoreSectionPropertyHolderListView = container.Q<ListView>("CoreSectionPropertyHolderListView");
            CoreSectionPropertyHolderListView.bindingPath = nameof(ThemeConfigDisplayVersion.Core);


            ListView CompilationSectionPropertyHolderListView = container.Q<ListView>("CompilationSectionPropertyHolderListView");
            CompilationSectionPropertyHolderListView.bindingPath = nameof(ThemeConfigDisplayVersion.Compilation);

            ListView UtilitiesSectionPropertyHolderListView = container.Q<ListView>("UtilitiesSectionPropertyHolderListView");

            UtilitiesSectionPropertyHolderListView.bindingPath = nameof(ThemeConfigDisplayVersion.Utilities);

            return container;

        }

    }


}
using UnityEditor;
using UnityEngine.UIElements;

namespace ZoboUI.Editor
{
    /// <summary>
    /// Represents a list view that displays the custom uss properties of a theme config utility. We made this into a custom element so it can be reused in multiple places with the same listview settings.
    /// </summary>
    public class CustomUssPropertyDisplayListView : VisualElement
    {

        public CustomUssPropertyDisplayListView()
        {
            // Load the UXML file
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/Shared/CustomUssPropertyDisplayListView/CustomUssPropertyDisplayListView_UXML.uxml");
            visualTree.CloneTree(this);

            ListView customUssPropertyDisplayListView = this.Q<ListView>();

            customUssPropertyDisplayListView.bindingPath = nameof(DisplayWithCustomUssProperties.UssProperties);

        }


        // UxmlFactory for AlertElement
        public new class UxmlFactory : UxmlFactory<CustomUssPropertyDisplayListView, UxmlTraits> { }

        // UxmlTraits for AlertElement
        public new class UxmlTraits : VisualElement.UxmlTraits
        {

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);


            }
        }
    }

}

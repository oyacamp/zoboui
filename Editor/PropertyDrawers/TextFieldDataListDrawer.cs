using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace ZoboUI.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(TextFieldDataList))]
    public class TextFieldDataListDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/TextFieldDataListDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            ListView listView = container.Q<ListView>();

            // Get the Value property
            SerializedProperty valuesProperty = property.FindPropertyRelative(nameof(TextFieldDataList.Values));

            listView.bindingPath = nameof(TextFieldDataList.Values);//valuesProperty.propertyPath;

            // Property drawers that reference this property as a PropertyField are not able to use Q to get the nested elements in order to set the header title, so we have them set the label field on the PropertyField and reference the value as preferred label. Read more: https://forum.unity.com/threads/how-can-i-gets-the-label-from-propertydrawer-createpropertygui.1184404/
            // Make sure you only show the showBoundCollectionSize when the showFoldoutHeader is true, otherwise there's a bug where it prevents all the items from being displayed.
            if (this.preferredLabel != null)
            {
                listView.headerTitle = this.preferredLabel;
                listView.showFoldoutHeader = true;
                listView.showBoundCollectionSize = true;
            }


            // Listen for items being added to the list
            listView.itemsAdded += itemIndices =>
            {
                foreach (var itemIndex in itemIndices)
                {
                    if (itemIndex >= 0)
                    {

                        TextFieldDataList textFieldDataList = SerializedPropertyHelper.GetArrayFieldValue<TextFieldDataList>(property);

                        // Get the item at the index
                        TextFieldStringData textDataItem = textFieldDataList.Values[itemIndex];


                        if (textDataItem == null)
                        {
                            return;
                        }

                        // Set the validator on the item to the validator on the list

                        textDataItem.Validator = textFieldDataList.Validator;


                    }
                }
            };

            return container;
        }

    }
}
using System.Collections.Generic;
using ZoboUI.Editor;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using ZoboUI.Core;

namespace ZoboUI.Editor.PropertyDrawers
{
    public class ValuePreviewDisplayHandler
    {
        public class ValueTypeElementInfo
        {
            public string NameOfElementToHide { get; set; }
            public string BindingPath { get; set; }

            public string NameOfElementToBind { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="elementName"></param>
            /// <param name="bindingPath"></param>
            /// <param name="elementToBindIfDifferent"></param>
            public ValueTypeElementInfo(string elementName, string bindingPath, string elementToBindIfDifferent = null)
            {
                NameOfElementToHide = elementName;
                BindingPath = bindingPath;
                NameOfElementToBind = elementToBindIfDifferent ?? elementName;
            }
        }

        public Dictionary<CorePropertyValueType, ValueTypeElementInfo> m_ValueTypeToElementNameDict = new Dictionary<CorePropertyValueType, ValueTypeElementInfo>(){
            {CorePropertyValueType.KeyValueDictionary, new ValueTypeElementInfo("StringValueDictionary", nameof(BaseCorePropertyValueDisplay.ValuesKeyValueDictionary)) },
            {CorePropertyValueType.ColorPalette, new ValueTypeElementInfo("ColorPaletteValue", nameof(BaseCorePropertyValueDisplay.ValuesColorPalette)) },
            {CorePropertyValueType.ImageValueDictionary, new ValueTypeElementInfo("ImageDictionaryValue", nameof(BaseCorePropertyValueDisplay.ValuesImageValueDictionary)) },
            {CorePropertyValueType.FontAssetValueDictionary, new ValueTypeElementInfo("FontAssetDictionaryValue", nameof(BaseCorePropertyValueDisplay.ValuesFontAssetValueDictionary)) },
            {CorePropertyValueType.String, new ValueTypeElementInfo("StringValuePropertyField", nameof(BaseCorePropertyValueDisplay.ValueString), "StringValuePropertyField") },
            {CorePropertyValueType.StringList, new ValueTypeElementInfo("StringListValue", nameof(BaseCorePropertyValueDisplay.ValueStringList)) },
            {CorePropertyValueType.ClassModifierDictionary, new ValueTypeElementInfo("ModifierDictionaryValue", nameof(BaseCorePropertyValueDisplay.ValuesClassModifierDictionary)) },
            {CorePropertyValueType.PluginDictionary, new ValueTypeElementInfo("PluginDictionaryValue", nameof(BaseCorePropertyValueDisplay.ValuesPluginDictionary))}
        };

        /// <summary>
        /// Handles the display of the value preview in the inspector for the given property
        /// </summary>
        /// <param name="container"></param>
        /// <param name="propertyDisplayName"></param>
        /// <param name="valueType"></param>
        public void DisplayValueInContainerForproperty(VisualElement container, string propertyDisplayName, CorePropertyValueType valueType, SerializedProperty serializedProperty)
        {
            // Hide all items first
            foreach (var item in m_ValueTypeToElementNameDict.Values)
            {
                var elementToHideOrShow = container.Q(item.NameOfElementToHide);
                var elementToBind = container.Q(item.NameOfElementToBind);

                if (elementToHideOrShow == null)
                {
                    Debug.LogError($"Could not find element {item.NameOfElementToHide}");
                }

                // Check if the element supports the binding path
                if (elementToBind is BindableElement bindableElement)
                {
                    bindableElement.bindingPath = item.BindingPath;
                }
                else if (elementToBind is PropertyField propertyField)
                {
                    // This catches the case where the element is a PropertyField or custom PropertyField
                    propertyField.bindingPath = item.BindingPath;
                }
                else
                {
                    Debug.LogWarning($"Could not set binding path for element {item.NameOfElementToHide}");
                }

                if (elementToHideOrShow != null)
                {
                    elementToHideOrShow.style.display = DisplayStyle.None; // Hide the ListView
                }
            }

            // Set the binding path of the list view to the property
            CorePropertyValueType currentValueType = valueType;

            // Try get the field name from the dictionary
            if (m_ValueTypeToElementNameDict.TryGetValue(currentValueType, out ValueTypeElementInfo elementInfo))
            {
                var element = container.Q(elementInfo.NameOfElementToHide);



                string formattedPropertyName = propertyDisplayName;

                // If the element is a ListView, format the property name and set the binding path
                if (element is ListView)
                {
                    var listView = element as ListView;

                    listView.headerTitle = formattedPropertyName;
                }
                else if (element is PropertyField)
                {
                    var propertyField = element as PropertyField;

                    propertyField.label = formattedPropertyName;
                }
                // If it is not a ListView, set the text of the element to the property name
                else
                {
                    var label = element.Q<Label>();
                    if (label != null)
                    {
                        label.text = formattedPropertyName;
                    }
                }



                element.style.display = DisplayStyle.Flex; // Show the ListView
            }
            else
            {
                UnityEngine.Debug.LogError($"Could not find the field name for the value type {currentValueType} and property name {propertyDisplayName}");
            }
        }
    }




    [CustomPropertyDrawer(typeof(BaseCorePropertyValueDisplay))]
    public class CorePropertyValuePropertyDrawer : PropertyDrawer
    {

        private string m_ValueTypeFieldName = nameof(BaseCorePropertyValueDisplay.ValueType);
        public static string m_PropertyNameFieldName = nameof(BaseCorePropertyValueDisplay.PropertyName);

        private string m_TooltipTextFieldName = nameof(BaseCorePropertyValueDisplay.TooltipText);

        private string m_CorePropertyContainerClass = "core_property_container";



        protected virtual VisualElement GetInputElementForProperty(SerializedProperty property)
        {
            return null;
        }



        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/CorePropertyValuePropertyDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            // We add this class so that we can add padding to it through USS to prevent the foldout from being too close to the edge of the inspector
            container.AddToClassList(m_CorePropertyContainerClass);

            SerializedProperty TooltipTextProperty = property.FindPropertyRelative(m_TooltipTextFieldName);

            // Get the tooltip text from the property
            string tooltipText = TooltipTextProperty.stringValue;

            // If the tooltip text is not empty, set the tooltip of the container
            if (!string.IsNullOrEmpty(tooltipText))
            {
                container.tooltip = tooltipText;
            }


            var displayHandler = new ValuePreviewDisplayHandler();
            string propertyName = property.FindPropertyRelative(m_PropertyNameFieldName).stringValue;
            int valueTypeIndex = property.FindPropertyRelative(m_ValueTypeFieldName).intValue;

            string propertyDisplayName = property.FindPropertyRelative(nameof(BaseCorePropertyValueDisplay.DisplayName)).stringValue;


            CorePropertyValueType currentValueType = (CorePropertyValueType)valueTypeIndex;
            displayHandler.DisplayValueInContainerForproperty(container, propertyDisplayName, currentValueType, property);



            // use the following to copy the property name to the clipboard so that users can extend the property
            container.RegisterCallback<ContextClickEvent>(evt =>
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Copy Theme Property Name"), false, () =>
            {
                EditorGUIUtility.systemCopyBuffer = PropertyFormatter.FormatPropertyExtensionName(propertyName, PropertyFormatter.PropertyExtensionContext.Core);
            });
            menu.ShowAsContext();
        });


            // Add a view data key to the container so it retains its state when the inspector is reloaded
            container.viewDataKey = "coreprop-" + property.propertyPath;

            // Get a list of all the ListViews in the container, subscribe to their items added event
            var listViews = container.Query<ListView>().ToList();

            // By default, when a new item is added to a list, the values of the new item are the same as the last item in the list.
            // We want to set the initial values of the new item to something else, so we subscribe to the itemsAdded event and set the values of the new item to something else
            foreach (var listView in listViews)
            {
                listView.itemsAdded += (itemIndices) =>
                {
                    foreach (var itemIndex in itemIndices)
                    {
                        if (itemIndex >= 0)
                        {

                            BaseCorePropertyValueDisplay corePropertyDisplayItem = SerializedPropertyHelper.GetArrayFieldValue<BaseCorePropertyValueDisplay>(property);
                            IPropertyDisplayItemValue newInstanceToUpdate = null;


                            switch (currentValueType)
                            {
                                case CorePropertyValueType.ColorPalette:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesColorPalette[itemIndex];
                                    break;
                                case CorePropertyValueType.ImageValueDictionary:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesImageValueDictionary[itemIndex];
                                    break;
                                case CorePropertyValueType.FontAssetValueDictionary:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesFontAssetValueDictionary[itemIndex];
                                    break;
                                case CorePropertyValueType.KeyValueDictionary:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesKeyValueDictionary[itemIndex];
                                    break;
                                case CorePropertyValueType.StringList:
                                    // We can't do the same thing for string lists because the list is not a list of IPropertyDisplayValue
                                    corePropertyDisplayItem.ValueStringList.Values[itemIndex].Value = "";
                                    break;
                                case CorePropertyValueType.ClassModifierDictionary:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesClassModifierDictionary[itemIndex];
                                    break;
                                case CorePropertyValueType.PluginDictionary:
                                    newInstanceToUpdate = corePropertyDisplayItem.ValuesPluginDictionary[itemIndex];
                                    break;
                                default:
                                    Debug.LogWarning($"Could not set initial values for new item for value type {currentValueType}");
                                    break;
                            }

                            if (newInstanceToUpdate != null)
                            {
                                newInstanceToUpdate.SetInitialValuesForNewItems();
                            }
                        }
                    }
                };
            }


            // Return the finished UI
            return container;
        }
    }



}
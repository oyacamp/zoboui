using System;
using System.Collections.Generic;
using ZoboUI.Core;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace ZoboUI.Editor.PropertyDrawers
{
    /// <summary>
    /// Base class for property drawers that require a unique key. This class handles the validation of the key field and shows an error indicator if the key is empty or if the key already exists.
    /// </summary>
    public abstract class StringAsDropdownDrawer : PropertyDrawer
    {

        protected abstract List<string> GetChoices(ThemeConfigDisplayVersion configDisplayHandler, SerializedProperty property);

        protected ThemeConfigDisplayVersion GetThemeConfigDisplayVersion(SerializedProperty property)
        {
            if (property == null)
            {
                return null;
            }



            try
            {
                // If we try to get the target object of the property (even to check if it's null), it throws an exception:
                //ArgumentNullException: Value cannot be null.

                // So we catch it and return null if it happens
                var targetObj = property.serializedObject.targetObject;

            }
            catch (ArgumentNullException ex)
            {
                return null;
            }

            if (property.serializedObject.targetObject is ThemeConfigManager themeConfigManager)
            {
                ThemeConfigDisplayVersion configDisplayVersion = themeConfigManager.ThemeConfigDisplay;
                return configDisplayVersion;
            }
            else
            {
                return null;
            }
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            // Create a new VisualElement to be the root the property UI
            var container = new VisualElement();

            // Load and clone a visual tree from UXML
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oyacamp.zoboui/Editor/PropertyDrawers/StringAsDropdownDrawer_UXML.uxml");
            visualTree.CloneTree(container);

            // Get the DropdownField from the UXML
            DropdownField dropdownField = container.Q<DropdownField>();
            dropdownField.bindingPath = nameof(ExtendFieldsStringDropdownFieldValue.Value);

            ThemeConfigManager themeConfigManager = property.serializedObject.targetObject as ThemeConfigManager;

            ThemeConfigDisplayVersion configDisplayVersion = themeConfigManager.ThemeConfigDisplay;


            if (configDisplayVersion.RequiredStringDropdownInfoInstance == null)
            {
                throw new Exception("RequiredStringDropdownInfoInstance is null");
            }



            dropdownField.choices = this.GetChoices(configDisplayVersion, property);



            return container;
        }

    }

    [CustomPropertyDrawer(typeof(ExtendFieldsStringDropdownFieldValue))]
    public class ExtendFieldsStringAsDropdownDrawer : StringAsDropdownDrawer
    {
        protected UtilityConfigDisplay GetParentCorePropertyDisplay(SerializedProperty property)
        {
            // Loop through the parent properties until we find the parent property that is a CorePropertyDisplay
            SerializedProperty parentProperty = SerializedPropertyHelper.GetParentProperty(property);
            while (parentProperty != null)
            {
                if (parentProperty.type == typeof(UtilityConfigDisplay).Name)
                {
                    return SerializedPropertyHelper.GetArrayFieldValue<UtilityConfigDisplay>(parentProperty);
                }
                parentProperty = SerializedPropertyHelper.GetParentProperty(parentProperty);
            }
            return null;
        }


        protected override List<string> GetChoices(ThemeConfigDisplayVersion configDisplayHandler, SerializedProperty property)
        {
            UtilityConfigDisplay parentUtilityConfigDisplay = GetParentCorePropertyDisplay(property);
            BaseCorePropertyValueDisplay parentCorePropertyDisplay = parentUtilityConfigDisplay.PropertyDisplay;

            if (parentCorePropertyDisplay == null)
            {
                throw new Exception("Could not find parent core property display");
            }

            var list = configDisplayHandler.RequiredStringDropdownInfoInstance.ValueTypeToFieldExtensionNamesList.Find(item => item.ValueType == parentCorePropertyDisplay.ValueType).ExtensibleFields;

            string currentPropertyFieldName = PropertyFormatter.FormatPropertyExtensionName(parentUtilityConfigDisplay.PropertyName, PropertyFormatter.PropertyExtensionContext.Utilities);

            // Remove the current property field name from the list of choices so that we don't have a dropdown with the same name as the property
            list.Remove(currentPropertyFieldName);

            return list;
        }



    }

    [CustomPropertyDrawer(typeof(ClassModifierStringDropdownFieldValue))]
    public class ClassModifierStringAsDropdownDrawer : StringAsDropdownDrawer
    {



        protected override List<string> GetChoices(ThemeConfigDisplayVersion configDisplayHandler, SerializedProperty property)
        {
            List<string> classModifierNames = new List<string>();

            BaseCorePropertyValueDisplay modifierDisplay = configDisplayHandler.Core.Find(x => x is BaseCorePropertyValueDisplay && x.PropertyName == configDisplayHandler.RequiredStringDropdownInfoInstance.ModifierPropertyDisplayPropertyName);

            BaseCorePropertyValueDisplay classModifierDictionaryCorePropertyDisplay = modifierDisplay;

            // We use the class modifier names from the user's config as the choices
            foreach (var item in classModifierDictionaryCorePropertyDisplay.ValuesClassModifierDictionary)
            {
                if (!string.IsNullOrEmpty(item.ModifierName))
                {
                    classModifierNames.Add(item.ModifierName);
                }
            }
            return classModifierNames;
        }


        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = base.CreatePropertyGUI(property);


            // Get the DropdownField from the UXML
            DropdownField dropdownField = container.Q<DropdownField>();


            ThemeConfigDisplayVersion configDisplayVersion = this.GetThemeConfigDisplayVersion(property);


            // We listen to the OnModifierValuesUpdated event so that we can update the choices for the dropdown when the modifier names are updated

            configDisplayVersion.RequiredStringDropdownInfoInstance.OnModifierValuesUpdated += () =>
            {



                ThemeConfigDisplayVersion configDisplayVersion = this.GetThemeConfigDisplayVersion(property);

                if (configDisplayVersion == null)
                {
                    return;
                }



                var choices = this.GetChoices(configDisplayVersion, property);



                dropdownField.choices = choices;

            };


            return container;
        }

    }
}

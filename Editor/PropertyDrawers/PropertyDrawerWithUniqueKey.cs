
using System;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine.UIElements;


namespace ZoboUI.Editor.PropertyDrawers
{
    /// <summary>
    /// Base class for property drawers that require a unique key. This class handles the validation of the key field and shows an error indicator if the key is empty or if the key already exists.
    /// </summary>
    public class PropertyDrawerWithUniqueKey : PropertyDrawer
    {
        // Declaring this outside of CreatePropertyGUI() causes the error indicator to only show up on the last item when there are multiple items in the list
        //private AlertElement alertElement;

        /// <summary>
        /// We store a dictionary of parent property paths to a dictionary of keys to item indices. This is used to check if a key already exists in the list. Every property using this drawer shares the same dictionary.
        /// </summary>
        protected PropertyPathToKeyToItemIndexDictionary m_PropertyPathToKeyIndexDictionary = new PropertyPathToKeyToItemIndexDictionary();

        protected enum ValidationMode
        {
            EmptyFields,
            ConflictingKeys,
            Both
        }


        public delegate void KeyChangedHandler(string parentPropertyPath, string currentPropertyPath, string newKey, string oldKey);

        public static class KeyChangedEvent
        {
            public static event KeyChangedHandler OnKeyChanged;

            public static void RaiseKeyChanged(string parentPropertyPath, string currentPropertyPath, string newKey, string oldKey)
            {
                OnKeyChanged?.Invoke(parentPropertyPath, currentPropertyPath, newKey, oldKey);
            }
        }

        /// <summary>
        /// Returns the property path of a serialized property. We do this in a try-catch block because sometimes the property path is null when used in lists. So this allows us to return null instead of throwing an exception () so we can handle it gracefully.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected string GetPathFromSerializedProperty(SerializedProperty property)
        {
            try
            {
                return property.propertyPath;
            }
            catch (Exception)
            {
                // The exception is usually "NullReferenceException: SerializedObject of SerializedProperty has been Disposed."
                return null;
            }
        }

        public static void ShowErrorIndicator(AlertElement alert, string message)
        {
            alert.UpdateAlert(AlertElement.AlertType.Error, message);
            alert.style.display = DisplayStyle.Flex;
        }

        public static void HideErrorIndicator(AlertElement alert)
        {
            alert.style.display = DisplayStyle.None;
        }


        /// <summary>
        /// Handles the validation of the key field. This method subscribes to the KeyChangedEvent and updates the dictionary of keys to item indices. It also shows an error indicator if the key is empty or if the key already exists.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="keyField"></param>
        /// <param name="alertElement"></param>
        protected void HandleKeyValidation(SerializedProperty property, TextField keyField, AlertElement alertElement, ValidationMode validationMode = ValidationMode.Both)
        {
            SerializedProperty parentProperty = SerializedPropertyHelper.GetParentProperty(property);

            if (!m_PropertyPathToKeyIndexDictionary.ContainsKey(parentProperty.propertyPath))
            {
                m_PropertyPathToKeyIndexDictionary.Add(parentProperty.propertyPath, new KeyToItemIndexDictionary());

            }
            HideErrorIndicator(alertElement);
            bool hadConflictError = false;

            string parentPropertyPath = parentProperty.propertyPath;
            string currentPropertyPath = property.propertyPath;

            // Subscribe to the event
            KeyChangedEvent.OnKeyChanged += (parentPath, inPropertyPath, newKey, oldKey) =>
            {
                // If the property or the serialized parent property is null, return, otherwise we will get an exception about the property being Disposed
                if (GetPathFromSerializedProperty(property) == null || GetPathFromSerializedProperty(parentProperty) == null)
                {
                    return;
                }

                if (String.Equals(parentPath, parentPropertyPath) && !String.Equals(inPropertyPath, currentPropertyPath))
                {


                    if (keyField.value == oldKey || keyField.value == newKey || hadConflictError)
                    {
                        if (CorePropertyKeyHelper.IsKeyConflict(m_PropertyPathToKeyIndexDictionary, property, keyField.value))
                        {
                            hadConflictError = true;
                            ShowErrorIndicator(alertElement, "Key already exists");

                            return;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(keyField.value))
                            {
                                HideErrorIndicator(alertElement);
                                hadConflictError = false;
                                return;
                            }
                        }
                    }
                }
            };

            // Subscribe to the change event on the text field
            keyField.RegisterValueChangedCallback(evt =>
            {


                // If the value is empty, show an error indicator
                if (string.IsNullOrEmpty(evt.newValue) && (validationMode == ValidationMode.EmptyFields || validationMode == ValidationMode.Both))
                {
                    ShowErrorIndicator(alertElement, "Key cannot be empty");
                    return;
                }
                CorePropertyKeyHelper.AddOrUpdateKey(m_PropertyPathToKeyIndexDictionary, property, evt.newValue, evt.previousValue);

                bool checkForConflict = validationMode == ValidationMode.Both || validationMode == ValidationMode.ConflictingKeys;

                if (checkForConflict && CorePropertyKeyHelper.IsKeyConflict(m_PropertyPathToKeyIndexDictionary, property, evt.newValue))
                {
                    hadConflictError = true;
                    ShowErrorIndicator(alertElement, "Key already exists");
                    KeyChangedEvent.RaiseKeyChanged(parentPropertyPath, currentPropertyPath, evt.newValue, evt.previousValue);

                    return;
                }

                HideErrorIndicator(alertElement);

                if (hadConflictError)
                {
                    KeyChangedEvent.RaiseKeyChanged(parentPropertyPath, currentPropertyPath, evt.newValue, evt.previousValue);

                }

                hadConflictError = false;



            });


        }

    }
}
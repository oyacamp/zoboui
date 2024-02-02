using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ZoboUI.Editor.Utils
{

    /// <summary>
    /// Helper class for retrieving the value of a SerializedProperty using reflection. This is useful for getting the value of a custom property type or dictionary.
    /// </summary>
    public static class SerializedPropertyHelper
    {



        /// <summary>
        /// Gets the value of a field in an array or list element. This is useful for getting the value of a custom property type or dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <returns></returns>
        public static T GetArrayFieldValue<T>(SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            Type targetType = targetObject.GetType();

            string[] pathParts = property.propertyPath.Split('.');
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];

                if (part.Equals("Array") && i < pathParts.Length - 1 && pathParts[i + 1].StartsWith("data["))
                {
                    // Handle array or list elements
                    targetObject = HandleArrayOrListElement(targetObject, pathParts[i + 1]);
                    if (targetObject == null) return default(T);

                    // Update the target type to the type of the element in the array/list
                    targetType = targetObject.GetType();

                    i++; // Skip the next part since it's the "data[i]" part
                }
                else
                {
                    // Handle regular fields
                    FieldInfo field = targetType.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null)
                    {
                        Debug.LogError($"Field not found: {part}");
                        return default(T);
                    }

                    targetObject = field.GetValue(targetObject);
                    if (targetObject == null)
                    {
                        Debug.LogError($"Null value encountered in field: {part}");
                        return default(T);
                    }

                    // Update the target type for the next part
                    targetType = targetObject.GetType();
                }
            }

            return targetObject is T castedValue ? castedValue : default(T);
        }

        public static T GetValueFromSerializedObject<T>(object targetObject, string propertyPath)
        {
            Type targetType = targetObject.GetType();

            string[] pathParts = propertyPath.Split('.');
            for (int i = 0; i < pathParts.Length; i++)
            {
                string part = pathParts[i];

                if (part.Equals("Array") && i < pathParts.Length - 1 && pathParts[i + 1].StartsWith("data["))
                {
                    // Handle array or list elements
                    targetObject = HandleArrayOrListElement(targetObject, pathParts[i + 1]);
                    if (targetObject == null) return default(T);

                    // Update the target type to the type of the element in the array/list
                    targetType = targetObject.GetType();

                    i++; // Skip the next part since it's the "data[i]" part
                }
                else
                {
                    // Handle regular fields
                    FieldInfo field = targetType.GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field == null)
                    {
                        Debug.LogError($"Field not found: {part}");
                        return default(T);
                    }

                    targetObject = field.GetValue(targetObject);
                    if (targetObject == null)
                    {
                        Debug.LogError($"Null value encountered in field: {part}");
                        return default(T);
                    }

                    // Update the target type for the next part
                    targetType = targetObject.GetType();
                }
            }

            return targetObject is T castedValue ? castedValue : default(T);
        }

        private static object HandleArrayOrListElement(object targetObject, string arrayPart)
        {
            int index = ExtractIndex(arrayPart);
            if (targetObject is IList list && index < list.Count)
            {
                return list[index];
            }

            Debug.LogError($"Array index out of range or object is not a list: {arrayPart}");
            return null;
        }

        private static int ExtractIndex(string arrayPart)
        {
            string indexString = arrayPart.Substring(5, arrayPart.Length - 6); // Extracts the numeric part from "data[i]"
            if (int.TryParse(indexString, out int index))
            {
                return index;
            }

            throw new ArgumentException($"Invalid array index encountered: {arrayPart}");
        }

        public static SerializedProperty GetParentProperty(SerializedProperty property)
        {
            string path = property.propertyPath;
            int lastDot = path.LastIndexOf('.');
            if (lastDot == -1)
            {
                return null; // No parent property
            }

            string parentPath = path.Substring(0, lastDot);
            return property.serializedObject.FindProperty(parentPath);
        }





    }
}
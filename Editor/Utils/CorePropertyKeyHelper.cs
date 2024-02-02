using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZoboUI.Editor;
using ZoboUI.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace ZoboUI.Editor.Utils
{


    /// <summary>
    /// Represents a dictionary that maps keys to item indices in a list.
    /// </summary>
    public class KeyToItemIndexDictionary : Dictionary<string, HashSet<int>> { }


    /// <summary>
    /// Represents a dictionary that maps property paths to KeyToItemIndexDictionary.
    /// </summary>
    public class PropertyPathToKeyToItemIndexDictionary : Dictionary<string, KeyToItemIndexDictionary> { }

    public static class CorePropertyKeyHelper
    {
        public static void AddOrUpdateKey(PropertyPathToKeyToItemIndexDictionary propPathToKeyDict, SerializedProperty property, string newKey, string prevKey)
        {
            SerializedProperty parentProperty = SerializedPropertyHelper.GetParentProperty(property);
            if (parentProperty.isArray)
            {
                if (!propPathToKeyDict.ContainsKey(parentProperty.propertyPath))
                {
                    propPathToKeyDict[parentProperty.propertyPath] = new KeyToItemIndexDictionary();
                }

                if (!propPathToKeyDict[parentProperty.propertyPath].ContainsKey(newKey))
                {
                    propPathToKeyDict[parentProperty.propertyPath][newKey] = new HashSet<int>();
                }

                int lastIndex = parentProperty.arraySize - 1;
                SerializedProperty lastItemProperty = parentProperty.GetArrayElementAtIndex(lastIndex);

                if (lastItemProperty != null && String.Equals(lastItemProperty.propertyPath, property.propertyPath))
                {
                    // If the new key is the last item, we can just use the last index
                    if (!propPathToKeyDict[parentProperty.propertyPath][newKey].Contains(lastIndex))
                    {
                        propPathToKeyDict[parentProperty.propertyPath][newKey].Add(lastIndex);

                    }
                    return;
                }

                if (!string.IsNullOrEmpty(prevKey) && propPathToKeyDict[parentProperty.propertyPath].ContainsKey(prevKey))
                {
                    int index = propPathToKeyDict[parentProperty.propertyPath][prevKey].FirstOrDefault();
                    SerializedProperty currentProperty = parentProperty.GetArrayElementAtIndex(index);

                    if (currentProperty != null && String.Equals(currentProperty.propertyPath, property.propertyPath))
                    {
                        if (!propPathToKeyDict[parentProperty.propertyPath][newKey].Contains(index))
                        {
                            propPathToKeyDict[parentProperty.propertyPath][newKey].Add(index);

                        }
                        return;
                    }
                }

                // Loop through the array to find the appropriate index
                int arraySize = parentProperty.arraySize;
                for (int i = 0; i < arraySize; i++)
                {
                    SerializedProperty currentProperty = parentProperty.GetArrayElementAtIndex(i);

                    if (currentProperty != null && String.Equals(currentProperty.propertyPath, property.propertyPath))
                    {
                        if (!propPathToKeyDict[parentProperty.propertyPath][newKey].Contains(i))
                        {
                            propPathToKeyDict[parentProperty.propertyPath][newKey].Add(i);


                        }
                        return;
                    }
                }
            }
        }


        public static bool IsKeyConflict(PropertyPathToKeyToItemIndexDictionary propPathToKeyDict, SerializedProperty property, string key)
        {
            var conflictingKeys = GetConflictingIndicesForKey(propPathToKeyDict, property, key);

            return conflictingKeys.Count > 1;
        }

        public static List<int> GetConflictingIndicesForKey(PropertyPathToKeyToItemIndexDictionary propPathToKeyDict, SerializedProperty property, string key, bool excludeCurrentProperty = false)
        {
            List<int> conflictingIndices = new List<int>();
            SerializedProperty parentProperty = SerializedPropertyHelper.GetParentProperty(property);

            if (parentProperty.isArray)
            {
                if (!propPathToKeyDict.ContainsKey(parentProperty.propertyPath))
                {
                    propPathToKeyDict[parentProperty.propertyPath] = new KeyToItemIndexDictionary();
                    int arraySize = parentProperty.arraySize;
                    for (int i = 0; i < arraySize; i++)
                    {
                        SerializedProperty currentProperty = parentProperty.GetArrayElementAtIndex(i);
                        IWithUniqueKey item = SerializedPropertyHelper.GetArrayFieldValue<IWithUniqueKey>(currentProperty);

                        string itemKey = item != null ? item.GetUniqueKey() : null;

                        if (item != null && !string.IsNullOrEmpty(itemKey))
                        {
                            if (!propPathToKeyDict[parentProperty.propertyPath].ContainsKey(itemKey))
                            {
                                propPathToKeyDict[parentProperty.propertyPath][itemKey] = new HashSet<int>();
                            }
                            propPathToKeyDict[parentProperty.propertyPath][itemKey].Add(i);
                        }
                    }
                }

                if (propPathToKeyDict[parentProperty.propertyPath].TryGetValue(key, out HashSet<int> indices))
                {
                    // Sort the indices in descending order so we can remove them from the list without messing up the indices. 
                    List<int> sortedIndicesToRemove = indices.OrderByDescending(x => x).ToList();


                    for (int i = 0; i < sortedIndicesToRemove.Count; i++)
                    {
                        // Likely means the item was removed from the list
                        if (sortedIndicesToRemove[i] >= parentProperty.arraySize)
                        {
                            continue;
                        }


                        SerializedProperty currentProperty = parentProperty.GetArrayElementAtIndex(sortedIndicesToRemove[i]);
                        if (currentProperty == null)
                        {
                            continue;
                        }

                        if (excludeCurrentProperty && String.Equals(currentProperty.propertyPath, property.propertyPath))
                        {
                            continue;
                        }

                        IWithUniqueKey item = SerializedPropertyHelper.GetArrayFieldValue<IWithUniqueKey>(currentProperty);

                        if (item != null)
                        {
                            if (string.Equals(item.GetUniqueKey(), key))
                            {
                                conflictingIndices.Add(sortedIndicesToRemove[i]);
                            }
                            else
                            {
                                indices.Remove(sortedIndicesToRemove[i]);

                            }
                        }



                    }


                }
            }


            return conflictingIndices;
        }

    }

}
using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEngine;

namespace ZoboUI.Editor
{


    [System.Serializable]
    public class ClassTagPropertyHolder
    {
        public StringListValue properties;
    }

    /// <summary>
    /// Represents a dictionary that maps a class tag to USS property names. E.g "rounded" : ["border-radius"] or "rounded-t" : ["border-top-left-radius", "border-top-right-radius"] , etc. The key is the class tag, and the value is a list of USS property names to generate using the values in the provided value type's dictionary.
    /// </summary>
    [System.Serializable]
    public class ClassTagToUssPropertyDictionary : SerializableDictionary<string, ClassTagPropertyHolder>
    {



        public bool IsSameValueType(IConfigValue other)
        {
            return other is ClassTagToUssPropertyDictionary;
        }
    }


    [System.Serializable]
    public class ClassTagToUssPropertyMapDisplay : IWithUniqueKey
    {
        public string ClassTag;

        public TextFieldDataList UssPropertyNames;

        public string GetUniqueKey()
        {
            return ClassTag;
        }

        public void SetInitialValuesForNewItems()
        {
            ClassTag = "";
            UssPropertyNames = new TextFieldDataList();
        }
    }


    public class ClassTagToUssPropertyMapConverter : IConvertibleToDisplay<List<ClassTagToUssPropertyMapDisplay>, ClassTagToUssPropertyDictionary>
    {
        public List<ClassTagToUssPropertyMapDisplay> ConvertToDisplay(ClassTagToUssPropertyDictionary model)
        {
            var displayList = new List<ClassTagToUssPropertyMapDisplay>();

            StringListConverter stringListConverter = new StringListConverter();

            foreach (var entry in model)
            {
                var displayItem = new ClassTagToUssPropertyMapDisplay { ClassTag = entry.Key, UssPropertyNames = stringListConverter.ConvertToDisplay(entry.Value.properties) };
                displayList.Add(displayItem);
            }

            return displayList;
        }

        public ClassTagToUssPropertyDictionary ConvertFromDisplay(List<ClassTagToUssPropertyMapDisplay> display)
        {
            var model = new ClassTagToUssPropertyDictionary();
            StringListConverter stringListConverter = new StringListConverter();

            foreach (var displayItem in display)
            {
                var properties = stringListConverter.ConvertFromDisplay(displayItem.UssPropertyNames);

                model[displayItem.ClassTag] = new ClassTagPropertyHolder { properties = properties };
            }

            return model;
        }
    }

}
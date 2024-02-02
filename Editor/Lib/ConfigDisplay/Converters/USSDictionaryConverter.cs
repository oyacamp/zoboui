using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using UnityEngine;


namespace ZoboUI.Editor
{

    [System.Serializable]
    public class UssPropertyHolderDisplay : IWithUniqueKey
    {
        public string PropertyName;
        public string PropertyValue;

        public string GetUniqueKey()
        {
            return PropertyName;
        }
    }

    /// <summary>
    /// Represents a dictionary that maps a USS property to the value of the USS property. e.g 'border-radius' : '4px', 'background-color' : '#FFF', etc.
    /// </summary>
    public class USSPropertyToValueDictionary : Dictionary<string, string> { }
    public class USSDictionaryConverter : IConvertibleToDisplay<List<UssPropertyHolderDisplay>, USSPropertyToValueDictionary>
    {
        public List<UssPropertyHolderDisplay> ConvertToDisplay(USSPropertyToValueDictionary model)
        {
            List<UssPropertyHolderDisplay> displayList = new List<UssPropertyHolderDisplay>();

            if (model == null)
                return displayList;

            foreach (var entry in model)
            {
                displayList.Add(new UssPropertyHolderDisplay { PropertyName = entry.Key, PropertyValue = entry.Value });
            }

            return displayList;
        }

        public USSPropertyToValueDictionary ConvertFromDisplay(List<UssPropertyHolderDisplay> display)
        {
            var model = new USSPropertyToValueDictionary();

            if (display == null)
                return model;

            foreach (var displayItem in display)
            {
                model[displayItem.PropertyName] = displayItem.PropertyValue;
            }

            return model;
        }
    }
}
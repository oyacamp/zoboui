using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZoboUI.Editor
{
    /// <summary>
    /// A custom class that holds the default ClassModifierDictionary values
    /// </summary>
    public class DefaultClassModifierDictionary : ClassModifierDictionary
    {
        public DefaultClassModifierDictionary()
        {
            this.AddKeyValue("hover", $"{GENERATED_CLASS_TEMPLATE_STRING}:hover");

            this.AddKeyValue("focus", $"{GENERATED_CLASS_TEMPLATE_STRING}:focus");
            this.AddKeyValue("checked", $"{GENERATED_CLASS_TEMPLATE_STRING}:checked");
            this.AddKeyValue("disabled", $"{GENERATED_CLASS_TEMPLATE_STRING}:disabled");
            this.AddKeyValue("enabled", $"{GENERATED_CLASS_TEMPLATE_STRING}:enabled");
            this.AddKeyValue("active", $"{GENERATED_CLASS_TEMPLATE_STRING}:active");
            this.AddKeyValue("inactive", $"{GENERATED_CLASS_TEMPLATE_STRING}:inactive");
            //this.AddKeyValue("dark", $".dark-theme {GENERATED_CLASS_TEMPLATE_STRING}");
        }

        private void AddKeyValue(string key, string value)
        {
            this.Add(key, new Core.ModifierValueHolder() { value = value });
        }
    }
}
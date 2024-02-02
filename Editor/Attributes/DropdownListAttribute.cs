using UnityEngine;

namespace ZoboUI.Editor.Attributes
{

    public class DropdownListAttribute : PropertyAttribute
    {
        public string[] choices;

        public DropdownListAttribute(params string[] choices)
        {
            this.choices = choices;
        }
    }
}
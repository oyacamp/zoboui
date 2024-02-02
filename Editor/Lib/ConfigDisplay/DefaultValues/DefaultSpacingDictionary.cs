using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZoboUI.Editor
{
    /// <summary>
    /// A custom class that holds the default spacing values. Can also be used to generative negative spacing values.
    /// </summary>
    public class DefaultSpacingDictionary : KeyValueDictionary
    {
        private bool isNegative = false;

        private const string NEGATIVE_KEY_PREFIX = "minus-";

        private const string NEGATIVE_VALUE_PREFIX = "-";

        public DefaultSpacingDictionary(bool isNegative = false)
        {
            this.isNegative = isNegative;
            AddSpacingValue("0", "0px");
            AddSpacingValue("1", "4px");
            AddSpacingValue("2", "8px");
            AddSpacingValue("3", "12px");
            AddSpacingValue("4", "16px");
            AddSpacingValue("5", "20px");
            AddSpacingValue("6", "24px");
            AddSpacingValue("7", "28px");
            AddSpacingValue("8", "32px");
            AddSpacingValue("9", "36px");
            AddSpacingValue("10", "40px");
            AddSpacingValue("11", "44px");
            AddSpacingValue("12", "48px");
            AddSpacingValue("14", "56px");
            AddSpacingValue("16", "64px");
            AddSpacingValue("20", "80px");
            AddSpacingValue("24", "96px");
            AddSpacingValue("28", "112px");
            AddSpacingValue("32", "128px");
            AddSpacingValue("36", "144px");
            AddSpacingValue("40", "160px");
            AddSpacingValue("44", "176px");
            AddSpacingValue("48", "192px");
            AddSpacingValue("52", "208px");
            AddSpacingValue("56", "224px");
            AddSpacingValue("60", "240px");
            AddSpacingValue("64", "256px");
            AddSpacingValue("72", "288px");
            AddSpacingValue("80", "320px");
            AddSpacingValue("96", "384px");
            AddSpacingValue("px", "1px");
            AddSpacingValue("0point5", "2px");
            AddSpacingValue("1point5", "6px");
            AddSpacingValue("2point5", "10px");
            AddSpacingValue("3point5", "14px");
        }

        private void AddSpacingValue(string key, string value)
        {
            if (isNegative)
            {
                key = NEGATIVE_KEY_PREFIX + key;

                // We don't want to add the - to 0
                if (value != "0px" && value != "0")
                {
                    value = NEGATIVE_VALUE_PREFIX + value;

                }

            }
            this.Add(key, new Core.StringKeyValueHolder() { value = value });
        }
    }
}
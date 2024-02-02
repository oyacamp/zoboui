using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor.Attributes
{
    /* We don't do this because we want to be able to serialize the validator and because unity doesn't support interfaces or polymorphized classes in the inspector.
    public interface ITextFieldValidator
    {

        string GetErrorMessage();

        bool IsValid(string value);
    }*/

    /// <summary>
    /// We use this as a class so that it can be serialized and the value is not lost when the script is recompiled.
    /// </summary>
    [System.Serializable]
    public class BaseTextFieldValidator
    {
        public class ParseResult
        {
            public bool IsValid;
            public string ErrorMessage;
        }

        /// <summary>
        /// Whether or not the string value can be empty.
        /// </summary>
        public bool IsRequired = false;



        public ParseResult Validate(string value)
        {
            ParseResult result = new ParseResult();

            if (IsRequired && string.IsNullOrEmpty(value))
            {
                result.IsValid = false;
                result.ErrorMessage = "This field is required.";
                return result;
            }


            result.IsValid = true;


            return result;
        }
    }


    /// <summary>
    /// A custom attribute that signifies that how a text value field should be displayed in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class TextFieldAttribute : PropertyAttribute, IAttributeWithBaseCorePropertyConverter
    {
        public bool IsMultiline;

        public BaseTextFieldValidator Validator;

        public TextFieldAttribute(bool isMultiline, bool isRequired = false, string defaultValueIfEmpty = null)
        {
            this.IsMultiline = isMultiline;

            Validator = new BaseTextFieldValidator()
            {
                IsRequired = isRequired,
            };
        }

        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            TextFieldStringDataConverter stringDataConverter = new TextFieldStringDataConverter(logger);
            var value = stringDataConverter.ConvertFromDisplay(display.ValueString);

            return value;
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, object propertyValue, string tooltipText = null, ICustomLogger logger = null)
        {
            // Check if the property value is a string
            if (propertyValue is string)
            {
                var stringDataConverter = new TextFieldStringDataConverter(logger);

                var stringValueDisplay = stringDataConverter.ConvertToDisplay((string)propertyValue);

                var display = new BaseCorePropertyValueDisplay
                {
                    PropertyName = propertyName,
                    TooltipText = tooltipText,
                    ValueString = stringValueDisplay,
                    ValueType = CorePropertyValueType.String

                };

                display.ValueString.isMultiline = IsMultiline;
                display.ValueString.Validator = Validator;

                return display;

            }
            else
            {
                throw new System.Exception($"The property value is not a string for the property {propertyName}");
            }

        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.String;
        }
    }
}
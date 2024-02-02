using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using ZoboUI.Editor.Attributes;
using UnityEngine;
using ZoboUI.Core.Utils;


namespace ZoboUI.Editor
{


    /// <summary>
    /// A custom class that holds a string value and signifies that the value should be displayed as a text field in the inspector.
    /// </summary>
    [System.Serializable]
    public class TextFieldStringData : IPropertyDisplayItemValue
    {

        /// <summary>
        /// The string value 
        /// </summary>
        public string Value;


        public bool isMultiline = false;

        /// <summary>
        /// The validator to use for this text field. If null, no validation will be done.
        /// </summary>
        public BaseTextFieldValidator Validator;


        public void SetInitialValuesForNewItems()
        {
            Value = "";
            isMultiline = false;
        }
    }


    public class TextFieldStringDataConverter : IConvertibleToDisplay<TextFieldStringData, string>
    {
        private ICustomLogger customLogger;

        public TextFieldStringDataConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }
        public string ConvertFromDisplay(TextFieldStringData model)
        {
            return model.Value;
        }

        public TextFieldStringData ConvertToDisplay(string display)
        {
            return new TextFieldStringData { Value = display };
        }
    }
}
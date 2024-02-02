using System.Collections;
using System.Collections.Generic;
using ZoboUI.Core;
using ZoboUI.Editor.Attributes;
using UnityEngine;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor
{


    [System.Serializable]
    public class StringListValue : List<string>, IConvertibleToBaseCorePropertyValueDisplay, IConvertibleFromBaseCorePropertyValueDisplay
    {
        /// <summary>
        /// The validator to use for this text field. If null, no validation will be done.
        /// </summary>
        public BaseTextFieldValidator Validator;

        public object ConvertFromBaseCorePropertyValueDisplayToThemeConfigValue(BaseCorePropertyValueDisplay display, ICustomLogger logger = null)
        {
            StringListConverter converter = new StringListConverter(logger);

            return converter.ConvertFromDisplay(display.ValueStringList);
        }

        public BaseCorePropertyValueDisplay ConvertToBaseCorePropertyValueDisplay(string propertyName, string tooltipText = null, ICustomLogger logger = null)
        {
            StringListConverter converter = new StringListConverter(logger);

            var display = new BaseCorePropertyValueDisplay()
            {
                PropertyName = propertyName,
                TooltipText = tooltipText
            };

            display.ValueType = CorePropertyValueType.StringList;

            display.ValueStringList = converter.ConvertToDisplay(this);

            return display;
        }

        public CorePropertyValueType GetBaseCorePropertyTypeValue()
        {
            return CorePropertyValueType.StringList;
        }
    }

    [System.Serializable]
    public class TextFieldDataList
    {
        public BaseTextFieldValidator Validator;

        public List<TextFieldStringData> Values = new List<TextFieldStringData>();

    }



    public class StringListConverter : IConvertibleToDisplay<TextFieldDataList, StringListValue>
    {

        private ICustomLogger customLogger;

        public StringListConverter(ICustomLogger logger = null)
        {
            this.customLogger = logger;

            if (logger == null)
            {
                this.customLogger = new CustomLogger();
            }
        }

        public StringListValue ConvertFromDisplay(TextFieldDataList display)
        {
            var stringListValue = new StringListValue();


            foreach (var item in display.Values)
            {
                stringListValue.Add(item.Value);
            }

            if (display.Validator != null)
            {
                stringListValue.Validator = display.Validator;
            }

            return stringListValue;
        }

        public TextFieldDataList ConvertToDisplay(StringListValue model)
        {
            var textFieldDataList = new TextFieldDataList();
            TextFieldStringDataConverter stringDataConverter = new TextFieldStringDataConverter();

            if (model.Validator != null)
            {
                textFieldDataList.Validator = model.Validator;
            }

            foreach (var textString in model)
            {
                TextFieldStringData convertedStringData = stringDataConverter.ConvertToDisplay(textString);

                if (textFieldDataList.Values == null)
                {
                    textFieldDataList.Values = new List<TextFieldStringData>();
                }

                textFieldDataList.Values.Add(convertedStringData);
            }

            return textFieldDataList;

        }
    }

}
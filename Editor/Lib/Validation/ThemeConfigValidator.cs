using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ZoboUI.Core;
using ZoboUI.Core.Utils;

namespace ZoboUI.Editor.Utils
{
    /// <summary>
    /// Interface for classes that validate a config value
    /// </summary>
    public interface IConfigValueValidator
    {
        string GetErrorMessage();
        bool ValueIsValid(Type type, object value, ICustomLogger logger = null);
    }

    /// <summary>
    /// Checks if the field/property values in the ThemeConfig are valid for USS Generation
    /// </summary>
    public class ThemeConfigValidator
    {
        ICustomLogger logger;

        public ThemeConfigValidator(ICustomLogger logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Validates the values in the ThemeConfig. Will throw an exception if any value is invalid
        /// </summary>
        /// <param name="config"></param>
        public void ValidateThemeConfig(ThemeConfig config)
        {
            ValidateFields(config.core);
            ValidateFields(config.utilities);
            ValidateFields(config.compilation);
        }

        private void ValidateFields(object configSection)
        {
            FieldInfo[] fields = configSection.GetType().GetFields();

            foreach (FieldInfo field in fields)
            {
                ValidateField(field, configSection);
            }
        }
        // Made public for testing
        public void ValidateField(FieldInfo field, object objectThatOwnsField)
        {
            List<IConfigValueValidator> attributesValidators = GetValidatorsFromField(field);

            foreach (IConfigValueValidator attribute in attributesValidators)
            {
                IConfigValueValidator validator = attribute as IConfigValueValidator;

                if (!validator.ValueIsValid(field.FieldType, field.GetValue(objectThatOwnsField)))
                {
                    throw new System.Exception(logger.FormatMessage($"Invalid value for {field.Name}: {validator.GetErrorMessage()}"));
                }
            }
        }

        public List<IConfigValueValidator> GetValidatorsFromField(FieldInfo field)
        {
            object[] attributes = field.GetCustomAttributes(true);

            List<IConfigValueValidator> validators = new List<IConfigValueValidator>();

            foreach (object attribute in attributes)
            {
                if (attribute is IConfigValueValidator)
                {
                    validators.Add(attribute as IConfigValueValidator);
                }
            }

            return validators;
        }
    }
}
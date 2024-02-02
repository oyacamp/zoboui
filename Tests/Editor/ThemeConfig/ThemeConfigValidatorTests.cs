using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZoboUI.Core;
using ZoboUI.Core.Utils;
using ZoboUI.Editor.Attributes;
using ZoboUI.Editor.Utils;


namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class ThemeConfigValidatorTests
    {
        private ThemeConfigValidator validator;
        private ICustomLogger logger;

        [SetUp]
        public void SetUp()
        {
            // Initialize logger mock
            logger = new TempLogger();

            // Initialize the object under test
            validator = new ThemeConfigValidator(logger);
        }

        [Test]
        public void ValidateField_WithValidAttribute_DoesNotThrowException()
        {
            ThemeConfig config = new ThemeConfig();

            config.core.modifierSeparator = "--";
            var field = config.core.GetType().GetField(nameof(BaseCoreProperties.modifierSeparator));

            var validatorAttributes = validator.GetValidatorsFromField(field);

            Assert.GreaterOrEqual(validatorAttributes.Count, 1);

            // Assert that one of the attributes is the correct type RequiredStringValueValidatorAttribute
            Assert.IsTrue(validatorAttributes.Exists(x => x.GetType() == typeof(RequiredStringValueValidatorAttribute)));
            Assert.DoesNotThrow(() => validator.ValidateField(field, config.core));

            Assert.DoesNotThrow(() => validator.ValidateThemeConfig(config));
        }

        [Test]
        public void ValidateField_WithInvalidAttribute_ThrowsException()
        {
            ThemeConfig config = new ThemeConfig();

            config.core.modifierSeparator = null;
            var field = config.core.GetType().GetField(nameof(BaseCoreProperties.modifierSeparator));

            var validatorAttributes = validator.GetValidatorsFromField(field);

            Assert.GreaterOrEqual(validatorAttributes.Count, 1);

            // Assert that one of the attributes is the correct type RequiredStringValueValidatorAttribute
            Assert.IsTrue(validatorAttributes.Exists(x => x.GetType() == typeof(RequiredStringValueValidatorAttribute)));

            Assert.Throws<Exception>(() => validator.ValidateField(field, config.core));

            Assert.Throws<Exception>(() => validator.ValidateThemeConfig(config));
        }
    }
}
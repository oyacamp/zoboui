using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using ZoboUI.Core;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

namespace ZoboUI.Editor.Tests
{
    [TestFixture]
    public class ThemeConfigTests
    {
        public static string testJsonFilePath = "Packages/com.oyacamp.zoboui/Tests/Editor/ThemeConfig/TestData/test-theme-config.json";

        public static bool AreObjectsEqual(object a, object b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a == null || b == null)
                return false;

            if (a.GetType() != b.GetType())
                return false;

            if (a is IComparable || a is string || a.GetType().IsPrimitive)
                return a.Equals(b);

            foreach (var property in a.GetType().GetFields())
            {
                var valueA = property.GetValue(a);
                var valueB = property.GetValue(b);

                if (!AreObjectsEqual(valueA, valueB))
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Compares two ThemeConfig objects for equality.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool AreThemeConfigsEqual(ThemeConfig a, ThemeConfig b)
        {
            return AreObjectsEqual(a, b);

        }

        [Test]
        public void AreThemeConfigsEqual_ReturnsTrueForEqualObjects()
        {
            var testConfig = ThemeConfig.FromJson(File.ReadAllText(testJsonFilePath));

            var testConfig2 = ThemeConfig.FromJson(File.ReadAllText(testJsonFilePath));

            Assert.IsTrue(AreThemeConfigsEqual(testConfig, testConfig2));
        }

        [Test]
        public void AreThemeConfigsEqual_ReturnsFalseForUnequalObjects()
        {
            var testConfig = ThemeConfig.FromJson(File.ReadAllText(testJsonFilePath));

            var testConfig2 = ThemeConfig.FromJson(File.ReadAllText(testJsonFilePath));

            testConfig2.core.prefix = "test";

            Assert.IsFalse(AreThemeConfigsEqual(testConfig, testConfig2));
        }


        [Test]
        public void ThemeConfig_SerializesAndDeserializesCorrectly()
        {

            string loadedText = File.ReadAllText(testJsonFilePath);
            var testConfig = ThemeConfig.FromJson(loadedText);

            var serializedJson = ThemeConfig.ToJson(testConfig);

            // We want to make sure the jsonification doesn't create duplicate entries in the extendFields list or other lists
            Assert.IsTrue(testConfig.utilities.backgroundColor.extendFields.Distinct().Count() == testConfig.utilities.backgroundColor.extendFields.Count);
            Assert.IsTrue(testConfig.utilities.backgroundColor.modifierVariations.Distinct().Count() == testConfig.utilities.backgroundColor.modifierVariations.Count);


            Assert.IsNotNull(loadedText);
            Assert.IsNotEmpty(loadedText);

            Assert.IsNotNull(serializedJson);
            Assert.IsNotEmpty(serializedJson);



            var deserializedSerialized = ThemeConfig.FromJson(serializedJson);

            // Compare the deserialized objects
            Assert.IsTrue(AreThemeConfigsEqual(testConfig, deserializedSerialized));




            Assert.IsNotNull(testConfig.core);
            Assert.IsNotNull(testConfig.utilities);
            Assert.IsNotNull(testConfig.compilation);


            FieldInfo[] fields = typeof(UtilityProperties).GetFields();

            Assert.Greater(fields.Length, 0);

            // Check that none of the utillity configs in testConfig.utilities are null using reflection
            // If a value is null, it means that the config was not serialized correctly or one of the config fields is null

            foreach (var property in fields)
            {
                var value = property.GetValue(testConfig.utilities);

                if (value is BaseUtilityConfig)
                {
                    Assert.IsNotNull(value, $"The utility config {property.Name} is null");
                }
            }


        }
    }
}
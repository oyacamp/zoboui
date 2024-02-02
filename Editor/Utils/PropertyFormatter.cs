using System.Linq;
using System.Runtime.Remoting.Contexts;
using UnityEditor.Search;
using ZoboUI.Core;

namespace ZoboUI.Editor.Utils
{
    public class PropertyFormatter
    {
        /// <summary>
        /// Formats a pascalCase or CamelCase property name to be more human readable
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string FormatPropertyNameForDisplay(string propertyName)
        {
            string[] values = SearchUtils.SplitCamelCase(propertyName);
            // We need to capitalize the first letter of the first word
            values[0] = values[0].First().ToString().ToUpper() + values[0].Substring(1);
            return System.String.Join(" ", values);
        }

        public enum PropertyExtensionContext
        {
            Core = 0,
            Utilities = 1,

            Compilation = 2,
        }

        /// <summary>
        /// Formats a property name to be shown in the dropdown list of properties to extend in the theme config
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string FormatPropertyExtensionName(string propertyName, PropertyExtensionContext context)
        {
            switch (context)
            {
                case PropertyExtensionContext.Core:
                    return $"{nameof(ThemeConfig.core)}.{propertyName}";
                case PropertyExtensionContext.Utilities:
                    return $"{nameof(ThemeConfig.utilities)}.{propertyName}";
                default:
                    return propertyName;
            }


        }
    }

}


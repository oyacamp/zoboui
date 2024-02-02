using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ZoboUI.Editor
{

    /// <summary>
    /// The default class extractor used by the purger. 
    /// </summary>
    public class DefaultClassExtractor : IClassExtractor
    {
        public class Config
        {
            /// <summary>
            /// The separator used to separate modifiers from the class name
            /// </summary>
            public string ModifierSeparator { get; set; }

            /// <summary>
            /// The custom prefix used to identify utility classes, if any
            /// </summary>
            public string CustomPrefix { get; set; }

            /// <summary>
            /// The list of class tags to filter by when extracting classes. If provided, only classes containing any of the provided tags will be extracted.
            /// </summary>
            public IEnumerable<string> FilterByKnownClassTags { get; set; }

            /// <summary>
            /// The list of keys to filter by when extracting classes. If provided, only classes containing any of the provided keys (e.g the sm in text-sm , or the blue-500 in bg-blue-500) will be extracted.
            /// </summary>
            public IEnumerable<string> FilterByKnownKeys { get; set; }

            /// <summary>
            /// The list of modifiers to filter by when extracting classes. If provided, only classes containing any of the provided modifiers (e.g the hover in hover_bg-blue-500) will be extracted.
            /// </summary>
            public IEnumerable<string> FilterByKnownModifiers { get; set; }


            // Add constructor to initialize properties
            public Config(string modifierSeparator = "_", string customPrefix = "", IEnumerable<string> filterByKnownClassTags = null, IEnumerable<string> filterByKnownKeys = null, IEnumerable<string> filterByKnownModifiers = null)
            {
                ModifierSeparator = modifierSeparator;
                CustomPrefix = customPrefix;
                FilterByKnownClassTags = filterByKnownClassTags;
                FilterByKnownKeys = filterByKnownKeys;
                FilterByKnownModifiers = filterByKnownModifiers;
            }
        }


        /// <summary>
        /// The separator used to separate modifiers from the class name
        /// </summary>
        string modifierSeparator;

        /// <summary>
        /// The custom prefix used to identify utility classes, if any
        /// </summary>
        string prefix;

        private readonly HashSet<string> knownClassTags;
        private readonly HashSet<string> knownKeys;

        private readonly HashSet<string> knownModifiers;

        private List<Regex> patterns;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modifierSeparator"></param>
        /// <param name="customPrefix"></param>
        /// <param name="filterByKnownClassTags"></param>
        /// <param name="filterByKnownKeys"></param>
        public DefaultClassExtractor(Config config)
        {
            modifierSeparator = config.ModifierSeparator;
            prefix = config.CustomPrefix;

            knownClassTags = config.FilterByKnownClassTags != null ? new HashSet<string>(config.FilterByKnownClassTags) : new HashSet<string>();
            knownKeys = config.FilterByKnownKeys != null ? new HashSet<string>(config.FilterByKnownKeys) : new HashSet<string>();
            knownModifiers = config.FilterByKnownModifiers != null ? new HashSet<string>(config.FilterByKnownModifiers) : new HashSet<string>();



            BuildRegex();
        }
        public IEnumerable<string> ExtractClasses(string content)
        {
            var results = new HashSet<string>();
            foreach (var pattern in patterns)
            {
                foreach (Match match in pattern.Matches(content))
                {
                    var result = GetBalancedClassString(match.Value);
                    if (IsClassValid(result))
                    {
                        results.Add(result);
                    }
                }
            }

            return results;
        }

        private bool IsClassValid(string className)
        {
            if (knownClassTags.Count == 0 && knownKeys.Count == 0 && knownModifiers.Count == 0)
            {
                return true;
            }

            if (knownClassTags.Count > 0)
            {
                // Example: Check if class starts with any known class tag
                bool hasClassTag = knownClassTags.Any(classTag => className.Contains(classTag));

                if (hasClassTag)
                {
                    return true;
                }
            }

            if (knownKeys.Count > 0)
            {
                // Example: Check if class starts with any known key
                bool hasKey = knownKeys.Any(key => className.Contains(key));

                if (hasKey)
                {
                    return true;
                }
            }

            if (knownModifiers.Count > 0)
            {
                // Example: Check if class starts with any known modifier
                bool hasModifier = knownModifiers.Any(modifier => className.Contains(modifier));

                if (hasModifier)
                {
                    return true;
                }
            }

            return false;


        }

        private void BuildRegex()
        {

            string prefixPattern = !string.IsNullOrEmpty(prefix) ? $"-?{Regex.Escape(prefix)}" : "";

            patterns = new List<Regex>
        {
            // Arbitrary properties without square brackets
            new Regex(@"\[[^\s:'""`]+:[^\s\[\]]+\]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Arbitrary properties with balanced square brackets
            new Regex(@"\[[^\s:'""`]]+:[^\s]+?\[[^\s]+\][^\s]+?\]", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Utilities with optional arbitrary values and variants
            new Regex($@"{prefixPattern}(\w+)(?:-(\w+))*((?:\[[^\]]+\])*)({modifierSeparator}(?:\w+))?", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Inner matches
            new Regex(@"[^<>" + "\"" + "'`\\s.(){}\\[\\]#=%$]*[^<>" + "\"" + "'`\\s.(){}\\[\\]#=%:$]", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };
        }

        /// <summary>
        /// This clips or truncates a string at the point where it becomes unbalanced. This is particularly useful when processing class names or selectors that may contain nested structures, such as e.g., w-[calc(100%-1rem)]).The function currently handles only square brackets ([...]), and it stops processing the string as soon as the brackets become unbalanced (e.g., more closing brackets than opening brackets).
        /// </summary>
        /// <param name="input">The input string to be checked for balanced brackets.</param>
        /// <returns>A substring of the input up to the point of the first bracket imbalance, or the entire string if brackets are balanced.</returns>
        private string GetBalancedClassString(string input)
        {
            // Initialize depth counter for bracket balancing
            int depth = 0;

            // Iterate through each character in the input string
            for (int i = 0; i < input.Length; i++)
            {
                // Increase depth for each opening bracket
                if (input[i] == '[') depth++;
                // Decrease depth for each closing bracket
                else if (input[i] == ']') depth--;

                // If depth becomes negative, it indicates more closing brackets than opening brackets
                // Therefore, clip the string at this point and return
                if (depth < 0)
                {
                    return input.Substring(0, i);
                }
            }

            // If no unbalanced brackets are found, return the original string
            return input;
        }
    }
}
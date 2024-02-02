using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ZoboUI.Core.Utils;
using UnityEditor;
using UnityEngine;

namespace ZoboUI.Editor
{
    public interface IClassExtractor
    {
        IEnumerable<string> ExtractClasses(string content);
    }







    /// <summary>
    /// Class responsible for purging unused uss classes
    /// </summary>
    public class UssPurger
    {

        private IEnumerable<string> ContentPatterns { get; set; }
        private IEnumerable<string> SafelistStrings { get; set; }
        private List<Regex> SafelistRegexes { get; set; }
        private IEnumerable<string> BlocklistStrings { get; set; }
        private List<Regex> BlocklistRegexes { get; set; }

        public IEnumerable<IClassExtractor> ClassExtractors { get; set; }

        private HashSet<string> ExcludedDirectories { get; set; }

        ICustomLogger logger;


        public UssPurger(IEnumerable<IClassExtractor> customClassExtractors, IEnumerable<string> contentPatterns = null, IEnumerable<string> safelist = null, IEnumerable<string> blocklist = null, ICustomLogger customLogger = null)
        {
            ContentPatterns = contentPatterns;
            (SafelistStrings, SafelistRegexes) = ProcessStringListValue(safelist);
            (BlocklistStrings, BlocklistRegexes) = ProcessStringListValue(blocklist);
            ClassExtractors = customClassExtractors;
            logger = customLogger ?? new CustomLogger(prefix: "UssPurger", logLevel: LogLevel.Warning);
            ExcludedDirectories = new HashSet<string>();

            if (ClassExtractors == null)
            {
                ClassExtractors = new List<IClassExtractor>();
            }
        }

        private (IEnumerable<string>, List<Regex>) ProcessStringListValue(IEnumerable<string> stringList)
        {
            var strings = new List<string>();
            var regexes = new List<Regex>();

            if (stringList == null)
            {
                return (strings, regexes);
            }

            foreach (var item in stringList)
            {
                if (IsRegexPattern(item))
                {
                    try
                    {
                        regexes.Add(new Regex(item, RegexOptions.Compiled | RegexOptions.IgnoreCase));
                    }
                    catch (ArgumentException)
                    {
                        logger.LogError($"Invalid regex pattern - {item}");
                    }
                }
                else
                {
                    strings.Add(item);
                }
            }
            return (strings, regexes);
        }

        private bool IsRegexPattern(string item)
        {
            // For simplicity, we consider strings starting and ending with '/' as regex
            return item.StartsWith("/") && item.EndsWith("/");
        }

        /// <summary>
        /// Retrieves all files matching the content patterns in the given root path
        /// </summary>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public IEnumerable<string> GetMatchingFiles(string rootPath)
        {
            // Validate root path
            if (!Directory.Exists(rootPath))
            {
                throw new DirectoryNotFoundException(logger.FormatMessage($"Root path does not exist - {rootPath}"));
            }


            foreach (var pattern in ContentPatterns)
            {
                var files = Directory.EnumerateFiles(rootPath, pattern, SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (!IsInExcludedDirectory(file, ExcludedDirectories))
                    {
                        yield return file;
                    }
                }
            }
        }

        private bool IsInExcludedDirectory(string filePath, HashSet<string> excludedDirectories)
        {
            if (excludedDirectories.Count == 0 || string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            var directory = new DirectoryInfo(filePath).Parent;
            while (directory != null)
            {
                if (excludedDirectories.Contains(directory.Name))
                {
                    return true;
                }
                directory = directory.Parent;
            }
            return false;
        }


        /// <summary>
        /// Retrieves all strings that look like classes in the given root path. This looks for classes in all files matching the content patterns in the given root path
        /// </summary>
        /// <param name="rootDirectoryPath"></param>
        /// <param name="showProgressBar"></param>
        /// <returns></returns>
        public HashSet<string> GetUsedClassLikeStrings(string rootDirectoryPath, bool showProgressBar = false)
        {
            var usedClasses = new HashSet<string>();
            var allExtractedClasses = new HashSet<string>();
            var matchingFiles = GetMatchingFiles(rootDirectoryPath).ToList();

            for (int i = 0; i < matchingFiles.Count; i++)
            {
                var file = matchingFiles[i];

                // Update the progress bar if showProgressBar is true
                if (showProgressBar)
                {
                    float progress = (float)i / matchingFiles.Count;
                    string formattedMessage = logger.FormatMessage($"Processing file {i + 1} of {matchingFiles.Count}: {Path.GetFileName(file)}");
                    bool cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Processing Files",
                        formattedMessage,
                        progress);

                    // Check if the user clicked 'Cancel'
                    if (cancel)
                    {
                        EditorUtility.ClearProgressBar();
                        return usedClasses;
                    }
                }

                using (var reader = new StreamReader(file))
                {
                    string fileContent = reader.ReadToEnd();
                    var classesInFile = ExtractClasses(fileContent, ClassExtractors.ToList());
                    foreach (var className in classesInFile)
                    {
                        allExtractedClasses.Add(className);
                        if (IsClassAllowed(className))
                        {
                            usedClasses.Add(className);
                        }
                    }
                }
            }

            // Include classes matching safelist regexes
            foreach (var regex in SafelistRegexes)
            {
                foreach (var className in allExtractedClasses)
                {
                    if (regex.IsMatch(className))
                    {
                        usedClasses.Add(className);
                    }
                }
            }

            // Include safelist strings
            foreach (var className in SafelistStrings)
            {
                usedClasses.Add(className);
            }

            // Clear the progress bar when done, only if it was shown
            if (showProgressBar)
            {
                EditorUtility.ClearProgressBar();
            }

            return usedClasses;
        }


        public async Task<HashSet<string>> GetUsedClassLikeStringsAsync(string rootDirectoryPath)
        {
            var usedClasses = new HashSet<string>();
            var allExtractedClasses = new ConcurrentBag<string>();
            var matchingFiles = GetMatchingFiles(rootDirectoryPath).ToList();

            await Task.Run(() =>
            {
                Parallel.ForEach(matchingFiles, (file) =>
                {
                    string fileContent;
                    using (var reader = new StreamReader(file))
                    {
                        fileContent = reader.ReadToEnd();
                    }
                    var classesInFile = ExtractClasses(fileContent, ClassExtractors.ToList());
                    foreach (var className in classesInFile)
                    {
                        allExtractedClasses.Add(className);
                    }
                });
            });

            var uniqueClasses = new HashSet<string>(allExtractedClasses);

            foreach (var className in uniqueClasses)
            {
                if (IsClassAllowed(className))
                {
                    usedClasses.Add(className);
                }
            }

            // Include classes matching safelist regexes and strings
            var combinedSafelist = SafelistRegexes.SelectMany(regex => uniqueClasses.Where(className => regex.IsMatch(className)))
                                                  .Union(SafelistStrings);
            foreach (var className in combinedSafelist)
            {
                usedClasses.Add(className);
            }

            return usedClasses;
        }







        /// <summary>
        /// Checks if the given class name is allowed based on the blocklist strings 
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        private bool IsClassAllowed(string className)
        {
            // Check blocklist strings
            if (BlocklistStrings.Contains(className))
            {
                return false;
            }

            // Check blocklist regexes
            foreach (var regex in BlocklistRegexes)
            {
                if (regex.IsMatch(className))
                {
                    return false;
                }
            }

            return true;
        }

        private HashSet<string> ExtractClasses(string fileContent, List<IClassExtractor> classExtractors)
        {
            var classes = new HashSet<string>();
            foreach (var extractor in classExtractors)
            {
                if (extractor == null)
                {
                    continue;
                }

                var extractedClasses = extractor.ExtractClasses(fileContent);
                foreach (var className in extractedClasses)
                {
                    classes.Add(className);
                }
            }
            return classes;
        }



        /// <summary>
        /// Retrieves all class selectors from the given rule's selector text. This method does not include pseudo-classes as the generated classes we're looking for (e.g hover_bg-red-500) wouldn't have them attached.
        /// </summary>
        /// <param name="selectorText"></param>
        /// <returns></returns>
        public static List<string> GetClassSelectorsFromRuleWithoutPseudoClasses(string selectorText)
        {
            // Regex to extract class selectors and ignore pseudo-classes
            var classSelectors = Regex.Matches(selectorText, @"\.[\w-]+(?![\w-])")
                                      .Cast<Match>()
                                      .Select(m => m.Value)
                                      .ToList();

            return classSelectors;
        }

        /// <summary>
        /// Signifier used to inform the purger to start ignoring a section of uss content, and to leave it untouched
        /// </summary>
        public static string PURGE_START_IGNORE_SIGNIFIER = "/* purge start ignore */";

        /// <summary>
        /// Signifier used to inform the purger to stop ignoring a section of uss content, and to start purging again
        /// </summary>
        public static string PURGE_END_IGNORE_SIGNIFIER = "/* purge end ignore */";

        public List<(string Section, bool IsIgnored)> SplitUssContentIntoSections(string ussFileContent)
        {
            var sections = new List<(string Section, bool IsIgnored)>();
            int startIndex = 0;
            bool isIgnoredSection = false;

            while (startIndex < ussFileContent.Length)
            {
                int startSignifierIndex = ussFileContent.IndexOf(PURGE_START_IGNORE_SIGNIFIER, startIndex);
                int endSignifierIndex = ussFileContent.IndexOf(PURGE_END_IGNORE_SIGNIFIER, startIndex);

                // If no more signifiers are found, add the remaining content as a section
                if (startSignifierIndex == -1 && endSignifierIndex == -1)
                {
                    string remainingContent = ussFileContent.Substring(startIndex).Trim();
                    if (!string.IsNullOrEmpty(remainingContent))
                    {
                        sections.Add((remainingContent, isIgnoredSection));
                    }
                    break;
                }

                if (startSignifierIndex != -1 && (startSignifierIndex < endSignifierIndex || endSignifierIndex == -1))
                {
                    // Add content before the start signifier as a non-ignored section
                    if (startSignifierIndex > startIndex)
                    {
                        string sectionContent = ussFileContent.Substring(startIndex, startSignifierIndex - startIndex).Trim();
                        if (!string.IsNullOrEmpty(sectionContent))
                        {
                            sections.Add((sectionContent, false));
                        }
                    }
                    isIgnoredSection = true;
                    startIndex = startSignifierIndex + PURGE_START_IGNORE_SIGNIFIER.Length;
                }
                else if (endSignifierIndex != -1)
                {
                    // Add content before the end signifier as an ignored section
                    if (endSignifierIndex > startIndex)
                    {
                        string sectionContent = ussFileContent.Substring(startIndex, endSignifierIndex - startIndex).Trim();
                        if (!string.IsNullOrEmpty(sectionContent))
                        {
                            sections.Add((sectionContent, true));
                        }
                    }
                    isIgnoredSection = false;
                    startIndex = endSignifierIndex + PURGE_END_IGNORE_SIGNIFIER.Length;
                }
            }

            return sections;
        }

        private List<string> ProcessChunk(string ussContentChunk, string selector)
        {

            // Escaping special CSS characters
            string[] specialCharacters = new string[] { ">", "+", "~", "[", "]", ":", ".", "#" };
            string pattern = "";

            // Building a regex pattern from the selector
            foreach (char c in selector)
            {
                if (char.IsWhiteSpace(c))
                {
                    // Replace whitespace with a regex pattern that matches any amount of whitespace
                    pattern += @"\s*";
                }
                else if (specialCharacters.Contains(c.ToString()))
                {
                    // For special characters, add them to the pattern with optional whitespace around
                    pattern += @"\s*" + Regex.Escape(c.ToString()) + @"\s*";
                }
                else
                {
                    // For other characters, just escape them as they are
                    pattern += Regex.Escape(c.ToString());
                }
            }

            // Finalizing the pattern to match the entire selector and its styles
            pattern = pattern + @"\s*\{(.*?)\}";


            MatchCollection matches = Regex.Matches(ussContentChunk, pattern, RegexOptions.Singleline);
            List<string> styles = new List<string>();



            if (matches.Count == 0)
            {
                return styles; // No matches found
            }

            foreach (Match match in matches)
            {
                // Adding each matched style to the list
                styles.Add("{" + match.Groups[1].Value + "}");
            }

            return styles;
        }

        /// <summary>
        /// Retrieves the styles for the given selector from the given uss file along with the braces e.g {background-color: red;}.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public List<string> GetStyleBracesForSelector(string ussContent, string selector)
        {
            List<string> styles = new List<string>();
            int chunkSize = 10000; // Example chunk size
            int totalLength = ussContent.Length;
            int processedLength = 0;

            while (processedLength < totalLength)
            {
                int lengthToProcess = Math.Min(chunkSize, totalLength - processedLength);
                string chunk = ussContent.Substring(processedLength, lengthToProcess);

                // Ensure the chunk ends in a complete rule
                int lastRuleEnd = chunk.LastIndexOf('}');
                if (lastRuleEnd != -1 && processedLength + lengthToProcess < totalLength)
                {
                    chunk = chunk.Substring(0, lastRuleEnd + 1);
                }

                // Process the chunk
                styles.AddRange(ProcessChunk(chunk, selector));

                processedLength += chunk.Length;
            }

            return styles;
        }



        /// <summary>
        /// Checks if the given selector is used in the project. 
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="usedClassesInProject"></param>
        /// <returns></returns>
        public bool IsUsedSelector(string selector, HashSet<string> usedClassesInProject)
        {
            var classSelectorsWithoutPseudoClasses = GetClassSelectorsFromRuleWithoutPseudoClasses(selector);

            // Check if any of the class selectors in the rule are in the used classes list
            foreach (var classSelector in classSelectorsWithoutPseudoClasses)
            {
                // Remove the leading '.' before comparison if it starts with a '.'
                string classNameWithoutDot = classSelector.Substring(1);
                if (usedClassesInProject.Contains(classNameWithoutDot))
                {
                    return true;
                }
            }

            return false;
        }

    }
}
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace ZoboUI.Editor
{
    /// <summary>
    /// This class takes the stored value for an asset in the theme config or uss file and gets the type and path. These assets are usually stored as resource() or url() values in the uss file.
    /// </summary>
    public class AssetValueValidator
    {

        public enum AssetValueType
        {


            None = 2,

            Url = 0,
            Resource = 1,


        }



        // Method to check if a path is within the Resources folder
        private bool IsResourcePath(string path)
        {
            // Valid resource paths start with "Resources/" or contain "/Resources/" as well as "Editor Default Resources/" or "/Editor Default Resources/"
            return path.Contains("/Resources/") || path.StartsWith("Resources/") || path.Contains("/Editor Default Resources/") || path.StartsWith("Editor Default Resources/");
        }

        private static bool IsValidPathFormat(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            char[] invalidPathChars = Path.GetInvalidPathChars();

            if (path.Any(p => invalidPathChars.Contains(p)))
            {
                return false;
            }


            try
            {
                FileInfo fileInfo = new FileInfo(path);
            }
            catch (ArgumentException)
            {
                // Path contains invalid characters
                return false;
            }
            catch (PathTooLongException)
            {
                // Path is too long
                return false;
            }
            catch (NotSupportedException)
            {
                // Path is in an invalid format
                return false;
            }

            return true;
        }

        /// <summary>
        /// This takes a string, checks if it is a path to an asset, and returns whether it is a resource or url, as well as the path to the asset
        /// </summary>
        /// <param name="assetValue"></param>
        /// <returns></returns>
        public (AssetValueType, string) GetAssetValueTypeAndPath(string assetValue)
        {
            // If the asset value is empty, return an empty string
            if (string.IsNullOrEmpty(assetValue))
            {
                return (AssetValueType.None, "");
            }

            // Define a regex pattern to match the asset value
            string pattern = @"^(url|resource)\(([""'])(.*?)(?<!\\)\2\)$";
            Regex regex = new Regex(pattern);

            // Match the pattern against the asset value
            Match match = regex.Match(assetValue);

            // If the match is successful, extract the asset type and path
            if (match.Success)
            {
                string assetType = match.Groups[1].Value;
                string assetPath = match.Groups[3].Value;

                // Check if the assetPath starts with "project://database/" and remove it
                string prefix = "project://database/";
                if (assetPath.StartsWith(prefix))
                {
                    assetPath = assetPath.Substring(prefix.Length);
                }

                if (assetType == "url")
                {
                    return (AssetValueType.Url, assetPath);
                }
                else if (assetType == "resource")
                {
                    return (AssetValueType.Resource, assetPath);
                }
            }

            // If the asset value doesn't match the pattern, it is a custom value
            return (AssetValueType.None, assetValue);
        }

        protected string FormatUrlAssetPathForStorage(string path)
        {
            if (string.IsNullOrEmpty(path) || !IsValidPathFormat(path))
            {
                throw new ArgumentException("Invalid or empty asset path");
            }

            // If it starts with "project://", remove it
            if (path.StartsWith("project://"))
            {
                path = path.Substring(10);
            }

            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            string escapedAssetPath = path.Replace("'", "\\'");

            return $"url('project://database/{escapedAssetPath}')";
        }



        /// <summary>
        /// Formats a given asset path string for storage. This method automatically determines whether the path is a URL or a local resource path within a Unity 'Resources' folder and formats the string accordingly.
        /// </summary>
        /// <param name="path">The asset path to be formatted. This should be a valid path string, either a URL or a local path to an asset within the Unity project.</param>
        /// <returns>A formatted asset value string. If the path is a URL, it returns in the format 'url('path')'. If it's a local resource, it returns in the format 'resource('path')'.</returns>
        public string FormatAssetPathStringForStorage(string path)
        {
            if (string.IsNullOrEmpty(path) || !IsValidPathFormat(path))
            {
                throw new ArgumentException("Invalid or empty asset path");
            }

            string escapedAssetPath = path.Replace("'", "\\'");
            AssetValueType assetValueType;

            if (IsResourcePath(path))
            {
                assetValueType = AssetValueType.Resource;

                // If the asset is in the Resources folder, remove the file extension as required by Unity: https://docs.unity3d.com/Manual/UIE-USS-PropertyTypes.html

                if (!path.Contains("Editor Default Resources"))
                {
                    escapedAssetPath = Path.ChangeExtension(escapedAssetPath, null);
                }
            }
            else
            {
                assetValueType = AssetValueType.Url;
            }

            switch (assetValueType)
            {
                case AssetValueType.Url:
                    return FormatUrlAssetPathForStorage(escapedAssetPath);
                case AssetValueType.Resource:
                    return $"resource('{escapedAssetPath}')";
                default:
                    throw new Exception("Invalid asset value type");
            }
        }


    }
}
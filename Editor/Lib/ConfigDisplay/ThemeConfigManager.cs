using UnityEngine;
using System.Collections.Generic;
using ZoboUI.Editor;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using ZoboUI.Core.Utils;





namespace ZoboUI.Core
{



    [CreateAssetMenu(fileName = "ZoboUIThemeConfigManager", menuName = "ZoboUI/Theme Config Manager")]
    public class ThemeConfigManager : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>
        /// The name of the package. 
        /// </summary>
        public static readonly string PACKAGE_NAME = "ZoboUI";

        [SerializeField] private UnityEngine.TextAsset m_importThemeConfigJsonFile;
        [SerializeField] private string m_exportConfigJsonFilePath = "Assets/exported-theme-config.json";

        [SerializeField] private LogLevel logLevel = LogLevel.Progress;


        /// <summary>
        /// The json string that is used to backup the config display version. This is used to load the config display version back into the inspector if the ThemeConfigDisplayVersion is null e.g because the classes referenced are updated during a package update
        /// </summary>
        [HideInInspector]
        [SerializeField] private string configDisplayBackupJson;

        private float lastUpdateTimeForConfig = 0;
        private float lastBackupTime = 0;
        private const float backupThreshold = 2.0f;



        /// <summary>
        /// The path to the json file that this ThemeConfigManager will export to.
        /// </summary>
        public string ExportConfigJsonFilePath { get => m_exportConfigJsonFilePath; set => m_exportConfigJsonFilePath = value; }



        public LogLevel LogLevel
        {
            get
            {
                return logLevel;
            }
            set
            {
                logLevel = value;
                if (Logger != null)
                    Logger.LogLevel = value;
            }
        }


        private ICustomLogger logger;

        public ICustomLogger Logger
        {
            get
            {
                if (logger == null)
                {
                    logger = new CustomLogger(logLevel: logLevel, prefix: PACKAGE_NAME);
                }

                return logger;
            }
        }


        private ConfigDisplayHandler configDisplayHandler;

        protected ConfigDisplayHandler ConfigDisplayHandler
        {
            get
            {
                if (configDisplayHandler == null)
                {
                    configDisplayHandler = new ConfigDisplayHandler(Logger);
                }

                return configDisplayHandler;
            }
        }

        /// <summary>
        /// The json string that is used to backup the config display version. This is used to load the config display version back into the inspector if the ThemeConfigDisplayVersion is null e.g because the classes referenced are updated during a package update
        /// </summary>
        public string BackupJson { get => configDisplayBackupJson; }





        [Tooltip("The path to the generated uss file. This file will be overwritten every time the generator is run.")]
        [SerializeField] private string m_ussFilePath = "Assets/Styles/generated.uss";


        [SerializeField] private string m_purgedUssFilePath = "Assets/Styles/purged.uss";


        /// <summary>
        /// The path to the generated uss file. This file will be overwritten every time the generator is run.
        /// </summary>
        public string GeneratedUssFilePath { get => m_ussFilePath; set => m_ussFilePath = value; }

        /// <summary>
        /// The path to the purged uss file. This file only contains the classes that are used in the project.
        /// </summary>
        public string PurgedUssFilePath { get => m_purgedUssFilePath; set => m_purgedUssFilePath = value; }



        [SerializeField]
        private ThemeConfigDisplayVersion themeConfigDisplay;

        public ThemeConfigDisplayVersion ThemeConfigDisplay
        {
            get
            {
                return themeConfigDisplay;
            }
            set
            {
                themeConfigDisplay = value;
            }
        }

        /// <summary>
        /// This is generated from the current data stored in the ThemeConfigManager. Note that updating this property will not update the ThemeConfigManager's data. To update the ThemeConfigManager's data, you'll need to use the LoadFromThemeConfig method with an updated ThemeConfig value.
        /// </summary>
        public ThemeConfig ThemeConfig
        {
            get
            {
                if (themeConfigDisplay == null)
                {
                    Logger.LogWarning("Theme config display is null, using default theme config instead");
                    LoadDefaultThemeConfig();
                }

                return ConfigDisplayHandler.ConvertThemeConfigDisplayVersionToThemeConfig(themeConfigDisplay);
                ;
            }
        }




        /// <summary>
        /// Initializes the ThemeConfigManager with the provided json string. This will overwrite any existing data in the ThemeConfigManager.
        /// </summary>
        /// <param name="jsonString"></param>
        public void LoadThemeConfigDisplayFromJsonString(string jsonString)
        {


            ThemeConfig config = ThemeConfig.FromJson(jsonString);

            LoadFromThemeConfig(config);
        }

        /// <summary>
        /// Initializes the ThemeConfigManager with the provided theme config. This will overwrite any existing data in the ThemeConfigManager.
        /// </summary>
        /// <param name="config"></param>
        public void LoadFromThemeConfig(ThemeConfig config)
        {
            themeConfigDisplay = ConfigDisplayHandler.ConvertThemeConfigToDisplayVersion(config);
            lastUpdateTimeForConfig = Time.realtimeSinceStartup;
            Logger.Log("Loaded from theme config");
        }

        /// <summary>
        /// Loads the default theme config display. This is the default theme config that is used when the ThemeConfigManager is first created.
        /// </summary>
        public void LoadDefaultThemeConfig()
        {
            ThemeConfig themeConfig = new ThemeConfig();
            LoadFromThemeConfig(themeConfig);
            return;
        }

        /// <summary>
        /// Loads the config backup json string into the ThemeConfigDisplayVersion. This is used to load the config display version back into the inspector if the ThemeConfigDisplayVersion is null e.g because the classes referenced are updated during a package update
        /// </summary>
        public void LoadConfigBackup()
        {
            if (string.IsNullOrEmpty(configDisplayBackupJson))
            {
                Logger.LogWarning("No config backup found");
                return;
            }

            LoadThemeConfigDisplayFromJsonString(configDisplayBackupJson);
        }

        private DefaultClassExtractor GetDefaultClassExtractor()
        {
            DefaultClassExtractor.Config config = new DefaultClassExtractor.Config();
            ThemeConfig currentThemeConfig = this.ThemeConfig;

            config.ModifierSeparator = currentThemeConfig.core.modifierSeparator;
            config.CustomPrefix = currentThemeConfig.core.prefix;
            DefaultClassExtractor classExtractor = new DefaultClassExtractor(config);
            return classExtractor;
        }

        /// <summary>
        /// Gets the root directory of the project. We choose to use the project directory as the root directory instead of the Assets directory because it makes copying and pasting file paths easier in the Unity editor as they default to the project directory.
        /// </summary>
        /// <returns></returns>
        private string GetProjectRootDirectory()
        {
            // Gets the path to the "Assets" folder
            string assetsPath = Application.dataPath;

            // To get the root project directory, go up one level from the "Assets" directory
            string projectPath = Path.GetDirectoryName(assetsPath);


            return projectPath;
        }

        /// <summary>
        /// Generates the uss file with all the classes in the theme config.
        /// </summary>
        public void GenerateUSSFile(ICustomLogger logger = null)
        {
            ICustomLogger customLogger = logger ?? this.Logger;
            ThemeUssGenerator themeUssGenerator = new ThemeUssGenerator(customLogger);

            string ussContent = themeUssGenerator.GenerateUssFileContentFromThemeConfig(this.ThemeConfig);

            WriteFileToDisk(GeneratedUssFilePath, ussContent);

            Logger.LogProgress($"Generated USS file at {this.GeneratedUssFilePath}");

            // Ping the file in the project window
            PingFileInProjectWindow(GeneratedUssFilePath);

        }

        /// <summary>
        /// Generates the purged uss file. This file only contains the classes that are used in the project.
        /// </summary>
        /// <param name="customClassExtractor"></param>
        public void GeneratePurgedUSSFile(IClassExtractor customClassExtractor = null, ICustomLogger logger = null)
        {
            ICustomLogger customLogger = logger ?? this.Logger;
            IClassExtractor extractor = customClassExtractor ?? GetDefaultClassExtractor();

            ThemeConfig currentThemeConfig = this.ThemeConfig;

            List<IClassExtractor> classExtractors = new List<IClassExtractor>() { extractor };

            UssPurger ussPurger = new UssPurger(classExtractors, contentPatterns: currentThemeConfig.compilation.content, safelist: currentThemeConfig.compilation.safelist, blocklist: currentThemeConfig.compilation.blocklist, customLogger);

            string projectRootDirectory = GetProjectRootDirectory();

            ThemeUssGenerator themeUssGenerator = new ThemeUssGenerator(customLogger);

            string ussContent = themeUssGenerator.GenerateUssFileContentFromThemeConfig(currentThemeConfig, new ThemeUssGenerator.PurgeSettings { UssPurger = ussPurger, RootDirectoryPath = projectRootDirectory });

            WriteFileToDisk(PurgedUssFilePath, ussContent);

            Logger.LogProgress("Purged USS file generated at " + PurgedUssFilePath);

            // Ping the file in the project window
            PingFileInProjectWindow(PurgedUssFilePath);
        }



        /// <summary>
        /// Imports the theme config from the provided json file in the import path.
        /// </summary>
        public void ImportThemeConfigFromJson()
        {
            if (m_importThemeConfigJsonFile == null)
            {
                Logger.LogWarning("No import file provided, using the default theme config instead");
                LoadDefaultThemeConfig();
                return;
            }

            var json = m_importThemeConfigJsonFile.text;
            LoadThemeConfigDisplayFromJsonString(json);
        }

        /// <summary>
        /// Exports the theme config to json using the provided export path.
        /// </summary>
        public void ExportThemeConfigToJson()
        {
            if (m_exportConfigJsonFilePath == null)
            {
                Logger.LogError("Export path is null");
                return;
            }

            if (themeConfigDisplay == null)
            {
                Logger.LogError("No theme config to export");
                return;
            }



            // Make sure we're converting the display version of the theme config to the actual theme config first and then serializing that

            ThemeConfig config = ConfigDisplayHandler.ConvertThemeConfigDisplayVersionToThemeConfig(themeConfigDisplay);

            var json = ThemeConfig.ToJson(config);

            WriteFileToDisk(m_exportConfigJsonFilePath, json);

            Logger.LogProgress("Exported theme config to json at " + m_exportConfigJsonFilePath);

            // Ping the file in the project window

            PingFileInProjectWindow(m_exportConfigJsonFilePath);
        }

        protected void PingFileInProjectWindow(string path)
        {
            Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            EditorGUIUtility.PingObject(obj);
        }

        /// <summary>
        /// Generates a text file with the provided content at the provided path. If the directory does not exist, it will be created.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        protected void WriteFileToDisk(string path, string content)
        {

            if (string.IsNullOrEmpty(path))
            {
                throw new System.Exception(Logger.FormatMessage("The provided file path is null or empty"));
            }
            // Ensure the directory exists
            string directoryPath = Path.GetDirectoryName(path);

            // If the directory already exists, Directory.CreateDirectory does nothing, so it's safe to call
            Directory.CreateDirectory(directoryPath);

            // Write the content string to disk
            using (var writer = new StreamWriter(path))
            {
                writer.Write(content);
            }

            // Refresh the AssetDatabase to reflect the new file
            AssetDatabase.Refresh();



        }

        private void OnValidate()
        {
            // We need to update the lastUpdateTimeForConfig so that the config display backup is updated
            lastUpdateTimeForConfig = Time.realtimeSinceStartup;

        }

        private void BackupConfigDisplay()
        {
            // Serialize  instance whenever a change is made in the Inspector
            ThemeConfig config = ConfigDisplayHandler.ConvertThemeConfigDisplayVersionToThemeConfig(themeConfigDisplay);

            var json = ThemeConfig.ToJson(config);

            // Backup myClassInstance if it's non-empty as we don't want to overwrite it with an empty string
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            configDisplayBackupJson = json;

        }


        // We backup the config display version to json so that we can load it back in the inspector if the ThemeConfigDisplayVersion is null e.g because the classes referenced are updated during a package update
        public void OnBeforeSerialize()
        {

            // Backup the themeConfigDisplay if it's non-empty
            if (themeConfigDisplay != null)
            {
                float timeDifferenceSinceLastBackup = Time.realtimeSinceStartup - lastBackupTime;

                bool dataChanged = lastUpdateTimeForConfig > lastBackupTime;

                if (timeDifferenceSinceLastBackup >= backupThreshold && dataChanged)
                {
                    try
                    {
                        BackupConfigDisplay();

                    }
                    catch (System.Exception e)
                    {
                        Logger.Log("Unable to serialize theme config display to json" + e.Message);
                    }
                    lastBackupTime = Time.realtimeSinceStartup;
                }

            }
        }

        public void OnAfterDeserialize()
        {

        }
    }


}



using System.IO;
using ZoboUI.Core;
using ZoboUI.Core.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Search;

namespace ZoboUI.Editor.Inspectors
{
    [CustomEditor(typeof(ThemeConfigManager))]
    public class ThemeConfigManagerInspector : UnityEditor.Editor
    {
        public VisualTreeAsset m_InspectorXML;


        private readonly string ExportConfigButtonName = "ExportConfigButton";
        private readonly string ImportConfigButtonName = "ImportConfigButton";

        private readonly string GenerateUSSButtonName = "GenerateUSSButton";

        private readonly string PurgeButtonName = "PurgeButton";

        private readonly string LogLevelEnumDropdownName = "LogLevelField";

        private void ExportThemeConfigToJson()
        {
            ThemeConfigManager themeConfigManager = (ThemeConfigManager)target;

            themeConfigManager.ExportThemeConfigToJson();

        }

        private void ImportThemeConfigFromJson()
        {
            ThemeConfigManager themeConfigManager = (ThemeConfigManager)target;

            themeConfigManager.ImportThemeConfigFromJson();

            EditorUtility.SetDirty(themeConfigManager);

        }

        private void GenerateUSS()
        {
            ThemeConfigManager themeConfigManager = (ThemeConfigManager)target;

            themeConfigManager.GenerateUSSFile();
        }

        private void Purge()
        {
            ThemeConfigManager themeConfigManager = (ThemeConfigManager)target;

            themeConfigManager.GeneratePurgedUSSFile();


        }

        public override VisualElement CreateInspectorGUI()
        {
            // Create a new VisualElement to be the root of our inspector UI
            VisualElement myInspector = new VisualElement();

            Label loadingConfigLabel = new Label("Loading config...");
            myInspector.Add(loadingConfigLabel);

            VisualElement loadedUxmlContent = new VisualElement();

            loadedUxmlContent.style.visibility = Visibility.Hidden;

            // Load from default reference
            m_InspectorXML.CloneTree(loadedUxmlContent);

            myInspector.Add(loadedUxmlContent);

            ThemeConfigManager themeConfigManager = (ThemeConfigManager)target;





            // This usually happens when the user just created the ThemeConfigManager scriptable object, or if the classes related to the theme config display are updated (e.g because of a package update) and Unity is not able to deserialize the old data. We have manually load from the backup json in this case.
            if (themeConfigManager.ThemeConfigDisplay == null || themeConfigManager.ThemeConfigDisplay?.Core == null || themeConfigManager.ThemeConfigDisplay?.Core.Count == 0)
            {
                if (!string.IsNullOrEmpty(themeConfigManager.BackupJson))
                {
                    themeConfigManager.Logger.Log("Rebuilding theme config...");
                    themeConfigManager.LoadConfigBackup();

                }
                else
                {
                    themeConfigManager.Logger.Log("Building theme config...");

                    themeConfigManager.LoadDefaultThemeConfig();
                }


            }

            TextField OutputUssFilePathTextField = myInspector.Q<TextField>("OutputUssFilePathTextField");

            TextField PurgedUssFilePathTextField = myInspector.Q<TextField>("PurgedUssFilePathTextField");


            ObjectField ImportJsonFileField = myInspector.Q<ObjectField>("ImportJsonFileField");


            // Get the export button from the UXML and assign it its click event
            Button exportConfigButton = myInspector.Q<Button>(ExportConfigButtonName);
            exportConfigButton.clickable.clicked += ExportThemeConfigToJson;

            // Get the import button from the UXML and assign it its click event
            Button importConfigButton = myInspector.Q<Button>(ImportConfigButtonName);
            importConfigButton.clickable.clicked += ImportThemeConfigFromJson;

            // Get the generate USS button from the UXML and assign it its click event
            Button generateUSSButton = myInspector.Q<Button>(GenerateUSSButtonName);
            generateUSSButton.clickable.clicked += GenerateUSS;

            // Get the purge button from the UXML and assign it its click event
            Button purgeButton = myInspector.Q<Button>(PurgeButtonName);
            purgeButton.clickable.clicked += Purge;



            // Get the log level enum dropdown from the UXML
            EnumField logLevelEnumDropdown = myInspector.Q<EnumField>(LogLevelEnumDropdownName);
            // Listen for changes to the enum dropdown and set the log level of the logger
            logLevelEnumDropdown.RegisterValueChangedCallback(evt =>
            {
                LogLevel logLevel = (LogLevel)evt.newValue;
                themeConfigManager.LogLevel = logLevel;

            });

            // This value changes when the data is first loaded from the data binding so its a good way to know when the data is loaded in the inspector
            OutputUssFilePathTextField.RegisterValueChangedCallback(evt =>
            {
                // When the inspector's data is loaded, hide the loading label and show the inspector content. This prevents the user from seeing the empty inspector fields while the data is loading
                if (loadedUxmlContent.style.visibility == Visibility.Hidden)
                {
                    loadedUxmlContent.style.visibility = Visibility.Visible;
                    loadingConfigLabel.style.display = DisplayStyle.None;
                }
            });

            // Return the finished inspector UI
            return myInspector;
        }

    }
}



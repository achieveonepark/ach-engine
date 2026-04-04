using System;
using System.IO;
using AchEngine.Editor.Table;
using AchEngine.Editor.UI;
using AchEngine.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor
{
    internal static class AchEngineSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider("Project/AchEngine", SettingsScope.Project)
            {
                label = "AchEngine",
                activateHandler = BuildUI,
                keywords = new[] { "achengine", "di", "table", "ui", "vcontainer", "memorypack" }
            };
        }

        private static void BuildUI(string searchContext, VisualElement root)
        {
            root.style.paddingLeft = 0;
            root.style.paddingRight = 0;
            root.style.paddingTop = 0;
            root.style.paddingBottom = 0;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1f;

            var ussPath = FindAssetPath<StyleSheet>("TableLoaderWindow");
            if (ussPath != null)
            {
                root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath));
            }

            var tabBar = new VisualElement();
            tabBar.style.flexDirection = FlexDirection.Row;
            tabBar.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            tabBar.style.borderBottomWidth = 1f;
            tabBar.style.borderBottomColor = new StyleColor(new Color(0.33f, 0.33f, 0.33f));
            root.Add(tabBar);

            var container = new ScrollView();
            container.style.flexGrow = 1f;
            container.contentContainer.style.paddingLeft = 16f;
            container.contentContainer.style.paddingRight = 16f;
            container.contentContainer.style.paddingTop = 16f;
            container.contentContainer.style.paddingBottom = 16f;
            root.Add(container);

            var panelOverview = BuildOverviewPanel();
            var panelTable = BuildTablePanel();
            var panelUI = BuildUIPanel();

            container.Add(panelOverview);
            container.Add(panelTable);
            container.Add(panelUI);

            panelTable.AddToClassList("panel-hidden");
            panelUI.AddToClassList("panel-hidden");

            string[] tabLabels = { "Overview", "Table Loader", "UI Workspace" };
            VisualElement[] panels = { panelOverview, panelTable, panelUI };

            for (int i = 0; i < tabLabels.Length; i++)
            {
                var index = i;
                var button = MakeTabButton(tabLabels[i], i == 0);
                button.RegisterCallback<ClickEvent>(_ => SwitchTab(tabBar, panels, index));
                tabBar.Add(button);
            }
        }

        private static void SwitchTab(VisualElement tabBar, VisualElement[] panels, int activeIndex)
        {
            var buttons = tabBar.Query<Button>().ToList();
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i == activeIndex)
                {
                    buttons[i].AddToClassList("tab-active");
                }
                else
                {
                    buttons[i].RemoveFromClassList("tab-active");
                }
            }

            for (int i = 0; i < panels.Length; i++)
            {
                if (i == activeIndex)
                {
                    panels[i].RemoveFromClassList("panel-hidden");
                }
                else
                {
                    panels[i].AddToClassList("panel-hidden");
                }
            }
        }

        private static VisualElement BuildOverviewPanel()
        {
            var root = new VisualElement();

            root.Add(MakeSectionTitle("AchEngine"));
            root.Add(MakeSectionBody(
                "AchEngine is a Unity toolkit that combines VContainer-based DI, UI scene helpers, and a Google Sheets table pipeline."));

            root.Add(MakeDivider());
            root.Add(MakeSectionTitle("Package Info"));

            var infoGrid = new VisualElement();
            infoGrid.style.flexDirection = FlexDirection.Column;
            infoGrid.style.marginBottom = 12f;

            AddInfoRow(infoGrid, "Package ID", "com.engine.achieve");
            AddInfoRow(infoGrid, "Version", "1.0.0");
            AddInfoRow(infoGrid, "Unity", "2021.3+");
            AddInfoRow(infoGrid, "Required", "com.unity.ugui");
            AddInfoRow(infoGrid, "Optional", "VContainer, MemoryPack");
            root.Add(infoGrid);

            root.Add(MakeDivider());
            root.Add(MakeSectionTitle("Installation"));
            root.Add(MakeSectionBody(
                "VContainer (recommended)\n" +
                "  UPM: https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.16.7\n" +
                "  Defines the ACHENGINE_VCONTAINER symbol automatically.\n\n" +
                "MemoryPack (optional)\n" +
                "  UPM: https://github.com/Cysharp/MemoryPack.git?path=src/MemoryPack.Unity/Assets/Plugins/MemoryPack\n" +
                "  Defines the ACHENGINE_MEMORYPACK symbol automatically."));

            root.Add(MakeDivider());
            root.Add(MakeSectionTitle("Editor Menu"));
            root.Add(MakeSectionBody(
                "Tools > AchEngine > Table Loader\n" +
                "Tools > AchEngine > UI Workspace"));

            return root;
        }

        private static VisualElement BuildTablePanel()
        {
            var root = new VisualElement();

            root.Add(MakeSectionTitle("Table Loader Settings"));
            root.Add(MakeSectionBody("Settings are stored in Assets/TableLoaderSettings.asset."));

            var settings = TableLoaderSettings.GetOrCreate();
            var serializedObject = new SerializedObject(settings);

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Google Spreadsheet"));

            var spreadsheetField = new TextField("Spreadsheet ID")
            {
                value = settings.spreadsheetId,
                isDelayed = true
            };
            spreadsheetField.style.marginBottom = 6f;
            spreadsheetField.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                serializedObject.FindProperty("spreadsheetId").stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
            });
            root.Add(spreadsheetField);

            var openButton = new Button(() =>
            {
                if (!string.IsNullOrEmpty(settings.spreadsheetId))
                {
                    Application.OpenURL(settings.GetSpreadsheetUrl());
                }
            })
            {
                text = "Open in Browser"
            };
            openButton.AddToClassList("btn-secondary");
            openButton.style.marginBottom = 12f;
            root.Add(openButton);

            root.Add(MakeSubTitle("Paths"));
            root.Add(MakeSerializedTextField(serializedObject, settings, "csvOutputPath", "CSV Output Path"));
            root.Add(MakeSerializedTextField(serializedObject, settings, "codeOutputPath", "Generated Code Path"));
            root.Add(MakeSerializedTextField(serializedObject, settings, "binaryOutputPath", "Binary Output Path"));

            root.Add(MakeSubTitle("Automation"));
            root.Add(MakeSerializedToggle(serializedObject, settings, "autoGenerateOnDownload", "Auto-generate after download"));
            root.Add(MakeSerializedToggle(serializedObject, settings, "autoBakeOnGenerate", "Auto-bake after code generation"));

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Sheets"));

            var sheetContainer = new VisualElement();
            sheetContainer.style.marginBottom = 8f;

            Action refreshSheets = null;
            refreshSheets = () =>
            {
                sheetContainer.Clear();
                for (int i = 0; i < settings.sheets.Count; i++)
                {
                    var index = i;
                    var sheet = settings.sheets[i];
                    var row = MakeSheetRow(sheet, () =>
                    {
                        settings.sheets.RemoveAt(index);
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        refreshSheets?.Invoke();
                    });
                    sheetContainer.Add(row);
                }
            };
            refreshSheets();
            root.Add(sheetContainer);

            var addSheetButton = new Button(() =>
            {
                settings.sheets.Add(new SheetInfo());
                EditorUtility.SetDirty(settings);
                refreshSheets();
            })
            {
                text = "+ Add Sheet"
            };
            addSheetButton.AddToClassList("btn-primary");
            addSheetButton.style.marginBottom = 12f;
            root.Add(addSheetButton);

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Pipeline"));
            root.Add(MakeSectionBody(
                "Save settings here or open the dedicated window at Tools > AchEngine > Table Loader."));

            var pipelineRow = new VisualElement();
            pipelineRow.style.flexDirection = FlexDirection.Row;
            pipelineRow.style.flexWrap = Wrap.Wrap;
            pipelineRow.style.marginTop = 8f;

            var saveButton = new Button(() =>
            {
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            })
            {
                text = "Save Settings"
            };
            saveButton.AddToClassList("btn-primary");
            pipelineRow.Add(saveButton);

            var openWindowButton = new Button(TableLoaderWindow.ShowWindow)
            {
                text = "Open Table Loader"
            };
            openWindowButton.AddToClassList("btn-secondary");
            pipelineRow.Add(openWindowButton);

            root.Add(pipelineRow);

            return root;
        }

        private static VisualElement BuildUIPanel()
        {
            var root = new VisualElement();

            root.Add(MakeSectionTitle("UI Workspace"));
            root.Add(MakeSectionBody(
                "Use the UI workspace to manage catalogs, scene setup, and validation from one place."));

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Current Scene State"));
            root.Add(MakeReadonlyInfo("UI Root", GetSceneObjectName(typeof(UIRoot))));
            root.Add(MakeReadonlyInfo("Bootstrapper", GetSceneObjectName(typeof(UIBootstrapper))));

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Quick Actions"));

            var actionRow = new VisualElement();
            actionRow.style.flexDirection = FlexDirection.Row;
            actionRow.style.flexWrap = Wrap.Wrap;
            actionRow.style.marginBottom = 8f;

            var createRootButton = new Button(() =>
            {
                var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
                if (existingRoot == null)
                {
                    AchEngineUIEditorUtility.CreateUIRoot();
                }
            })
            {
                text = "Create UI Root"
            };
            createRootButton.AddToClassList("btn-primary");

            var openWorkspaceButton = new Button(() => AchEngineUIWorkspaceWindow.Open(null))
            {
                text = "Open UI Workspace"
            };
            openWorkspaceButton.AddToClassList("btn-secondary");

            actionRow.Add(createRootButton);
            actionRow.Add(openWorkspaceButton);
            root.Add(actionRow);

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("Layer Order"));
            root.Add(MakeSectionBody(
                "Background (0)\n" +
                "Screen (10)\n" +
                "Popup (20)\n" +
                "Overlay (30)\n" +
                "Tooltip (40)"));

            root.Add(MakeDivider());
            root.Add(MakeSubTitle("VContainer Setup"));
            root.Add(MakeSectionBody(
                "Add AchEngineScope to your scene when you want VContainer to register IUIService and ITableService automatically.\n\n" +
                "  [Inject] readonly IUIService uiService;\n" +
                "  uiService.Show(\"MainMenu\");"));

            return root;
        }

        private static Button MakeTabButton(string label, bool active)
        {
            var button = new Button { text = label };
            button.style.flexGrow = 1f;
            button.style.paddingTop = 8f;
            button.style.paddingBottom = 8f;
            button.style.marginLeft = 0f;
            button.style.marginRight = 0f;
            button.style.marginTop = 0f;
            button.style.marginBottom = 0f;
            button.style.borderLeftWidth = 0f;
            button.style.borderRightWidth = 0f;
            button.style.borderTopWidth = 0f;
            button.style.borderBottomWidth = 2f;
            button.style.borderTopLeftRadius = 0f;
            button.style.borderTopRightRadius = 0f;
            button.style.borderBottomLeftRadius = 0f;
            button.style.borderBottomRightRadius = 0f;
            button.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
            button.style.color = new StyleColor(new Color(0.67f, 0.67f, 0.67f));
            button.style.fontSize = 13f;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.unityTextAlign = TextAnchor.MiddleCenter;

            if (active)
            {
                button.AddToClassList("tab-active");
            }

            return button;
        }

        private static Label MakeSectionTitle(string text)
        {
            var label = new Label(text);
            label.style.fontSize = 16f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = new StyleColor(new Color(0.36f, 0.63f, 0.83f));
            label.style.marginBottom = 6f;
            label.style.paddingBottom = 4f;
            label.style.borderBottomWidth = 1f;
            label.style.borderBottomColor = new StyleColor(new Color(0.27f, 0.27f, 0.27f));
            return label;
        }

        private static Label MakeSubTitle(string text)
        {
            var label = new Label(text);
            label.style.fontSize = 13f;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.color = new StyleColor(new Color(0.85f, 0.85f, 0.85f));
            label.style.marginTop = 8f;
            label.style.marginBottom = 4f;
            return label;
        }

        private static Label MakeSectionBody(string text)
        {
            var label = new Label(text);
            label.style.fontSize = 12f;
            label.style.color = new StyleColor(new Color(0.8f, 0.8f, 0.8f));
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.marginBottom = 8f;
            return label;
        }

        private static VisualElement MakeDivider()
        {
            var divider = new VisualElement();
            divider.style.height = 1f;
            divider.style.marginTop = 12f;
            divider.style.marginBottom = 12f;
            divider.style.backgroundColor = new StyleColor(new Color(0.27f, 0.27f, 0.27f));
            return divider;
        }

        private static VisualElement MakeReadonlyInfo(string label, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 4f;

            var labelElement = new Label(label);
            labelElement.style.width = 100f;
            labelElement.style.color = new StyleColor(new Color(0.67f, 0.67f, 0.67f));
            labelElement.style.fontSize = 12f;

            var valueElement = new Label(string.IsNullOrEmpty(value) ? "Not found in open scenes" : value);
            valueElement.style.color = string.IsNullOrEmpty(value)
                ? new StyleColor(new Color(0.91f, 0.30f, 0.24f))
                : new StyleColor(new Color(0.18f, 0.80f, 0.44f));
            valueElement.style.fontSize = 12f;

            row.Add(labelElement);
            row.Add(valueElement);
            return row;
        }

        private static TextField MakeSerializedTextField(SerializedObject serializedObject, UnityEngine.Object target, string propertyName, string label)
        {
            var field = new TextField(label)
            {
                value = serializedObject.FindProperty(propertyName).stringValue,
                isDelayed = true
            };
            field.style.marginBottom = 4f;
            field.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                serializedObject.FindProperty(propertyName).stringValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            return field;
        }

        private static Toggle MakeSerializedToggle(SerializedObject serializedObject, UnityEngine.Object target, string propertyName, string label)
        {
            var toggle = new Toggle(label)
            {
                value = serializedObject.FindProperty(propertyName).boolValue
            };
            toggle.style.marginBottom = 4f;
            toggle.RegisterValueChangedCallback(evt =>
            {
                serializedObject.Update();
                serializedObject.FindProperty(propertyName).boolValue = evt.newValue;
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            });
            return toggle;
        }

        private static VisualElement MakeSheetRow(SheetInfo sheet, Action onRemove)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 4f;
            row.style.paddingBottom = 4f;
            row.style.paddingLeft = 6f;
            row.style.paddingRight = 6f;
            row.style.marginBottom = 4f;
            row.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
            row.style.borderTopLeftRadius = 3f;
            row.style.borderTopRightRadius = 3f;
            row.style.borderBottomLeftRadius = 3f;
            row.style.borderBottomRightRadius = 3f;

            var enabledToggle = new Toggle { value = sheet.enabled };
            enabledToggle.style.width = 20f;
            enabledToggle.style.marginRight = 4f;
            enabledToggle.RegisterValueChangedCallback(evt => sheet.enabled = evt.newValue);

            var nameField = new TextField { value = sheet.sheetName };
            nameField.style.flexGrow = 2f;
            nameField.style.marginRight = 4f;
            nameField.RegisterValueChangedCallback(evt => sheet.sheetName = evt.newValue);

            var gidField = new TextField { value = sheet.sheetGid };
            gidField.style.width = 70f;
            gidField.style.marginRight = 4f;
            gidField.RegisterValueChangedCallback(evt => sheet.sheetGid = evt.newValue);

            var classField = new TextField { value = sheet.className };
            classField.style.flexGrow = 1f;
            classField.style.marginRight = 4f;
            classField.RegisterValueChangedCallback(evt => sheet.className = evt.newValue);

            var removeButton = new Button(onRemove) { text = "X" };
            removeButton.style.width = 24f;
            removeButton.style.height = 22f;
            removeButton.style.backgroundColor = new StyleColor(new Color(0.42f, 0.12f, 0.12f));
            removeButton.style.color = new StyleColor(Color.white);
            removeButton.style.borderTopLeftRadius = 3f;
            removeButton.style.borderTopRightRadius = 3f;
            removeButton.style.borderBottomLeftRadius = 3f;
            removeButton.style.borderBottomRightRadius = 3f;
            removeButton.style.borderTopWidth = 0f;
            removeButton.style.borderRightWidth = 0f;
            removeButton.style.borderBottomWidth = 0f;
            removeButton.style.borderLeftWidth = 0f;
            removeButton.style.unityTextAlign = TextAnchor.MiddleCenter;

            row.Add(enabledToggle);
            row.Add(nameField);
            row.Add(gidField);
            row.Add(classField);
            row.Add(removeButton);
            return row;
        }

        private static void AddInfoRow(VisualElement parent, string label, string value)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 3f;

            var labelElement = new Label(label);
            labelElement.style.width = 110f;
            labelElement.style.color = new StyleColor(new Color(0.6f, 0.6f, 0.6f));
            labelElement.style.fontSize = 12f;

            var valueElement = new Label(value);
            valueElement.style.color = new StyleColor(new Color(0.85f, 0.85f, 0.85f));
            valueElement.style.fontSize = 12f;

            row.Add(labelElement);
            row.Add(valueElement);
            parent.Add(row);
        }

        private static string GetSceneObjectName(Type type)
        {
            var component = UnityEngine.Object.FindObjectOfType(type) as Component;
            return component != null ? component.gameObject.name : string.Empty;
        }

        private static string FindAssetPath<T>(string name) where T : UnityEngine.Object
        {
            foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name} {name}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == name)
                {
                    return path;
                }
            }

            return null;
        }
    }
}
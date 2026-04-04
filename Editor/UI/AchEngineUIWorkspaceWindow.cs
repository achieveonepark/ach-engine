using System;
using System.IO;
using AchEngine.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchEngine.Editor.UI
{
    public sealed class AchEngineUIWorkspaceWindow : EditorWindow
    {
        private UIViewCatalog catalog;
        private SerializedObject catalogSerializedObject;
        private SerializedProperty entriesProperty;

        private VisualElement panelGuide;
        private VisualElement panelCatalog;
        private VisualElement panelScene;

        private ObjectField catalogField;
        private Label catalogStatusLabel;
        private VisualElement entriesContainer;
        private VisualElement validationContainer;
        private Button addEntryButton;
        private Button addSelectedButton;

        private ObjectField rootField;
        private ObjectField bootstrapperField;
        private Label sceneStatusLabel;
        private Button createBootstrapperButton;
        private Button assignCatalogButton;

        public static void Open(UIViewCatalog initialCatalog)
        {
            var window = GetWindow<AchEngineUIWorkspaceWindow>();
            window.titleContent = new GUIContent("UI Workspace");
            window.minSize = new Vector2(480f, 560f);
            window.Show();
            window.SetCatalog(initialCatalog);
        }

        private void OnEnable()
        {
            Selection.selectionChanged += HandleSelectionChanged;
            Undo.undoRedoPerformed += HandleUndoRedo;
            EditorApplication.hierarchyChanged += HandleHierarchyChanged;
        }

        private void OnDisable()
        {
            Selection.selectionChanged -= HandleSelectionChanged;
            Undo.undoRedoPerformed -= HandleUndoRedo;
            EditorApplication.hierarchyChanged -= HandleHierarchyChanged;
        }

        public void CreateGUI()
        {
            var uxmlPath = FindAssetPath<VisualTreeAsset>("AchEngineUIWorkspaceWindow");
            var ussPath = FindAssetPath<StyleSheet>("AchEngineUIWorkspaceWindow");

            if (uxmlPath != null)
            {
                var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
                uxml.CloneTree(rootVisualElement);
            }

            if (ussPath != null)
            {
                var uss = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                rootVisualElement.styleSheets.Add(uss);
            }

            panelGuide = rootVisualElement.Q("panel-guide");
            panelCatalog = rootVisualElement.Q("panel-catalog");
            panelScene = rootVisualElement.Q("panel-scene");

            SetupTabs();
            SetupCatalogPanel();
            SetupScenePanel();

            SetCatalog(catalog ?? Selection.activeObject as UIViewCatalog);
            RefreshSceneSection();
        }

        private void SetupTabs()
        {
            rootVisualElement.Q<Button>("tab-guide")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-guide", panelGuide));
            rootVisualElement.Q<Button>("tab-catalog")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-catalog", panelCatalog));
            rootVisualElement.Q<Button>("tab-scene")?.RegisterCallback<ClickEvent>(_ => SwitchTab("tab-scene", panelScene));
        }

        private void SwitchTab(string activeTabName, VisualElement activePanel)
        {
            string[] tabNames = { "tab-guide", "tab-catalog", "tab-scene" };
            VisualElement[] panels = { panelGuide, panelCatalog, panelScene };

            for (int i = 0; i < tabNames.Length; i++)
            {
                var button = rootVisualElement.Q<Button>(tabNames[i]);
                if (tabNames[i] == activeTabName)
                {
                    button?.AddToClassList("tab-active");
                    panels[i]?.RemoveFromClassList("panel-hidden");
                }
                else
                {
                    button?.RemoveFromClassList("tab-active");
                    panels[i]?.AddToClassList("panel-hidden");
                }
            }
        }

        private void SetupCatalogPanel()
        {
            var catalogFieldRow = rootVisualElement.Q("catalog-field-row");
            if (catalogFieldRow != null)
            {
                catalogField = new ObjectField("Catalog")
                {
                    objectType = typeof(UIViewCatalog),
                    allowSceneObjects = false
                };
                catalogField.style.flexGrow = 1f;
                catalogField.style.marginRight = 6f;
                catalogField.RegisterValueChangedCallback(evt => SetCatalog(evt.newValue as UIViewCatalog));

                var createButton = new Button(CreateCatalog) { text = "Create New" };
                createButton.AddToClassList("btn-secondary");

                catalogFieldRow.Add(catalogField);
                catalogFieldRow.Add(createButton);
            }

            var catalogActionRow = rootVisualElement.Q("catalog-action-row");
            if (catalogActionRow != null)
            {
                addEntryButton = new Button(AddEmptyEntry) { text = "+ Add Entry" };
                addEntryButton.AddToClassList("btn-primary");

                addSelectedButton = new Button(AddSelectedPrefabs) { text = "Import Selected Prefabs" };
                addSelectedButton.AddToClassList("btn-secondary");

                var validateButton = new Button(RefreshValidation) { text = "Validate" };
                validateButton.AddToClassList("btn-secondary");

                catalogActionRow.Add(addEntryButton);
                catalogActionRow.Add(addSelectedButton);
                catalogActionRow.Add(validateButton);
            }

            catalogStatusLabel = rootVisualElement.Q<Label>("label-catalog-status");
            entriesContainer = rootVisualElement.Q("entries-container");
            validationContainer = rootVisualElement.Q("validation-container");
        }

        private void SetupScenePanel()
        {
            var fieldsContainer = rootVisualElement.Q("scene-fields-container");
            if (fieldsContainer != null)
            {
                rootField = new ObjectField("UI Root")
                {
                    objectType = typeof(UIRoot),
                    allowSceneObjects = true
                };
                rootField.SetEnabled(false);
                rootField.style.marginBottom = 6f;

                bootstrapperField = new ObjectField("Bootstrapper")
                {
                    objectType = typeof(UIBootstrapper),
                    allowSceneObjects = true
                };
                bootstrapperField.SetEnabled(false);
                bootstrapperField.style.marginBottom = 6f;

                fieldsContainer.Add(rootField);
                fieldsContainer.Add(bootstrapperField);
            }

            var sceneActionRow = rootVisualElement.Q("scene-action-row");
            if (sceneActionRow != null)
            {
                var createRootButton = new Button(EnsureRootInScene) { text = "Create UI Root" };
                createRootButton.AddToClassList("btn-primary");

                createBootstrapperButton = new Button(EnsureBootstrapperInScene) { text = "Create Bootstrapper" };
                createBootstrapperButton.AddToClassList("btn-primary");

                assignCatalogButton = new Button(AssignCatalogToBootstrapper) { text = "Assign Catalog" };
                assignCatalogButton.AddToClassList("btn-secondary");

                sceneActionRow.Add(createRootButton);
                sceneActionRow.Add(createBootstrapperButton);
                sceneActionRow.Add(assignCatalogButton);
            }

            sceneStatusLabel = rootVisualElement.Q<Label>("label-scene-status");
        }

        private void CreateCatalog()
        {
            SetCatalog(AchEngineUIEditorUtility.CreateViewCatalogAsset());
        }

        private void AddEmptyEntry()
        {
            if (catalog == null) return;

            ModifyCatalog(() =>
            {
                var newIndex = entriesProperty.arraySize;
                entriesProperty.InsertArrayElementAtIndex(newIndex);
                InitializeEntry(entriesProperty.GetArrayElementAtIndex(newIndex), AchEngineUIEditorUtility.MakeUniqueId(catalog, "NewView"), null);
            });
        }

        private void AddSelectedPrefabs()
        {
            if (catalog == null) return;

            var count = AchEngineUIEditorUtility.AddSelectedViewPrefabs(catalog);
            catalogSerializedObject = new SerializedObject(catalog);
            entriesProperty = catalogSerializedObject.FindProperty("entries");
            RefreshEntries();
            RefreshValidation();
            ShowNotification(new GUIContent(count > 0 ? $"{count} prefab(s) added." : "Select one or more UIView prefabs first."));
        }

        private void SetCatalog(UIViewCatalog newCatalog)
        {
            catalog = newCatalog;
            catalogSerializedObject = catalog != null ? new SerializedObject(catalog) : null;
            entriesProperty = catalogSerializedObject?.FindProperty("entries");

            catalogField?.SetValueWithoutNotify(catalog);

            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
            RefreshSceneSection();
        }

        private void RefreshCatalogStatus()
        {
            if (catalogStatusLabel == null) return;

            if (catalog == null)
            {
                catalogStatusLabel.text = "Select a UIViewCatalog to begin editing.";
                SetCatalogActionState(false);
                return;
            }

            var count = catalog.Entries.Count;
            catalogStatusLabel.text = count == 1
                ? $"'{catalog.name}' - 1 entry"
                : $"'{catalog.name}' - {count} entries";
            SetCatalogActionState(true);
        }

        private void SetCatalogActionState(bool enabled)
        {
            addEntryButton?.SetEnabled(enabled);
            addSelectedButton?.SetEnabled(enabled);
            createBootstrapperButton?.SetEnabled(enabled);
            assignCatalogButton?.SetEnabled(enabled && AchEngineUIEditorUtility.FindBootstrapperInOpenScenes() != null);
        }

        private void RefreshEntries()
        {
            if (entriesContainer == null) return;
            entriesContainer.Clear();

            if (catalog == null || entriesProperty == null)
            {
                entriesContainer.Add(MakeHelpBox("Select a catalog to edit its entries.", HelpBoxMessageType.Info));
                return;
            }

            catalogSerializedObject.Update();
            entriesProperty = catalogSerializedObject.FindProperty("entries");

            if (entriesProperty.arraySize == 0)
            {
                entriesContainer.Add(MakeHelpBox("No entries yet. Click '+ Add Entry' or import selected prefabs.", HelpBoxMessageType.Info));
                return;
            }

            for (int i = 0; i < entriesProperty.arraySize; i++)
            {
                entriesContainer.Add(CreateEntryCard(i));
            }
        }

        private VisualElement CreateEntryCard(int index)
        {
            var entryProperty = entriesProperty.GetArrayElementAtIndex(index);
            var idProperty = entryProperty.FindPropertyRelative("id");
            var prefabProperty = entryProperty.FindPropertyRelative("prefab");
            var layerProperty = entryProperty.FindPropertyRelative("layer");
            var pooledProperty = entryProperty.FindPropertyRelative("pooled");
            var prewarmProperty = entryProperty.FindPropertyRelative("prewarmCount");
            var singleInstanceProperty = entryProperty.FindPropertyRelative("singleInstance");

            var card = new VisualElement();
            card.AddToClassList("entry-card");

            var header = new VisualElement();
            header.AddToClassList("entry-card-header");

            var title = new Label(string.IsNullOrWhiteSpace(idProperty.stringValue) ? $"Entry {index + 1}" : idProperty.stringValue);
            title.AddToClassList("entry-card-title");
            header.Add(title);

            var removeButton = new Button(() => RemoveEntry(index)) { text = "Remove" };
            removeButton.AddToClassList("btn-danger");
            header.Add(removeButton);
            card.Add(header);

            var idField = new TextField("View ID")
            {
                value = idProperty.stringValue,
                isDelayed = true
            };
            idField.AddToClassList("entry-field");
            idField.RegisterValueChangedCallback(evt => ModifyCatalog(() => idProperty.stringValue = evt.newValue));
            card.Add(idField);

            var prefabField = new ObjectField("Prefab")
            {
                objectType = typeof(UIView),
                allowSceneObjects = false,
                value = prefabProperty.objectReferenceValue
            };
            prefabField.AddToClassList("entry-field");
            prefabField.RegisterValueChangedCallback(evt => ModifyCatalog(() =>
            {
                prefabProperty.objectReferenceValue = evt.newValue;
                if (string.IsNullOrWhiteSpace(idProperty.stringValue) && evt.newValue != null)
                {
                    idProperty.stringValue = AchEngineUIEditorUtility.MakeUniqueId(catalog, evt.newValue.name);
                }
            }));
            card.Add(prefabField);

            var layerField = new EnumField("Layer", (UILayerId)layerProperty.intValue);
            layerField.AddToClassList("entry-field");
            layerField.RegisterValueChangedCallback(evt => ModifyCatalog(() => layerProperty.intValue = Convert.ToInt32(evt.newValue)));
            card.Add(layerField);

            var toggleRow = new VisualElement();
            toggleRow.AddToClassList("entry-toggle-row");

            var pooledToggle = new Toggle("Pooled") { value = pooledProperty.boolValue };
            pooledToggle.style.marginRight = 16f;
            pooledToggle.RegisterValueChangedCallback(evt => ModifyCatalog(() =>
            {
                pooledProperty.boolValue = evt.newValue;
                if (!evt.newValue)
                {
                    prewarmProperty.intValue = 0;
                }
            }));
            toggleRow.Add(pooledToggle);

            var singleInstanceToggle = new Toggle("Single Instance") { value = singleInstanceProperty.boolValue };
            singleInstanceToggle.RegisterValueChangedCallback(evt => ModifyCatalog(() => singleInstanceProperty.boolValue = evt.newValue));
            toggleRow.Add(singleInstanceToggle);
            card.Add(toggleRow);

            var prewarmField = new IntegerField("Prewarm Count")
            {
                value = prewarmProperty.intValue,
                isDelayed = true
            };
            prewarmField.AddToClassList("entry-field");
            prewarmField.SetEnabled(pooledProperty.boolValue);
            prewarmField.RegisterValueChangedCallback(evt => ModifyCatalog(() => prewarmProperty.intValue = Mathf.Max(0, evt.newValue)));
            card.Add(prewarmField);

            var warning = BuildEntryWarning(index, idProperty.stringValue, prefabProperty.objectReferenceValue as UIView, pooledProperty.boolValue, prewarmProperty.intValue);
            if (!string.IsNullOrEmpty(warning))
            {
                card.Add(MakeHelpBox(warning, HelpBoxMessageType.Warning));
            }

            return card;
        }

        private string BuildEntryWarning(int index, string id, UIView prefab, bool pooled, int prewarm)
        {
            if (string.IsNullOrWhiteSpace(id)) return $"Entry {index + 1} is missing a view ID.";
            if (prefab == null) return $"'{id}' does not have a prefab assigned.";
            if (!pooled && prewarm > 0) return $"'{id}' has a prewarm count but pooling is disabled.";
            return string.Empty;
        }

        private void RemoveEntry(int index)
        {
            if (catalog == null) return;
            ModifyCatalog(() => entriesProperty.DeleteArrayElementAtIndex(index));
        }

        private void ModifyCatalog(Action change)
        {
            if (catalog == null || catalogSerializedObject == null || entriesProperty == null) return;

            Undo.RecordObject(catalog, "Edit UI Catalog");
            catalogSerializedObject.Update();
            entriesProperty = catalogSerializedObject.FindProperty("entries");
            change();
            catalogSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(catalog);
            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
        }

        private void InitializeEntry(SerializedProperty entryProperty, string id, UIView prefab)
        {
            entryProperty.FindPropertyRelative("id").stringValue = id;
            entryProperty.FindPropertyRelative("prefab").objectReferenceValue = prefab;
            entryProperty.FindPropertyRelative("layer").intValue = (int)UILayerId.Screen;
            entryProperty.FindPropertyRelative("pooled").boolValue = true;
            entryProperty.FindPropertyRelative("prewarmCount").intValue = 0;
            entryProperty.FindPropertyRelative("singleInstance").boolValue = true;
        }

        private void RefreshValidation()
        {
            if (validationContainer == null) return;
            validationContainer.Clear();

            var issues = AchEngineUIEditorUtility.CollectCatalogIssues(catalog);
            if (issues.Count == 0)
            {
                var okLabel = new Label("No issues found");
                okLabel.AddToClassList("validation-ok");
                validationContainer.Add(okLabel);
                return;
            }

            foreach (var issue in issues)
            {
                var box = MakeHelpBox(issue.Message, ToHelpBoxMessageType(issue.MessageType));
                box.AddToClassList("validation-item");
                validationContainer.Add(box);
            }
        }

        private void EnsureRootInScene()
        {
            var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
            if (existingRoot == null)
            {
                AchEngineUIEditorUtility.CreateUIRoot();
                ShowNotification(new GUIContent("UI Root created."));
            }
            else
            {
                Selection.activeObject = existingRoot.gameObject;
                ShowNotification(new GUIContent("Using existing UI Root."));
            }

            RefreshSceneSection();
        }

        private void EnsureBootstrapperInScene()
        {
            if (catalog == null)
            {
                ShowNotification(new GUIContent("Select a catalog before creating a bootstrapper."));
                return;
            }

            var existingRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes() ?? AchEngineUIEditorUtility.CreateUIRoot();
            var existingBootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();

            if (existingBootstrapper == null)
            {
                AchEngineUIEditorUtility.CreateBootstrapper(catalog, existingRoot);
                ShowNotification(new GUIContent("Bootstrapper created."));
            }
            else
            {
                AchEngineUIEditorUtility.AssignBootstrapperReferences(existingBootstrapper, catalog, existingRoot);
                Selection.activeObject = existingBootstrapper.gameObject;
                ShowNotification(new GUIContent("Bootstrapper updated."));
            }

            RefreshSceneSection();
        }

        private void AssignCatalogToBootstrapper()
        {
            var bootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();
            if (bootstrapper == null || catalog == null) return;

            AchEngineUIEditorUtility.AssignBootstrapperReferences(bootstrapper, catalog, AchEngineUIEditorUtility.FindUIRootInOpenScenes());
            RefreshSceneSection();
            ShowNotification(new GUIContent("Catalog assigned to bootstrapper."));
        }

        private void RefreshSceneSection()
        {
            var sceneRoot = AchEngineUIEditorUtility.FindUIRootInOpenScenes();
            var bootstrapper = AchEngineUIEditorUtility.FindBootstrapperInOpenScenes();
            var assignedCatalog = AchEngineUIEditorUtility.GetAssignedCatalog(bootstrapper);
            var assignedRoot = AchEngineUIEditorUtility.GetAssignedRoot(bootstrapper);

            rootField?.SetValueWithoutNotify(sceneRoot);
            bootstrapperField?.SetValueWithoutNotify(bootstrapper);

            if (sceneStatusLabel != null)
            {
                if (sceneRoot == null)
                {
                    sceneStatusLabel.text = "UI Root not found.";
                }
                else if (bootstrapper == null)
                {
                    sceneStatusLabel.text = $"UI Root: {sceneRoot.name}. Bootstrapper not found.";
                }
                else if (assignedCatalog == null)
                {
                    sceneStatusLabel.text = $"UI Root: {sceneRoot.name}. Bootstrapper exists but no catalog is assigned.";
                }
                else
                {
                    var assignedRootName = assignedRoot != null ? assignedRoot.name : "None";
                    sceneStatusLabel.text = $"UI Root: {sceneRoot.name}. Catalog: {assignedCatalog.name}. Assigned Root: {assignedRootName}.";
                }
            }

            createBootstrapperButton?.SetEnabled(catalog != null);
            assignCatalogButton?.SetEnabled(catalog != null && bootstrapper != null);
        }

        private void HandleSelectionChanged()
        {
            if (Selection.activeObject is UIViewCatalog selectedCatalog && selectedCatalog != catalog)
            {
                SetCatalog(selectedCatalog);
            }
        }

        private void HandleUndoRedo()
        {
            catalogSerializedObject?.Update();
            RefreshCatalogStatus();
            RefreshEntries();
            RefreshValidation();
            RefreshSceneSection();
        }

        private void HandleHierarchyChanged()
        {
            RefreshSceneSection();
        }

        private static HelpBox MakeHelpBox(string text, HelpBoxMessageType type)
        {
            var box = new HelpBox(text, type);
            box.style.flexShrink = 0f;
            return box;
        }

        private static HelpBoxMessageType ToHelpBoxMessageType(MessageType messageType)
        {
            return messageType switch
            {
                MessageType.Error => HelpBoxMessageType.Error,
                MessageType.Warning => HelpBoxMessageType.Warning,
                _ => HelpBoxMessageType.Info,
            };
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
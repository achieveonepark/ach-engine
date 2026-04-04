using AchEngine.UI;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AchEngine.Editor.UI
{
    [CustomEditor(typeof(UIViewCatalog))]
    public sealed class UIViewCatalogInspector : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var helpBox = new HelpBox(
                "Use the AchEngine UI workspace for faster catalog editing, scene setup, and validation.",
                HelpBoxMessageType.Info);
            helpBox.style.marginBottom = 8f;
            root.Add(helpBox);

            var buttonRow = new VisualElement();
            buttonRow.style.flexDirection = FlexDirection.Row;
            buttonRow.style.marginBottom = 8f;

            var openWorkspaceButton = new Button(() => AchEngineUIWorkspaceWindow.Open(target as UIViewCatalog))
            {
                text = "Open Workspace"
            };
            openWorkspaceButton.style.marginRight = 8f;
            buttonRow.Add(openWorkspaceButton);

            var addSelectedPrefabsButton = new Button(() =>
            {
                var addedCount = AchEngineUIEditorUtility.AddSelectedViewPrefabs(target as UIViewCatalog);
                if (addedCount > 0)
                {
                    serializedObject.Update();
                    Repaint();
                }
            })
            {
                text = "Add Selected Prefabs"
            };
            buttonRow.Add(addSelectedPrefabsButton);
            root.Add(buttonRow);

            InspectorElement.FillDefaultInspector(root, serializedObject, this);
            return root;
        }
    }
}

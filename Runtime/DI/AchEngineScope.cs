#if ACHENGINE_VCONTAINER
using UnityEngine;
using VContainer;
using VContainer.Unity;
using AchEngine.Table;
using AchEngine.UI;

namespace AchEngine.DI
{
    /// <summary>
    /// VContainer LifetimeScope for the AchEngine.
    /// Registers ITableService, IUIService, and their dependencies.
    ///
    /// Usage:
    ///   1. Add this component to a GameObject in your scene.
    ///   2. Assign UIViewCatalog in the inspector.
    ///   3. All services are automatically resolved via VContainer.
    /// </summary>
    [DisallowMultipleComponent]
    public class AchEngineScope : LifetimeScope
    {
        [Header("UI")]
        [SerializeField] private UIViewCatalog catalog;
        [SerializeField] private UIRoot uiRoot;
        [SerializeField] private bool autoCreateRoot = true;
        [SerializeField] private bool prewarmOnStart = true;
        [SerializeField] private bool makePersistent = true;

        protected override void Configure(IContainerBuilder builder)
        {
            // Table
            builder.Register<TableDatabase>(Lifetime.Singleton).As<ITableDatabase>();
            builder.Register<TableService>(Lifetime.Singleton).As<ITableService>();

            // UI
            if (uiRoot == null && autoCreateRoot)
            {
                uiRoot = FindObjectOfType<UIRoot>();
            }

            if (uiRoot == null && autoCreateRoot)
            {
                uiRoot = UIRoot.CreateDefault();
            }

            if (catalog != null)
            {
                builder.RegisterInstance(catalog);
            }

            if (uiRoot != null)
            {
                builder.RegisterInstance(uiRoot);
            }

            builder.RegisterComponentInNewPrefab<UIService>(CreateUIServicePrefab(), Lifetime.Singleton)
                .DontDestroyOnLoad();
        }

        private UIService CreateUIServicePrefab()
        {
            var go = new GameObject("UIService (DI)", typeof(UIService));
            go.SetActive(false);
            return go.GetComponent<UIService>();
        }

        private void Start()
        {
            var uiService = Container.Resolve<UIService>();

            if (catalog != null && uiRoot != null)
            {
                uiService.Initialize(catalog, uiRoot);
                UI.UI.SetService(uiService);

                if (prewarmOnStart)
                {
                    uiService.Prewarm();
                }
            }

            // Register TableService to static accessor
            var tableService = Container.Resolve<ITableService>();
            TableManager.SetService(tableService);

            if (makePersistent)
            {
                DontDestroyOnLoad(gameObject);
                if (uiRoot != null)
                {
                    DontDestroyOnLoad(uiRoot.gameObject);
                }
            }
        }
    }
}
#endif

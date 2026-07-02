using UnityEngine;
using Zenject;
using Domain.MVP.Hub;
using Domain.MVP.Journal;
using Domain.MVP.Inventory;
using Domain.MVP.Market;
using Domain.MVP.Party;
using Domain.MVP.Settlement;
using Domain.MVP.Tab;

namespace Domain.MVP
{
    /// <summary>
    /// Binds all MVP tab models, views, and presenters.
    ///
    /// Each prefab is instantiated exactly once using the standard Zenject pattern:
    ///   1. Bind the first component with FromComponentInNewPrefab — this owns the instance.
    ///   2. Bind every other component on the same prefab with FromMethod, resolving
    ///      the first binding and calling GetComponent on its GameObject. This guarantees
    ///      zero extra instantiations and no resolve-during-install warnings because
    ///      FromMethod is deferred — it only runs when the binding is first needed,
    ///      after all InstallBindings calls have completed.
    /// </summary>
    public class MVPInstaller : MonoInstaller
    {
        [Header("Hub")]
        public GameObject hubPrefab;

        [Header("Tab Prefabs")]
        public GameObject journalTabPrefab;
        public GameObject inventoryTabPrefab;
        public GameObject marketTabPrefab;
        public GameObject partyTabPrefab;
        public GameObject settlementTabPrefab;

        [Header("Title Screen (optional)")]
        public GameObject titlePrefab;

        [Header("Runtime")]
        public bool showOnStart = true;

        public override void InstallBindings()
        {
            // ── Hub ──────────────────────────────────────────────────────────
            Container.Bind<HubModel>().AsSingle();

            // HubView owns the prefab instance.
            Container.Bind<HubView>()
                .FromComponentInNewPrefab(hubPrefab)
                .WithGameObjectName("Hub")
                .AsSingle()
                .NonLazy();

            // HubPresenter is retrieved from the same GameObject via the already-
            // resolved HubView — no second instantiation.
            Container.Bind<HubPresenter>()
                .FromMethod(ctx => ctx.Container
                    .Resolve<HubView>()
                    .GetComponent<HubPresenter>())
                .AsSingle()
                .NonLazy();

            // ── Tab models (pure C#) ─────────────────────────────────────────
            Container.Bind<JournalTabModel>().AsSingle();
            Container.Bind<InventoryTabModel>().AsSingle();
            Container.Bind<MarketTabModel>().AsSingle();
            Container.Bind<PartyTabModel>().AsSingle();
            Container.Bind<SettlementTabModel>().AsSingle();

            // ── Tabs ─────────────────────────────────────────────────────────
            // View owns the prefab; Presenter is fetched from the same instance.
            BindTab<JournalTabView,    JournalTabPresenter>   (journalTabPrefab,    "Tab_Journal");
            BindTab<InventoryTabView,  InventoryTabPresenter> (inventoryTabPrefab,  "Tab_Inventory");
            BindTab<MarketTabView,     MarketTabPresenter>    (marketTabPrefab,     "Tab_Market");
            BindTab<PartyTabView,      PartyTabPresenter>     (partyTabPrefab,      "Tab_Party");
            BindTab<SettlementTabView, SettlementTabPresenter>(settlementTabPrefab, "Tab_Settlement");

            // ── TabPresenter[] collection for HubPresenter ───────────────────
            // FromResolve reuses the singletons above — no new GameObjects.
            Container.Bind<TabPresenter>().To<JournalTabPresenter>().FromResolve();
            Container.Bind<TabPresenter>().To<InventoryTabPresenter>().FromResolve();
            Container.Bind<TabPresenter>().To<MarketTabPresenter>().FromResolve();
            Container.Bind<TabPresenter>().To<PartyTabPresenter>().FromResolve();
            Container.Bind<TabPresenter>().To<SettlementTabPresenter>().FromResolve();

            // ── Title screen (optional) ───────────────────────────────────────
            if (titlePrefab != null)
            {
                Container.Bind<TitleModel>().AsSingle();

                Container.Bind<TitleView>()
                    .FromComponentInNewPrefab(titlePrefab)
                    .WithGameObjectName("TitleScreen")
                    .AsSingle()
                    .NonLazy();

                Container.Bind<TitlePresenter>()
                    .FromMethod(ctx => ctx.Container
                        .Resolve<TitleView>()
                        .GetComponent<TitlePresenter>())
                    .AsSingle()
                    .NonLazy();
            }
        }

        /// <summary>
        /// Instantiate <paramref name="prefab"/> once (owned by the TView binding)
        /// and resolve TPresenter from the same GameObject via FromMethod.
        /// </summary>
        private void BindTab<TView, TPresenter>(GameObject prefab, string goName)
            where TView      : TabView
            where TPresenter : TabPresenter
        {
            Container.Bind<TView>()
                .FromComponentInNewPrefab(prefab)
                .WithGameObjectName(goName)
                .AsSingle()
                .NonLazy();

            Container.Bind<TPresenter>()
                .FromMethod(ctx => ctx.Container
                    .Resolve<TView>()
                    .GetComponent<TPresenter>())
                .AsSingle()
                .NonLazy();
        }
    }
}

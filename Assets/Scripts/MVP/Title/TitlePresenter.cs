using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

    public class TitlePresenter : MonoBehaviour
    {
        [Header("MVP Components")]
        public TitleView view;
        public TitleModel model;

        [Header("Presentation Settings")]
        public string gameSceneName = "GameScene";
        public int gameSceneBuildIndex = 1;
        public float settingsFadeOutDuration = 0.6f;
        public float settingsFadeInDuration = 0.8f;
        public float settingsDelay = 0.8f;

        private bool _isTransitioning;

        void Awake()
        {
            if (view == null)
                view = GetComponent<TitleView>();

            if (model == null)
                model = new TitleModel();

            BindEvents();
        }

        void OnDestroy()
        {
            UnbindEvents();
        }

        void Start()
        {
            view.SetContinueAvailable(model.ContinueAvailable);
            StartCoroutine(view.EnterSequence(0.3f, model.FadeDuration > 0f ? model.FadeDuration : 1.2f));
        }

        void BindEvents()
        {
            view.OnNewGameClick += OnNewGameClick;
            view.OnContinueClick += OnContinueClick;
            view.OnSettingsClick += OnSettingsClick;
            view.OnExitClick += OnExitClick;
        }

        void UnbindEvents()
        {
            view.OnNewGameClick -= OnNewGameClick;
            view.OnContinueClick -= OnContinueClick;
            view.OnSettingsClick -= OnSettingsClick;
            view.OnExitClick -= OnExitClick;
        }

        void Update()
        {
            if (model.IsFading)
            {
                model.UpdateFade(Time.unscaledDeltaTime);
                view.SetCanvasAlpha(model.GetFadeAlpha());
            }
        }

        // ==================== Button Handlers ====================

        void OnNewGameClick()
        {
            if (_isTransitioning || model.IsFading) return;

            StartCoroutine(FadeOutThen(() =>
            {
                model.StartGame();
                model.TriggerNewGame();
                view.LoadScene(GetTargetSceneIndex());
            }));
        }

        void OnContinueClick()
        {
            if (_isTransitioning || model.IsFading || !model.ContinueAvailable) return;

            StartCoroutine(FadeOutThen(() =>
            {
                model.TriggerContinue();
                view.LoadScene(GetTargetSceneIndex());
            }));
        }

        void OnSettingsClick()
        {
            if (_isTransitioning || model.IsFading) return;

            _isTransitioning = true;
            view.SetButtonsInteractable(false);

            StartCoroutine(FadeToZero(settingsFadeOutDuration, () =>
            {
                // TODO: Open settings panel
                // e.g., SettingsPanel.Instance.Open();
            }));
        }

        void OnExitClick()
        {
            if (_isTransitioning || model.IsFading) return;

            StartCoroutine(FadeOutThen(() =>
            {
                model.TriggerExit();
                view.ExitGame();
            }));
        }

        // ==================== Post-Fade Callbacks ====================

        void OnFadeInComplete()
        {
            _isTransitioning = false;
            view.SetButtonsInteractable(true);
        }

        void OnFadeOutComplete()
        {
            _isTransitioning = false;
            view.SetCanvasInteractable(false, false);
        }

        void OnSettingsBack()
        {
            StartCoroutine(FadeToOne(settingsFadeInDuration, () =>
            {
                view.SetButtonsInteractable(true);
            }));
        }

        // ==================== Helpers ====================

        IEnumerator FadeOutThen(System.Action onDone)
        {
            _isTransitioning = true;
            model.StartFade(FadeDirection.Out, 1.2f);
            yield return new WaitWhile(() => model.IsFading);
            onDone?.Invoke();
        }

        IEnumerator FadeToZero(float duration, System.Action onDone)
        {
            _isTransitioning = true;
            model.StartFade(FadeDirection.Out, duration);
            yield return new WaitWhile(() => model.IsFading);
            onDone?.Invoke();
            StartCoroutine(FadeToOne(settingsFadeInDuration, () =>
            {
                _isTransitioning = false;
                view.SetButtonsInteractable(true);
            }));
        }

        IEnumerator FadeToOne(float duration, System.Action onDone)
        {
            model.StartFade(FadeDirection.In, duration);
            yield return new WaitWhile(() => model.IsFading);
            view.SetCanvasAlpha(1f);
            onDone?.Invoke();
        }

        int GetTargetSceneIndex()
        {
            if (!string.IsNullOrEmpty(gameSceneName))
            {
                var idx = SceneManager.GetSceneByName(gameSceneName).buildIndex;
                if (idx >= 0) return idx;
            }
            return gameSceneBuildIndex;
        }
    }

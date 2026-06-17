using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace SoftKitty.MVP.Title
{
    public class TitleView : MonoBehaviour
    {
        [Header("Buttons")]
        public Button btnNewGame;
        public Button btnContinue;
        public Button btnSettings;
        public Button btnExit;

        [Header("Canvas Group")]
        public CanvasGroup canvasGroup;

        [Header("Fade Settings")]
        public float fadeDuration = 1.2f;

        [Header("Button Animation")]
        public float hoverScale = 1.08f;
        public float hoverDuration = 0.15f;
        public float pressScale = 0.95f;
        public float pressDuration = 0.08f;
        public float releaseDuration = 0.2f;
        public Color hoverTintColor = new Color(0.7f, 0.15f, 0.15f, 1f);
        public float hoverTintAmount = 0.18f;

        [Header("Surreal Atmosphere")]
        public float ambientSquashSpeed = 0.3f;
        public float ambientSquashAmount = 0.015f;
        public float ambientRotationSpeed = 0.5f;
        public float ambientRotationAmount = 0.2f;

        public event Action OnNewGameClick;
        public event Action OnContinueClick;
        public event Action OnSettingsClick;
        public event Action OnExitClick;

        private CanvasGroup _canvasGroup;
        private Image _backgroundImage;
        private Sequence _ambientSeq;
        private Tween[] _hoverTweens;
        private Tween[] _pressTweens;
        private Tween[] _releaseTweens;
        private Image[] _buttonImages;
        private Color[] _buttonBaseColors;
        private RectTransform[] _buttonRTs;

        public void Awake()
        {
            _canvasGroup = canvasGroup ?? GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _backgroundImage = GetComponent<Image>();

            var buttons = new[] { btnNewGame, btnContinue, btnSettings, btnExit };
            var count = 0;
            foreach (var btn in buttons)
            {
                if (btn != null) count++;
            }
            _buttonImages = new Image[count];
            _buttonBaseColors = new Color[count];
            _buttonRTs = new RectTransform[count];
            _hoverTweens = new Tween[count];
            _pressTweens = new Tween[count];
            _releaseTweens = new Tween[count];

            var idx = 0;
            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    var img = buttons[i].GetComponent<Image>();
                    _buttonImages[idx] = img;
                    _buttonBaseColors[idx] = img.color;
                    _buttonRTs[idx] = buttons[i].GetComponent<RectTransform>();
                    idx++;
                }
            }
        }

        void OnDestroy()
        {
            _ambientSeq?.Kill();
            foreach (var t in _hoverTweens) t?.Kill();
            foreach (var t in _pressTweens) t?.Kill();
            foreach (var t in _releaseTweens) t?.Kill();
        }

        void Start()
        {
            SetupButtonEvents();
            StartAmbientAnimation();
            StartCoroutine(EnterSequence(0f, 0.5f));
        }

        void SetupButtonEvents()
        {
            if (btnNewGame != null)
            {
                btnNewGame.onClick.AddListener(() => OnNewGameClick?.Invoke());
                SetupButtonHover(btnNewGame, 0);
            }
            if (btnContinue != null)
            {
                btnContinue.onClick.AddListener(() => OnContinueClick?.Invoke());
                SetupButtonHover(btnContinue, 1);
            }
            if (btnSettings != null)
            {
                btnSettings.onClick.AddListener(() => OnSettingsClick?.Invoke());
                SetupButtonHover(btnSettings, 2);
            }
            if (btnExit != null)
            {
                btnExit.onClick.AddListener(() => OnExitClick?.Invoke());
                SetupButtonHover(btnExit, 3);
            }
        }

        void SetupButtonHover(Button btn, int idx)
        {
            var img = _buttonImages[idx];
            var baseColor = _buttonBaseColors[idx];

// btn.OnPointerEnter(_ => OnHoverEnter(img, baseColor, idx));
//             btn.OnPointerExit.AddListener((_) => OnHoverExit(img, baseColor, idx));
//             btn.OnPointerDown.AddListener((_) => OnPress(img, baseColor, idx));
//             btn.OnPointerUp.AddListener((_) => OnRelease(img, baseColor, idx));
        }

        void OnHoverEnter(Image img, Color baseColor, int idx)
        {
            _hoverTweens[idx]?.Kill();
            _pressTweens[idx]?.Kill();
            _releaseTweens[idx]?.Kill();

            img.rectTransform.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutQuad).SetTarget(img);
            img.DOColor(baseColor + hoverTintColor * hoverTintAmount, hoverDuration).SetEase(Ease.InOutQuad).SetTarget(img);
        }

        void OnHoverExit(Image img, Color baseColor, int idx)
        {
            _hoverTweens[idx]?.Kill();
            _pressTweens[idx]?.Kill();
            _releaseTweens[idx]?.Kill();

            img.rectTransform.DOScale(Vector3.one, releaseDuration).SetEase(Ease.OutBack).SetTarget(img);
            img.DOColor(baseColor, releaseDuration).SetEase(Ease.OutQuad).SetTarget(img);
        }

        void OnPress(Image img, Color baseColor, int idx)
        {
            _hoverTweens[idx]?.Kill();
            _releaseTweens[idx]?.Kill();

            img.rectTransform.DOScale(pressScale, pressDuration).SetEase(Ease.InOutQuad).SetTarget(img);
            img.DOFade(0.7f, pressDuration).SetTarget(img);
        }

        void OnRelease(Image img, Color baseColor, int idx)
        {
            _releaseTweens[idx] = img.DOColor(baseColor, releaseDuration)
                .SetEase(Ease.OutQuad).SetTarget(img);
            img.rectTransform.DOScale(Vector3.one, releaseDuration)
                .SetEase(Ease.OutBack).SetTarget(img);
        }

        void StartAmbientAnimation()
        {
            if (_backgroundImage == null) return;

            var rt = GetComponent<RectTransform>();

            _ambientSeq = DOTween.Sequence();
            var squashTween = _backgroundImage.rectTransform.DOScale(
                new Vector3(1f + ambientSquashAmount, 1f - ambientSquashAmount, 1f),
                ambientSquashSpeed / 2f
            ).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            _ambientSeq.Append(squashTween);
            _ambientSeq.AppendInterval(ambientSquashSpeed / 2f);

            var rotTween = rt.DORotate(
                new Vector3(0, 0, ambientRotationAmount),
                ambientRotationSpeed / 2f
            ).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            _ambientSeq.Append(rotTween);
            _ambientSeq.AppendInterval(ambientRotationSpeed / 2f);
        }

        public void SetContinueAvailable(bool available)
        {
            if (btnContinue != null)
                btnContinue.interactable = available;
        }

        public void SetButtonsInteractable(bool value)
        {
            var buttons = new[] { btnNewGame, btnContinue, btnSettings, btnExit };
            foreach (var btn in buttons)
            {
                if (btn != null) btn.interactable = value;
            }
        }

        public void SetCanvasAlpha(float alpha)
        {
            _canvasGroup.alpha = alpha;
        }

        public void SetCanvasInteractable(bool interactable, bool blocksRaycasts)
        {
            _canvasGroup.interactable = interactable;
            _canvasGroup.blocksRaycasts = blocksRaycasts;
        }

        public void ShowButton(Button btn, bool show)
        {
            if (btn != null) btn.gameObject.SetActive(show);
        }

        public void AnimateButtonAppear(Button btn, int index)
        {
            if (btn == null) return;
            btn.gameObject.SetActive(true);

            var img = btn.GetComponent<Image>();
            btn.transform.localScale = Vector3.one * 0.85f;
            btn.transform.DOScale(Vector3.one, 0.4f).SetDelay(0.05f);
            img.DOFade(1f, 0.4f).SetDelay(0.05f);
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void LoadScene(int buildIndex)
        {
            SceneManager.LoadScene(buildIndex);
        }

        // ==================== Coroutine Helpers ====================

        public System.Collections.IEnumerator EnterSequence(float fadeDelay, float fadeInDuration)
        {
            yield return new WaitForSeconds(fadeDelay);
            yield return StartCoroutine(FadeTo(1f, fadeInDuration));
            SetCanvasInteractable(true, true);

            var buttons = new[] { btnNewGame, btnContinue, btnSettings, btnExit };
            foreach (var btn in buttons)
            {
                AnimateButtonAppear(btn, 0);
                yield return new WaitForSeconds(0.12f);
            }
        }

        public System.Collections.IEnumerator FadeTo(float targetAlpha, float duration)
        {
            SetCanvasInteractable(false, false);

            float t = 0f;
            float startAlpha = _canvasGroup.alpha;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            _canvasGroup.alpha = targetAlpha;
            SetCanvasInteractable(targetAlpha > 0f, targetAlpha > 0f);
        }
    }
}

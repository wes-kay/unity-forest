using System;
using System.IO;
using UnityEngine;

namespace SoftKitty.MVP.Title
{
    public enum FadeDirection { In, Out }

    public class TitleModel
    {
        public bool IsFading { get; private set; }
        public FadeDirection CurrentFade { get; private set; }
        public float FadeProgress { get; private set; }
        public float FadeDuration { get; private set; }
        public bool ContinueAvailable { get; private set; }
        public bool IsGameActive { get; private set; }

        public event Action<bool> OnFadeComplete;
        public event Action OnNewGameRequested;
        public event Action OnContinueRequested;
        public event Action OnSettingsRequested;
        public event Action OnExitRequested;

        private readonly string _savePath;

        public TitleModel(string savePath = null)
        {
            _savePath = savePath;
            CheckContinueAvailability();
        }

        public void CheckContinueAvailability()
        {
            if (!string.IsNullOrEmpty(_savePath))
                ContinueAvailable = File.Exists(_savePath);
        }

        public void StartFade(FadeDirection direction, float duration)
        {
            IsFading = true;
            CurrentFade = direction;
            FadeProgress = 0f;
            FadeDuration = duration;
        }

        public void StopFade()
        {
            IsFading = false;
            FadeProgress = 0f;
        }

        public void UpdateFade(float deltaTime)
        {
            if (!IsFading) return;

            FadeProgress += deltaTime / FadeDuration;

            if (FadeProgress >= 1f)
            {
                FadeProgress = 1f;
                IsFading = false;
                OnFadeComplete?.Invoke(CurrentFade == FadeDirection.In);
            }
        }

        public float GetFadeAlpha()
        {
            var target = CurrentFade == FadeDirection.In ? 1f : 0f;
            var start = CurrentFade == FadeDirection.In ? 0f : 1f;
            return Mathf.Lerp(start, target, FadeProgress);
        }

        public void TriggerNewGame() => OnNewGameRequested?.Invoke();
        public void TriggerContinue() => OnContinueRequested?.Invoke();
        public void TriggerSettings() => OnSettingsRequested?.Invoke();
        public void TriggerExit() => OnExitRequested?.Invoke();

        public void StartGame() => IsGameActive = true;
    }
}

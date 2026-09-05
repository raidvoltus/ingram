using UnityEngine;
using Genevore.AI;

namespace Genevore.Stability
{
    public class ThermalAdaptiveSystem : MonoBehaviour
    {
        public enum QualityTier { High = 0, Medium = 1, Low = 2, Critical = 3 }
        [SerializeField] private int targetFrameRate = 30;
        [SerializeField] private float sampleWindowSeconds = 10f;
        [SerializeField] private float degradeFpsThreshold = 26f;
        [SerializeField] private float recoverFpsThreshold = 29f;
        [SerializeField] private int consecutiveWindowsToDegrade = 1;
        [SerializeField] private int consecutiveWindowsToRecover = 3;
        [SerializeField] private AbstractAISimulator abstractAI;
        [SerializeField] private float aiRadiusHigh = 50f, aiRadiusMedium = 35f, aiRadiusLow = 22f, aiRadiusCritical = 12f;
        [SerializeField] private float shadowDistanceHigh = 40f, shadowDistanceMedium = 25f, shadowDistanceLow = 12f, shadowDistanceCritical = 0f;
        private float _windowAccum; private int _windowFrames; private float _lastWindowAvgFps = 30f;
        private int _badWindows, _goodWindows; private QualityTier _tier = QualityTier.High;
        public float LastWindowAvgFps => _lastWindowAvgFps;
        public QualityTier CurrentTier => _tier;
        public float SessionMinFps { get; private set; } = 999f;
        public float SessionMaxFps { get; private set; }
        public int DegradeEvents { get; private set; }
        private void Awake() { ApplyFrameCap(); if (abstractAI == null) abstractAI = FindObjectOfType<AbstractAISimulator>(); }
        private void OnEnable() { ApplyFrameCap(); ApplyTier(_tier, true); }
        private void ApplyFrameCap() { Application.targetFrameRate = targetFrameRate; QualitySettings.vSyncCount = 0; }
        private void Update()
        {
            float dt = Time.unscaledDeltaTime; if (dt <= 0f) return;
            float fps = 1f / dt; if (fps < SessionMinFps) SessionMinFps = fps; if (fps > SessionMaxFps) SessionMaxFps = fps;
            _windowAccum += dt; _windowFrames++;
            if (_windowAccum >= sampleWindowSeconds)
            { float avg = _windowFrames / _windowAccum; _lastWindowAvgFps = avg; EvaluateWindow(avg); _windowAccum = 0f; _windowFrames = 0; }
        }
        private void EvaluateWindow(float avgFps)
        {
            if (avgFps < degradeFpsThreshold) { _badWindows++; _goodWindows = 0; if (_badWindows >= consecutiveWindowsToDegrade) { _badWindows = 0; Downgrade(); } }
            else if (avgFps >= recoverFpsThreshold) { _goodWindows++; _badWindows = 0; if (_goodWindows >= consecutiveWindowsToRecover) { _goodWindows = 0; Upgrade(); } }
            else { _badWindows = 0; _goodWindows = 0; }
        }
        private void Downgrade() { if (_tier >= QualityTier.Critical) return; _tier = (QualityTier)((int)_tier + 1); DegradeEvents++; ApplyTier(_tier, true); Debug.LogWarning($"[ThermalAdaptive] DEGRADE → {_tier}"); }
        private void Upgrade() { if (_tier <= QualityTier.High) return; _tier = (QualityTier)((int)_tier - 1); ApplyTier(_tier, true); }
        private void ApplyTier(QualityTier tier, bool force)
        {
            ApplyFrameCap();
            float shadowDist, aiRadius; int q;
            switch (tier) {
                case QualityTier.High: shadowDist = shadowDistanceHigh; aiRadius = aiRadiusHigh; q = Mathf.Min(QualitySettings.names.Length - 1, 2); break;
                case QualityTier.Medium: shadowDist = shadowDistanceMedium; aiRadius = aiRadiusMedium; q = Mathf.Min(QualitySettings.names.Length - 1, 1); break;
                case QualityTier.Low: shadowDist = shadowDistanceLow; aiRadius = aiRadiusLow; q = 0; break;
                default: shadowDist = shadowDistanceCritical; aiRadius = aiRadiusCritical; q = 0; break;
            }
            if (QualitySettings.names != null && QualitySettings.names.Length > 0) QualitySettings.SetQualityLevel(Mathf.Clamp(q, 0, QualitySettings.names.Length - 1), force);
            QualitySettings.shadowDistance = shadowDist;
            if (abstractAI != null) abstractAI.SetMaterialiseRadius(aiRadius);
        }
        public void ForceTier(QualityTier tier) { _tier = tier; ApplyTier(tier, true); }
        public void ResetTelemetry() { SessionMinFps = 999f; SessionMaxFps = 0f; DegradeEvents = 0; _badWindows = 0; _goodWindows = 0; _windowAccum = 0f; _windowFrames = 0; }
    }
}

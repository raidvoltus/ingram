using UnityEngine;
using Genevore.Core;
using Genevore.Player;
using Genevore.Stability;
using Genevore.Systems;

namespace Genevore.QA
{
    public class EnduranceTestRunner : MonoBehaviour
    {
        public enum Phase { Idle, Running, Finished }
        [SerializeField] private float totalDurationSeconds = 45f * 60f;
        [SerializeField] private float telemetryMinuteA = 5f, telemetryMinuteB = 35f;
        [SerializeField] private float moveChangeInterval = 4f, devourInterval = 2.5f, geneSwapInterval = 8f, moveInputMagnitude = 0.85f;
        [SerializeField] private MobilePlayerController player;
        [SerializeField] private DevourController devour;
        [SerializeField] private GenomeManager genome;
        [SerializeField] private ThermalAdaptiveSystem thermal;
        [SerializeField] private AppLifecycleHandler lifecycle;
        [SerializeField] private bool runOnStart = false;
        private Phase _phase; private float _elapsed, _moveTimer, _devourTimer, _geneTimer;
        private Vector2 _currentInput;
        private float _fpsAtMin5 = -1f, _fpsAtMin35 = -1f, _minFpsAtMin5 = -1f, _minFpsAtMin35 = -1f;
        private long _memAtMin5, _memAtMin35; private bool _captured5, _captured35;
        public Phase CurrentPhase => _phase;
        public float FpsAtMinute5 => _fpsAtMin5;
        public float FpsAtMinute35 => _fpsAtMin35;
        public float SessionMinFps => thermal != null ? thermal.SessionMinFps : -1f;
        private void Start() { AutoWire(); if (runOnStart) Begin(); }
        private void AutoWire()
        {
            if (player == null) player = FindObjectOfType<MobilePlayerController>();
            if (devour == null) devour = FindObjectOfType<DevourController>();
            if (genome == null) genome = FindObjectOfType<GenomeManager>();
            if (thermal == null) thermal = FindObjectOfType<ThermalAdaptiveSystem>();
            if (lifecycle == null) lifecycle = FindObjectOfType<AppLifecycleHandler>();
        }
        [ContextMenu("Begin Endurance Test")]
        public void Begin()
        {
            AutoWire(); _phase = Phase.Running; _elapsed = 0f; _captured5 = _captured35 = false;
            if (thermal != null) thermal.ResetTelemetry();
            Application.targetFrameRate = 30;
            PickNewMoveDirection();
            Debug.Log("[Endurance] START 45 min @ 30 FPS cap");
        }
        private void Update()
        {
            if (_phase != Phase.Running) return;
            if (lifecycle != null && lifecycle.IsPaused) return;
            float dt = Time.unscaledDeltaTime; _elapsed += dt;
            _moveTimer += dt; if (_moveTimer >= moveChangeInterval) { _moveTimer = 0f; PickNewMoveDirection(); }
            if (player != null) player.SetJoystickInput(_currentInput);
            _devourTimer += dt; if (_devourTimer >= devourInterval) { _devourTimer = 0f; if (devour != null) devour.ForceDevourCycle(); else if (genome != null) genome.ApplyRandomMutation(); }
            _geneTimer += dt; if (_geneTimer >= geneSwapInterval) { _geneTimer = 0f; if (genome != null) { if (genome.GeneCount > 0) genome.UnequipGeneAt(Random.Range(0, genome.GeneCount)); genome.ApplyRandomMutation(); } }
            CaptureTelemetry();
            if (_elapsed >= totalDurationSeconds) Finish();
        }
        private void PickNewMoveDirection() { float a = Random.Range(0f, Mathf.PI * 2f); _currentInput = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * moveInputMagnitude; }
        private void CaptureTelemetry()
        {
            float m = _elapsed / 60f;
            if (!_captured5 && m >= telemetryMinuteA) { _captured5 = true; _fpsAtMin5 = thermal != null ? thermal.LastWindowAvgFps : 30f; _minFpsAtMin5 = thermal != null ? thermal.SessionMinFps : 30f; _memAtMin5 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(); Debug.Log($"[Endurance] t=5 avg={_fpsAtMin5:F1} min={_minFpsAtMin5:F1} memMB={_memAtMin5/1048576f:F1}"); }
            if (!_captured35 && m >= telemetryMinuteB) { _captured35 = true; _fpsAtMin35 = thermal != null ? thermal.LastWindowAvgFps : 30f; _minFpsAtMin35 = thermal != null ? thermal.SessionMinFps : 30f; _memAtMin35 = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(); LogThermalComparison(); }
        }
        private void LogThermalComparison()
        {
            Debug.Log($"THERMAL: min5 avg={_fpsAtMin5:F1} min={_minFpsAtMin5:F1} | min35 avg={_fpsAtMin35:F1} min={_minFpsAtMin35:F1}");
            long d = _memAtMin35 - _memAtMin5;
            Debug.Log($"\u0394RAM={(d/1048576f):F2}MB");
            if (d > 15L * 1048576L) Debug.LogWarning("[Endurance] POSSIBLE SLOW LEAK >15MB");
            if (_minFpsAtMin35 > 0 && _minFpsAtMin35 < 25f) Debug.LogError($"FAIL 1% low {_minFpsAtMin35:F1} < 25");
        }
        private void Finish() { _phase = Phase.Finished; if (player != null) player.SetJoystickInput(Vector2.zero); Debug.Log($"[Endurance] DONE sessionMin={SessionMinFps:F1}"); if (_captured5 && _captured35) LogThermalComparison(); }
    }
}

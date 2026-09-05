using UnityEngine;
using Genevore.Core;
using Genevore.Combat;
using Genevore.Player;

namespace Genevore.Systems
{
    public class BiomassMetabolism : MonoBehaviour
    {
        [SerializeField] private GenomeManager genome;
        [SerializeField] private CreatureAssembly assembly;
        [SerializeField] private DamageableEntity damageable;
        [SerializeField] private MobilePlayerController playerController;
        [SerializeField] private ProceduralScaleAdapter scaleAdapter;
        [SerializeField] private float baseBiomass = 1f;
        [SerializeField] private float biomassPerModule = 0.85f;
        [SerializeField] private float biomassPerScaleUnit = 0.6f;
        [SerializeField] private float baseDrainPerSecond = 0.35f;
        [SerializeField] private float decayExponent = 1.15f;
        [SerializeField] private float maxDrainMultiplier = 12f;
        [SerializeField] private float minBiomassClamp = 0.5f;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float minMoveSpeed = 1.8f;
        [SerializeField] private float speedPenaltyK = 0.22f;
        [SerializeField] private float apexIdleSurvivalSeconds = 180f;

        private float _currentBiomass = 1f;
        private float _drainPerSecond;
        private float _cachedMoveSpeed;

        public float CurrentBiomass => _currentBiomass;
        public float DrainPerSecond => _drainPerSecond;
        public float CurrentMoveSpeed => _cachedMoveSpeed;
        public float EstimatedIdleSurvivalSeconds =>
            (damageable == null || _drainPerSecond <= 0.0001f) ? float.MaxValue : damageable.CurrentHP / _drainPerSecond;

        private void Awake()
        {
            if (genome == null) genome = GetComponent<GenomeManager>();
            if (assembly == null) assembly = GetComponent<CreatureAssembly>();
            if (damageable == null) damageable = GetComponent<DamageableEntity>();
            if (playerController == null) playerController = GetComponent<MobilePlayerController>();
            if (scaleAdapter == null) scaleAdapter = GetComponent<ProceduralScaleAdapter>();
            if (genome != null)
            {
                genome.OnStatsRecalculated += RecalculateBiomass;
                genome.OnGeneEquipped += (_, __) => RecalculateBiomass();
                genome.OnGeneRemoved += _ => RecalculateBiomass();
            }
        }

        private void OnDestroy() { if (genome != null) genome.OnStatsRecalculated -= RecalculateBiomass; }
        private void Start() => RecalculateBiomass();

        private void Update()
        {
            if (damageable == null || !damageable.IsAlive) return;
            if (_drainPerSecond > 0f) damageable.TakeDamage(_drainPerSecond * Time.deltaTime, -1);
        }

        public void RecalculateBiomass()
        {
            int modules = genome != null ? genome.GeneCount : 0;
            float scaleFactor = scaleAdapter != null ? scaleAdapter.CurrentUniformScale : transform.localScale.x;
            _currentBiomass = Mathf.Max(minBiomassClamp,
                baseBiomass + modules * biomassPerModule + Mathf.Max(0f, scaleFactor - 1f) * biomassPerScaleUnit);
            _drainPerSecond = EvaluateDrain(_currentBiomass);
            _cachedMoveSpeed = EvaluateMoveSpeed(_currentBiomass);
            if (playerController is IMetabolismSpeedReceiver r) r.SetMetabolismMoveSpeed(_cachedMoveSpeed);
        }

        public float EvaluateDrain(float biomass)
        {
            float b = Mathf.Max(minBiomassClamp, biomass);
            float excess = Mathf.Max(0f, b - 1f);
            float arg = Mathf.Min(decayExponent * excess * 0.15f, 8f);
            float mult = Mathf.Min(Mathf.Exp(arg), maxDrainMultiplier);
            return Mathf.Max(0f, baseDrainPerSecond * mult);
        }

        public float EvaluateMoveSpeed(float biomass)
        {
            float excess = Mathf.Max(0f, Mathf.Max(minBiomassClamp, biomass) - 1f);
            return Mathf.Max(minMoveSpeed, baseMoveSpeed / (1f + speedPenaltyK * excess));
        }

        public bool ValidateApexSurvival(float apexBiomass, float fullHP)
        {
            float drain = EvaluateDrain(apexBiomass);
            if (drain <= 0f) return false;
            return fullHP / drain <= apexIdleSurvivalSeconds + 0.5f;
        }
    }

    public interface IMetabolismSpeedReceiver { void SetMetabolismMoveSpeed(float speed); }
}

using UnityEngine;
using Genevore.Core;

namespace Genevore.Systems
{
    [RequireComponent(typeof(CharacterController))]
    public class ProceduralScaleAdapter : MonoBehaviour
    {
        [SerializeField] private GenomeManager genome;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float baseHeight = 2f;
        [SerializeField] private float baseRadius = 0.4f;
        [SerializeField] private float baseCenterY = 1f;
        [SerializeField] private float scalePerModule = 0.08f;
        [SerializeField] private float minScale = 0.85f;
        [SerializeField] private float maxScale = 3.5f;
        [SerializeField] private float scaleLerpSpeed = 6f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundProbeUp = 2f;
        [SerializeField] private float groundProbeDown = 10f;
        [SerializeField] private float skinWidthMargin = 0.05f;

        private float _targetScale = 1f;
        private float _currentScale = 1f;
        private static readonly Collider[] PenetrationBuffer = new Collider[8];

        public float CurrentUniformScale => _currentScale;

        private void Awake()
        {
            if (characterController == null) characterController = GetComponent<CharacterController>();
            if (genome == null) genome = GetComponent<GenomeManager>();
            if (visualRoot == null) visualRoot = transform;
            if (genome != null)
            {
                genome.OnGeneEquipped += OnGeneChanged;
                genome.OnGeneRemoved += OnGeneRemoved;
                genome.OnStatsRecalculated += RecalculateTargetScale;
            }
        }

        private void OnDestroy()
        {
            if (genome != null)
            {
                genome.OnGeneEquipped -= OnGeneChanged;
                genome.OnGeneRemoved -= OnGeneRemoved;
                genome.OnStatsRecalculated -= RecalculateTargetScale;
            }
        }

        private void Start() { RecalculateTargetScale(); ApplyScaleImmediate(_targetScale); }

        private void Update()
        {
            if (Mathf.Abs(_currentScale - _targetScale) > 0.0005f)
            {
                _currentScale = Mathf.Lerp(_currentScale, _targetScale, Time.deltaTime * scaleLerpSpeed);
                ApplyScale(_currentScale);
                CorrectGroundPenetration();
            }
        }

        private void OnGeneChanged(int slot, Genevore.Data.GeneDataSO gene) => RecalculateTargetScale();
        private void OnGeneRemoved(int slot) => RecalculateTargetScale();

        public void RecalculateTargetScale()
        {
            int count = genome != null ? genome.GeneCount : 0;
            _targetScale = Mathf.Clamp(1f + count * scalePerModule, minScale, maxScale);
        }

        public void ApplyScaleImmediate(float scale)
        {
            _currentScale = _targetScale = Mathf.Clamp(scale, minScale, maxScale);
            ApplyScale(_currentScale);
            CorrectGroundPenetration();
        }

        private void ApplyScale(float scale)
        {
            if (visualRoot != null) visualRoot.localScale = new Vector3(scale, scale, scale);
            if (characterController != null)
            {
                bool was = characterController.enabled;
                characterController.enabled = false;
                characterController.height = baseHeight * scale;
                characterController.radius = baseRadius * scale;
                characterController.center = new Vector3(0f, baseCenterY * scale, 0f);
                characterController.enabled = was;
            }
        }

        private void CorrectGroundPenetration()
        {
            if (characterController == null) return;
            Vector3 origin = transform.position + Vector3.up * groundProbeUp;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundProbeUp + groundProbeDown, groundMask, QueryTriggerInteraction.Ignore))
            {
                float feetY = hit.point.y + skinWidthMargin;
                float bottomOffset = characterController.center.y - characterController.height * 0.5f;
                Vector3 pos = transform.position;
                if (pos.y < feetY - bottomOffset) { pos.y = feetY - bottomOffset; transform.position = pos; }
            }
            ResolveLateralPenetration();
        }

        private void ResolveLateralPenetration()
        {
            if (characterController == null) return;
            int count = Physics.OverlapSphereNonAlloc(transform.position + characterController.center,
                characterController.radius + 0.1f, PenetrationBuffer, groundMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var other = PenetrationBuffer[i];
                if (other == null || other.transform == transform) continue;
                if (Physics.ComputePenetration(characterController, transform.position, transform.rotation,
                        other, other.transform.position, other.transform.rotation, out Vector3 dir, out float dist))
                {
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.0001f) transform.position += dir.normalized * dist;
                }
            }
        }

        public void DebugForceScaleSteps(int steps)
        {
            for (int i = 0; i < steps; i++)
                ApplyScaleImmediate(Mathf.Lerp(minScale, maxScale, (i + 1) / (float)steps));
        }
    }
}

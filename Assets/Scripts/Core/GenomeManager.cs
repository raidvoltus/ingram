using UnityEngine;
using Genevore.Data;

namespace Genevore.Core
{
    public class GenomeManager : MonoBehaviour
    {
        public const int MaxGeneSlots = 6;
        [SerializeField] private GeneDataSO[] availableGenes;
        [SerializeField] private BodyModuleSO[] availableModules;
        private readonly GeneDataSO[] _activeGenes = new GeneDataSO[MaxGeneSlots];
        private int _geneCount;
        private StatBlock _cachedTotalStats;
        private bool _statsDirty = true;

        public event System.Action OnStatsRecalculated;
        public event System.Action<int, GeneDataSO> OnGeneEquipped;
        public event System.Action<int> OnGeneRemoved;

        public StatBlock TotalStats
        {
            get { if (_statsDirty) RecalculateStats(); return _cachedTotalStats; }
        }
        public int GeneCount => _geneCount;

        public bool EquipGene(int geneId)
        {
            bool success = TryAddGeneInternal(geneId);
            if (success)
            {
                int slot = _geneCount - 1;
                OnGeneEquipped?.Invoke(slot, _activeGenes[slot]);
                NotifyStatsChanged();
            }
            return success;
        }

        public bool UnequipGeneAt(int slot)
        {
            bool success = RemoveGeneAtInternal(slot);
            if (success) { OnGeneRemoved?.Invoke(slot); NotifyStatsChanged(); }
            return success;
        }

        public bool TryAddGene(int geneId) => EquipGene(geneId);
        public bool RemoveGeneAt(int slot) => UnequipGeneAt(slot);

        private bool TryAddGeneInternal(int geneId)
        {
            if (_geneCount >= MaxGeneSlots || availableGenes == null) return false;
            GeneDataSO gene = null;
            for (int i = 0; i < availableGenes.Length; i++)
                if (availableGenes[i] != null && availableGenes[i].GeneId == geneId) { gene = availableGenes[i]; break; }
            if (gene == null) return false;
            _activeGenes[_geneCount++] = gene;
            _statsDirty = true;
            return true;
        }

        private bool RemoveGeneAtInternal(int slot)
        {
            if (slot < 0 || slot >= _geneCount) return false;
            for (int i = slot; i < _geneCount - 1; i++) _activeGenes[i] = _activeGenes[i + 1];
            _activeGenes[--_geneCount] = null;
            _statsDirty = true;
            return true;
        }

        public void ClearAllGenes()
        {
            for (int i = 0; i < MaxGeneSlots; i++) _activeGenes[i] = null;
            _geneCount = 0; _statsDirty = true; NotifyStatsChanged();
        }

        private void RecalculateStats()
        {
            _cachedTotalStats.Reset();
            for (int i = 0; i < _geneCount; i++)
                if (_activeGenes[i] != null) _cachedTotalStats.Add(_activeGenes[i].StatModifiers);
            _statsDirty = false;
        }

        private void NotifyStatsChanged() { var _ = TotalStats; OnStatsRecalculated?.Invoke(); }

        public GeneDataSO GetGeneAt(int slot) => (slot < 0 || slot >= _geneCount) ? null : _activeGenes[slot];

        public int GetModuleHashForGene(int geneId)
        {
            if (availableGenes == null) return 0;
            for (int i = 0; i < availableGenes.Length; i++)
                if (availableGenes[i] != null && availableGenes[i].GeneId == geneId)
                    return availableGenes[i].ModulePrefabHash;
            return 0;
        }

        public bool ApplyRandomMutation()
        {
            if (availableGenes == null || availableGenes.Length == 0 || _geneCount >= MaxGeneSlots) return false;
            var gene = availableGenes[Random.Range(0, availableGenes.Length)];
            return gene != null && EquipGene(gene.GeneId);
        }
    }
}

using UnityEngine;
using Genevore.Data;
using Genevore.Core;

namespace Genevore.Combat
{
    public class DamageableEntity : MonoBehaviour, IDamageable
    {
        [SerializeField] private float baseMaxHP = 100f;
        [SerializeField] private GenomeManager genome;

        private float _currentHP;
        private float _maxHP;
        private int _entityId;
        private static int _nextId = 1;

        public bool IsAlive => _currentHP > 0f;
        public float CurrentHP => _currentHP;
        public float MaxHP => _maxHP;
        public int EntityId => _entityId;

        public event System.Action<float, float> OnHealthChanged;
        public event System.Action OnDeath;

        private void Awake()
        {
            _entityId = _nextId++;
            RecalculateMaxHP();
            _currentHP = _maxHP;
        }

        public void BindGenome(GenomeManager g)
        {
            genome = g;
            RecalculateMaxHP();
            _currentHP = Mathf.Min(_currentHP, _maxHP);
            OnHealthChanged?.Invoke(_currentHP, _maxHP);
        }

        public void RecalculateMaxHP()
        {
            float geneBonus = genome != null ? genome.TotalStats.HP : 0f;
            _maxHP = Mathf.Max(1f, baseMaxHP + geneBonus);
        }

        public bool TakeDamage(float amount, int attackerId)
        {
            if (!IsAlive) return false;
            _currentHP = Mathf.Max(0f, _currentHP - amount);
            OnHealthChanged?.Invoke(_currentHP, _maxHP);
            if (_currentHP <= 0f) { OnDeath?.Invoke(); return true; }
            return false;
        }

        public void Heal(float amount)
        {
            if (!IsAlive) return;
            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);
            OnHealthChanged?.Invoke(_currentHP, _maxHP);
        }

        public void ResetFullHealth()
        {
            RecalculateMaxHP();
            _currentHP = _maxHP;
            OnHealthChanged?.Invoke(_currentHP, _maxHP);
        }

        public void ForceKill()
        {
            _currentHP = 0f;
            OnHealthChanged?.Invoke(0f, _maxHP);
            OnDeath?.Invoke();
        }
    }
}

using UnityEngine;
using Genevore.Data;
using Genevore.Core;

namespace Genevore.Combat
{
    public static class CombatDamageSystem
    {
        public static event System.Action<int, float, bool> OnDamageApplied;
        public static event System.Action<int> OnEntityDied;

        public static bool ResolveAttack(in StatBlock attackerStats, IDamageable defender, int attackerId)
        {
            if (defender == null || !defender.IsAlive) return false;
            float raw = attackerStats.Attack;
            float finalDamage = Mathf.Max(1f, raw);
            bool died = defender.TakeDamage(finalDamage, attackerId);
            OnDamageApplied?.Invoke(attackerId, finalDamage, died);
            if (died) OnEntityDied?.Invoke(attackerId);
            return died;
        }

        public static bool ResolveAttack(GenomeManager attackerGenome, IDamageable defender, int attackerId)
        {
            if (attackerGenome == null) return false;
            return ResolveAttack(attackerGenome.TotalStats, defender, attackerId);
        }
    }
}

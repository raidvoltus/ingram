using UnityEngine;
namespace Genevore.Security
{
    public struct ObfuscatedFloat
    {
        private uint _masked, _key;
        public void Set(float value)
        {
            if (_key == 0) _key = (uint)(UnityEngine.Random.Range(1, int.MaxValue) ^ (int)(Time.realtimeSinceStartup * 1000f));
            _masked = System.BitConverter.ToUInt32(System.BitConverter.GetBytes(value), 0) ^ _key ^ 0xA5A5C3C3u;
        }
        public float Get()
        {
            if (_key == 0) return 0f;
            return System.BitConverter.ToSingle(System.BitConverter.GetBytes(_masked ^ _key ^ 0xA5A5C3C3u), 0);
        }
    }
    public class SecureBiomassVault : MonoBehaviour
    {
        private ObfuscatedFloat _biomass, _hp;
        public void WriteBiomass(float v) => _biomass.Set(v);
        public float ReadBiomass() => _biomass.Get();
        public void WriteHP(float v) => _hp.Set(v);
        public float ReadHP() => _hp.Get();
    }
}

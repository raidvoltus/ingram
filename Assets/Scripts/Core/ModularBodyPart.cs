using UnityEngine;
using Genevore.Data;

namespace Genevore.Core
{
    /// <summary>
    /// Component attached to every pooled body module prefab.
    /// Implements IPoolable to guarantee clean state on release.
    /// </summary>
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class ModularBodyPart : MonoBehaviour, IPoolable
    {
        public int PrefabHash;
        public BodyModuleSO ModuleData;

        private SkinnedMeshRenderer _smr;
        private MaterialPropertyBlock _mpb;

        private void Awake()
        {
            _smr = GetComponent<SkinnedMeshRenderer>();
            _mpb = new MaterialPropertyBlock();
        }

        public void OnSpawn()
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            if (_smr != null)
            {
                _smr.SetPropertyBlock(null);
            }
        }

        public void OnDespawn()
        {
            if (_smr != null)
            {
                _smr.bones = System.Array.Empty<Transform>();
                _smr.rootBone = null;
                _smr.SetPropertyBlock(null);
            }

            transform.SetParent(null);
        }

        public SkinnedMeshRenderer GetRenderer() => _smr;
    }
}

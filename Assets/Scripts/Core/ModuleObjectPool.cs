using System.Collections.Generic;
using UnityEngine;
using Genevore.Data;

namespace Genevore.Core
{
    public class ModuleObjectPool : MonoBehaviour
    {
        public static ModuleObjectPool Instance { get; private set; }

        [System.Serializable]
        public struct PoolConfig
        {
            public int PrefabHash;
            public GameObject Prefab;
            public int PrewarmCount;
        }

        [SerializeField] private PoolConfig[] configs;
        [SerializeField] private Transform poolRoot;

        private readonly Dictionary<int, Queue<GameObject>> _pools = new Dictionary<int, Queue<GameObject>>(16);
        private readonly Dictionary<int, GameObject> _prefabLookup = new Dictionary<int, GameObject>(16);

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (poolRoot == null)
            {
                var rootGo = new GameObject("[ModulePoolRoot]");
                rootGo.transform.SetParent(transform);
                poolRoot = rootGo.transform;
            }
            PrewarmAll();
        }

        private void PrewarmAll()
        {
            if (configs == null) return;
            for (int i = 0; i < configs.Length; i++)
            {
                var cfg = configs[i];
                if (cfg.Prefab == null || cfg.PrefabHash == 0) continue;
                if (!_pools.ContainsKey(cfg.PrefabHash))
                {
                    _pools[cfg.PrefabHash] = new Queue<GameObject>(cfg.PrewarmCount);
                    _prefabLookup[cfg.PrefabHash] = cfg.Prefab;
                }
                for (int j = 0; j < cfg.PrewarmCount; j++)
                    _pools[cfg.PrefabHash].Enqueue(CreateInstance(cfg.PrefabHash, cfg.Prefab));
            }
        }

        private GameObject CreateInstance(int hash, GameObject prefab)
        {
            var go = Instantiate(prefab, poolRoot);
            go.SetActive(false);
            if (go.GetComponent<IPoolable>() == null)
                Debug.LogError($"[ModuleObjectPool] Prefab {prefab.name} missing IPoolable");
            return go;
        }

        public GameObject Acquire(int prefabHash)
        {
            if (!_pools.TryGetValue(prefabHash, out var queue))
            {
                Debug.LogError($"[ModuleObjectPool] Unknown hash: {prefabHash}");
                return null;
            }
            GameObject instance;
            if (queue.Count > 0) instance = queue.Dequeue();
            else if (_prefabLookup.TryGetValue(prefabHash, out var prefab))
            {
                Debug.LogWarning($"[ModuleObjectPool] Pool exhausted for {prefabHash}");
                instance = CreateInstance(prefabHash, prefab);
            }
            else return null;
            instance.SetActive(true);
            instance.GetComponent<IPoolable>()?.OnSpawn();
            return instance;
        }

        public void Release(int prefabHash, GameObject instance)
        {
            if (instance == null) return;
            instance.GetComponent<IPoolable>()?.OnDespawn();
            instance.SetActive(false);
            instance.transform.SetParent(poolRoot);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            if (!_pools.TryGetValue(prefabHash, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefabHash] = queue;
            }
            queue.Enqueue(instance);
        }

        public int GetPooledCount(int prefabHash) =>
            _pools.TryGetValue(prefabHash, out var q) ? q.Count : 0;
    }
}

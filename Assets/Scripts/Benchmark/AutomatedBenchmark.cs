using UnityEngine;
using Genevore.Core;

namespace Genevore.Benchmark
{
    public class AutomatedBenchmark : MonoBehaviour
    {
        [SerializeField] private DevourController playerDevour;
        [SerializeField] private GenomeManager playerGenome;
        [SerializeField] private CreatureAssembly playerAssembly;
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform enemySpawnRoot;
        [SerializeField] private int enemyCount = 15;
        [SerializeField] private int mutationCycles = 100;
        [SerializeField] private float spawnRadius = 8f;
        [SerializeField] private float cycleInterval = 0.05f;

        private GameObject[] _enemies;
        private int _completedCycles;
        private bool _running;
        private float _timer;
        private float _startTime;

        private void Start()
        {
            if (playerDevour == null) playerDevour = FindObjectOfType<DevourController>();
            if (playerGenome == null) playerGenome = FindObjectOfType<GenomeManager>();
            if (playerAssembly == null) playerAssembly = FindObjectOfType<CreatureAssembly>();
            SpawnEnemies();
            _running = true;
            _startTime = Time.realtimeSinceStartup;
            Debug.Log($"[AutomatedBenchmark] Started. Target: {mutationCycles} cycles with {enemyCount} entities.");
        }

        private void SpawnEnemies()
        {
            _enemies = new GameObject[enemyCount];
            for (int i = 0; i < enemyCount; i++)
            {
                Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
                pos.y = 0f;
                GameObject enemy;
                if (enemyPrefab != null)
                    enemy = Instantiate(enemyPrefab, pos, Quaternion.identity, enemySpawnRoot);
                else
                {
                    enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    enemy.transform.position = pos;
                    enemy.transform.SetParent(enemySpawnRoot);
                }
                enemy.name = $"BenchmarkEnemy_{i}";
                _enemies[i] = enemy;
            }
        }

        private void Update()
        {
            if (!_running) return;
            _timer += Time.deltaTime;
            if (_timer < cycleInterval) return;
            _timer = 0f;
            if (_completedCycles >= mutationCycles) { Finish(); return; }
            if (playerDevour != null) playerDevour.ForceDevourCycle();
            else if (playerGenome != null) playerGenome.ApplyRandomMutation();
            _completedCycles++;
        }

        private void Finish()
        {
            _running = false;
            float elapsed = Time.realtimeSinceStartup - _startTime;
            Debug.Log($"[AutomatedBenchmark] COMPLETED {_completedCycles} cycles in {elapsed:F2}s.");
            Debug.Log($"[AutomatedBenchmark] Final gene count: {(playerGenome != null ? playerGenome.GeneCount : -1)}");
        }

        public void ResetBenchmark()
        {
            _completedCycles = 0; _timer = 0f; _running = true;
            _startTime = Time.realtimeSinceStartup;
            if (playerGenome != null) playerGenome.ClearAllGenes();
            if (playerAssembly != null) playerAssembly.DetachAll();
            if (playerDevour != null) playerDevour.ResetState();
            if (_enemies != null)
                for (int i = 0; i < _enemies.Length; i++)
                    if (_enemies[i] != null) _enemies[i].SetActive(true);
        }
    }
}

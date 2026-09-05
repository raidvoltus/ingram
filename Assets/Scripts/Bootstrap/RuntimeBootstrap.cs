using UnityEngine;
using Genevore.Core;
using Genevore.Player;
using Genevore.Stability;
using Genevore.QA;
using Genevore.Systems;
using Genevore.Combat;

namespace Genevore.Bootstrap
{
    public class RuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private bool buildOnAwake = true;
        private void Awake() { if (buildOnAwake) BuildMinimalSandbox(); }

        [ContextMenu("Build Minimal Sandbox")]
        public void BuildMinimalSandbox()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);

            var playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player";
            playerGo.transform.position = new Vector3(0f, 1f, 0f);
            Object.Destroy(playerGo.GetComponent<CapsuleCollider>());
            var cc = playerGo.AddComponent<CharacterController>();
            cc.height = 2f; cc.radius = 0.4f; cc.center = new Vector3(0f, 1f, 0f);

            playerGo.AddComponent<GenomeManager>();
            playerGo.AddComponent<DamageableEntity>();
            playerGo.AddComponent<DevourController>();
            playerGo.AddComponent<CreatureAssembly>();
            playerGo.AddComponent<MobilePlayerController>();
            playerGo.AddComponent<BiomassMetabolism>();
            playerGo.AddComponent<ProceduralScaleAdapter>();

            var systems = new GameObject("Systems");
            systems.AddComponent<ModuleObjectPool>();
            systems.AddComponent<ThermalAdaptiveSystem>();
            systems.AddComponent<AppLifecycleHandler>();
            systems.AddComponent<EnduranceTestRunner>();

            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }
            cam.transform.position = new Vector3(0f, 8f, -10f);
            cam.transform.LookAt(playerGo.transform);

            if (Object.FindObjectOfType<Light>() == null)
            {
                var lightGo = new GameObject("Directional Light");
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Directional;
                lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
            Application.targetFrameRate = 30;
            Debug.Log("[RuntimeBootstrap] Minimal sandbox ready.");
        }
    }
}

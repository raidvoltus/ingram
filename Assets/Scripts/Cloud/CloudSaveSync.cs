using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using Genevore.Stability;
using Genevore.Core;
using Genevore.Systems;
using Genevore.Combat;
using Genevore.Player;

namespace Genevore.Cloud
{
    public class CloudSaveSync : MonoBehaviour
    {
        public const byte FormatVersion = 1;
        public struct CloudPayload
        {
            public byte Version; public long TimestampUtcTicks; public int WorldSeed;
            public float PosX, PosY, PosZ, Biomass, CurrentHP, MaxHP;
            public byte GeneCount;
            public int GeneId0, GeneId1, GeneId2, GeneId3, GeneId4, GeneId5;
        }
        [SerializeField] private AppLifecycleHandler lifecycle;
        [SerializeField] private GenomeManager genome;
        [SerializeField] private BiomassMetabolism metabolism;
        [SerializeField] private DamageableEntity damageable;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private int worldSeed;
        [SerializeField] private string localFileName = "genevore_cloud.bin";
        private byte[] _lastUploadBytes;
        private float _loginStartTime = -1f;
        public bool IsLoggedIn { get; private set; }
        public float LastLoginDurationSeconds { get; private set; } = -1f;
        public event Action OnLoginSuccess;
        public void BeginLoginTimer() { _loginStartTime = Time.realtimeSinceStartup; IsLoggedIn = false; }
        public void NotifyLoginSuccess()
        {
            if (_loginStartTime > 0f) LastLoginDurationSeconds = Time.realtimeSinceStartup - _loginStartTime;
            IsLoggedIn = true; OnLoginSuccess?.Invoke();
        }
        public static CloudPayload ResolveConflict(in CloudPayload local, in CloudPayload remote)
            => remote.TimestampUtcTicks >= local.TimestampUtcTicks ? remote : local;
        public static byte[] Encode(in CloudPayload p)
        {
            using (var ms = new MemoryStream(64))
            using (var gz = new GZipStream(ms, System.IO.Compression.CompressionLevel.Optimal, true))
            using (var bw = new BinaryWriter(gz))
            {
                bw.Write(p.Version); bw.Write(p.TimestampUtcTicks); bw.Write(p.WorldSeed);
                bw.Write(p.PosX); bw.Write(p.PosY); bw.Write(p.PosZ);
                bw.Write(p.Biomass); bw.Write(p.CurrentHP); bw.Write(p.MaxHP); bw.Write(p.GeneCount);
                bw.Write(p.GeneId0); bw.Write(p.GeneId1); bw.Write(p.GeneId2);
                bw.Write(p.GeneId3); bw.Write(p.GeneId4); bw.Write(p.GeneId5);
                bw.Flush(); gz.Close(); return ms.ToArray();
            }
        }
        public void SaveNow()
        {
            var p = new CloudPayload { Version = FormatVersion, TimestampUtcTicks = DateTime.UtcNow.Ticks, WorldSeed = worldSeed };
            if (playerTransform != null) { var pos = playerTransform.position; p.PosX = pos.x; p.PosY = pos.y; p.PosZ = pos.z; }
            if (metabolism != null) p.Biomass = metabolism.CurrentBiomass;
            if (damageable != null) { p.CurrentHP = damageable.CurrentHP; p.MaxHP = damageable.MaxHP; }
            if (genome != null)
            {
                p.GeneCount = (byte)Mathf.Min(genome.GeneCount, 6);
                for (int i = 0; i < p.GeneCount; i++) { var g = genome.GetGeneAt(i); int id = g != null ? g.GeneId : 0;
                    if (i==0) p.GeneId0=id; else if (i==1) p.GeneId1=id; else if (i==2) p.GeneId2=id;
                    else if (i==3) p.GeneId3=id; else if (i==4) p.GeneId4=id; else p.GeneId5=id; }
            }
            _lastUploadBytes = Encode(p);
            try { File.WriteAllBytes(Path.Combine(Application.persistentDataPath, localFileName), _lastUploadBytes); } catch {}
            Debug.Log($"[CloudSave] {_lastUploadBytes.Length} bytes");
        }
        private void OnApplicationPause(bool pause) { if (pause) SaveNow(); }
    }
}

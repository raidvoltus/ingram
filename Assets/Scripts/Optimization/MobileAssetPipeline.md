# Mobile Asset Pipeline — Stage 3 Constraints

Target device: Snapdragon 720G / 4 GB system RAM / ≤ 1.2 GB VRAM budget.

## Texture Rules (mandatory)

| Asset class | Max resolution | Compression |
|-------------|----------------|-------------|
| Player / Hero modules | 1024 x 1024 | ASTC 6x6 |
| Enemy modules | 1024 x 1024 | ASTC 6x6 |
| Environment | 512 x 512 | ASTC 8x8 |
| UI / Icons | 256 x 256 | ASTC 6x6 or ETC2 |

- Never ship uncompressed or DXT/BC on Android.
- Enable GPU Instancing on every environment material.
- Prefer URP Lit / Simple Lit.
- Environment LODs: LOD0 <= 1500 tris, LOD1 <= 500, LOD2 <= 150.
- Skinned modules: bone count <= 40 per module.
- Addressables groups: WorldChunks_Local, WorldChunks_Remote, EnemyModules, UI.

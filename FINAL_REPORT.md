# Genevore Production Build Report

## Environment

| Item | Value |
|------|--------|
| OS | Linux x86_64 container |
| RAM | **1.2 GiB** (0 swap) |
| Disk | 20 GiB free ~19 GiB |
| Java | OpenJDK 21 |
| adb/aapt/apksigner | Installed |
| Unity Editor | **Not runnable** (RAM) |

Unity 2022.3.32f1 Linux tarball is **3.78 GB** (HTTP 200 verified). Host cannot extract/run Editor.

## Project reconstruction: DONE

Packages, ProjectSettings, Bootstrap.unity, RuntimeBootstrap, AndroidManifest, Editor build menu, build-android.sh, link.xml, Stages 1–6 scripts.

## APK
**Not generated in this environment.**

On a machine with ≥8 GB RAM + Unity 2022.3.32f1 Android IL2CPP:
```
git clone https://github.com/raidvoltus/ingram.git && cd ingram
export UNITY_PATH=/path/to/Unity && ./build-android.sh
```

# Android Release Build Instructions

## Requirements
- Unity **2022.3.32f1 LTS** + Android Build Support + IL2CPP
- Android SDK API 24–34

## Steps
1. Open this folder in Unity; wait for packages.
2. Create `Assets/Scenes/Bootstrap.unity` with `RuntimeBootstrap` component.
3. Menu **Genevore → Build Android Release APK** (or AAB).
4. Output: `Builds/Genevore-Release.apk`

## Player Settings
- Package: `com.genevore.ingram`
- Min SDK 24 / Target 34 / IL2CPP / ARM64 / Stripping Medium + link.xml

## Signing
Production keystore not in repo. First APK uses Unity debug keystore = RELEASE CANDIDATE.

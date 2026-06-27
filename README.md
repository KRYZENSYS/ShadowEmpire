# 🎮 ShadowEmpire — AAA Open World Game

> **Production-level Unity 6 game project** — open world, multiplayer, AI, shop, and battle pass.

A cinematic AAA open-world game built with **Unity 6 LTS + C#** featuring:
realistic graphics, ray-traced lighting, dynamic weather, advanced NPC AI,
online multiplayer, voice chat, monetization, and a full Battle Pass system.

---

## ✨ Features

| Category | Systems |
|---|---|
| 🌍 **World** | Open-world city + forests + villages + mountains, dynamic day/night cycle, weather system |
| 🎨 **Graphics** | HDRP ray tracing, volumetric lighting, post-processing (bloom, DOF, motion blur, SSAO, SSR) |
| 🤖 **AI** | NavMesh pathfinding, FSM behaviors, combat AI, dynamic NPC schedules |
| 🎮 **Gameplay** | FPS + Third-Person switch, vehicles (cars/bikes/helicopters/airplanes), character customization |
| 🔫 **Combat** | Realistic weapons (recoil, ballistics), footstep audio, environmental sounds |
| 🌐 **Multiplayer** | Netcode for GameObjects, voice chat (Vivox/Dissonance), server-authoritative |
| 💰 **Monetization** | In-game shop, skins, Battle Pass (free + premium), IAP via Unity IAP |
| 💾 **Persistence** | Save/Load with JSON + PlayerPrefs, Cloud Save via Unity Gaming Services |
| 📱 **Optimization** | LODs, GPU instancing, occlusion culling, adaptive performance, mobile + PC |
| 🛡 **Security** | Server-side validation, basic anti-cheat, packet signature checks |
| 🎬 **UI/UX** | Animated main menu, settings panel, HUD, responsive mobile UI |

---

## 🏗 Architecture

```
ShadowEmpire/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Core/          — GameManager, Bootstrap, EventBus
│   │   │   ├── Player/        — Controller, Input, Camera, Customization
│   │   │   ├── AI/            — FSM, NPCController, EnemyAI
│   │   │   ├── Combat/        — Weapon, Ballistics, Recoil
│   │   │   ├── Vehicles/      — CarController, HelicopterController
│   │   │   ├── World/         — DayNightCycle, WeatherSystem, Streaming
│   │   │   ├── Multiplayer/   — NetworkManager, VoiceChat, SyncVars
│   │   │   ├── UI/            — MainMenu, HUD, Shop, BattlePass
│   │   │   ├── Inventory/     — Inventory, Items, Equipment
│   │   │   ├── Shop/          — ShopManager, Currency, IAP
│   │   │   ├── BattlePass/    — BPManager, Rewards, Tiers
│   │   │   ├── Missions/      — MissionSystem, Objectives, Storyline
│   │   │   ├── Audio/         — FootstepSystem, AudioManager
│   │   │   ├── Save/          — SaveSystem, CloudSave
│   │   │   ├── Security/      — AntiCheat, Validation
│   │   │   └── Utils/         — Helpers, Extensions, Pools
│   │   ├── Prefabs/
│   │   ├── Materials/
│   │   ├── Shaders/
│   │   ├── Animations/
│   │   ├── Audio/
│   │   └── Scenes/
│   └── Plugins/                — Third-party packages
├── Packages/                   — Unity Package Manager
├── ProjectSettings/            — Unity project config
└── Server/                     — Node.js multiplayer server
    ├── src/
    │   ├── index.js
    │   ├── rooms/
    │   ├── handlers/
    │   └── db/
    └── package.json
```

---

## 🚀 Quick Start

### Requirements
- Unity 6 LTS (6000.x) or newer
- HDRP package (High Definition Render Pipeline)
- Netcode for GameObjects 2.x
- Node.js 18+ (for the dedicated server)

### Install

```bash
# Clone the repo
git clone https://github.com/KRYZENSYS/ShadowEmpire.git
cd ShadowEmpire

# Open Unity Hub → Add project → select this folder
# Unity auto-imports assets (first run: ~5–10 min)

# Install Node.js server deps
cd Server
npm install
npm start
```

### Build

| Platform | Menu → |
|---|---|
| Windows 64-bit | File → Build Settings → PC, Mac & Linux → Build |
| macOS | → macOS → Build |
| Android (ARM64) | → Android → Switch Platform → Build |
| iOS | → iOS → Build (Xcode opens) |

---

## 🛠 Recommended Packages

Open `Window → Package Manager` and install:

| Package | Version | Why |
|---|---|---|
| `com.unity.render-pipelines.high-definition` | 17.x | HDRP for cinematic graphics |
| `com.unity.netcode.gameobjects` | 2.x | Multiplayer |
| `com.unity.services.authentication` | 3.x | UGS login |
| `com.unity.services.cloudcode` | 4.x | Cloud logic |
| `com.unity.services.cloudsave` | 2.x | Player data |
| `com.unity.purchasing` | 4.x | IAP for shop |
| `com.unity.cinemachine` | 3.x | Cinematic cameras |
| `com.unity.inputsystem` | 1.x | Modern input |

Recommended Asset Store:
- **Synty Studios POLYGON** (low-poly stylized characters/cars)
- **HQ Fighting Animation Bundle** (Mixamo free alternative)
- **AllSky Free** (10 GB skybox)
- **Epic Toon FX** (VFX)
- **Odin Inspector** (developer productivity)

---

## 🌐 Multiplayer Server

`/Server` is a **Colyseus** authoritative Node.js server.

```bash
cd Server
npm install
npm start
# → listening on ws://localhost:2567
```

Unity connects via `Multiplayer/NetworkManager.cs`.

---

## 💰 Monetization

- **Currency:** Soft Coins (`Gold`) and Hard Gems (`Gems`)
- **Battle Pass:** 100 tiers, free + premium tracks
- **Shop:** Rotating cosmetics, vehicle skins, weapon skins
- **IAP Products** (defined in `Shop/IAPProducts.cs`)
- **Ad placements:** Rewarded video for revives & bonus XP

---

## 📱 Platforms Tested

- ✅ PC (Windows 10/11) — 60–120 FPS on RTX 3060
- ✅ macOS (Apple Silicon) — 60 FPS native
- ✅ Android 11+ — 30–60 FPS on mid-range
- ⚠️ iOS — works, needs Apple Developer signing
- ⚠️ Console — needs platform SDK + NDA

---

## 📜 License

MIT — free for commercial and personal use.

## 👤 Author

**FirdavsVIP / KRYZENSYS**

---

> Need help? Open an issue, or check `Assets/_Project/Scripts/Core/README_DEV.md`.
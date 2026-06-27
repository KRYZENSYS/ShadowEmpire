// =============================================================================
// Bootstrap.cs
// =============================================================================
// PURPOSE:
//   First scene loaded at startup. Creates persistent managers, loads settings,
//   then transitions to MainMenu. Place this script on a single GameObject in
//   the "Bootstrap" scene (Build Index 0).
// =============================================================================

using UnityEngine;

namespace ShadowEmpire.Core
{
    public class Bootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            // Auto-create via [RuntimeInitializeOnLoadMethod] — no scene needed,
            // but the empty Bootstrap scene is still first in Build Settings.
        }

        private void Awake()
        {
            CreatePersistentManager<GameManager>("GameManager");
            CreatePersistentManager<ShadowEmpire.Audio.AudioManager>("AudioManager");
            CreatePersistentManager<ShadowEmpire.Save.SaveSystem>("SaveSystem");
            CreatePersistentManager<ShadowEmpire.Shop.CurrencyService>("CurrencyService");
            CreatePersistentManager<ShadowEmpire.BattlePass.BattlePassManager>("BattlePassManager");
            CreatePersistentManager<ShadowEmpire.Multiplayer.NetworkManager>("NetworkManager");

            Application.runInBackground = true;
            Debug.Log("[Bootstrap] Persistent managers created.");
        }

        private static T CreatePersistentManager<T>(string name) where T : Component
        {
            if (FindObjectOfType<T>() != null) return FindObjectOfType<T>();
            var go = new GameObject(name);
            DontDestroyOnLoad(go);
            return go.AddComponent<T>();
        }
    }
}
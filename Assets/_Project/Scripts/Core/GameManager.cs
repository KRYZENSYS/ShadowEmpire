// =============================================================================
// GameManager.cs
// =============================================================================
// PURPOSE:
//   Central game-state controller. Singleton, lives across scene loads.
//   Handles bootstrap, scene transitions, save/load triggers, and shutdown.
//
// ENGINE: Unity 6 LTS
// LANGUAGE: C# 9.0+
// LAYER: Core (always loaded)
// =============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShadowEmpire.Core
{
    /// <summary>
    /// High-level game state machine.
    /// </summary>
    public enum GameState
    {
        Booting,
        MainMenu,
        Loading,
        Playing,
        Paused,
        Cutscene,
        InShop,
        InBattlePass,
        ShuttingDown
    }

    /// <summary>
    /// Singleton game manager — survives scene loads (DontDestroyOnLoad).
    /// Subscribe to <see cref="OnStateChanged"/> to react to state transitions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // -------- Singleton --------
        public static GameManager Instance { get; private set; }

        // -------- Public API --------
        public GameState State { get; private set; } = GameState.Booting;
        public event Action<GameState, GameState> OnStateChanged; // (old, new)

        [Header("Config")]
        [SerializeField] private float targetFps = 60f;
        [SerializeField] private bool mobileOptimized = false;

        // -------- Lifecycle --------
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyPerformanceProfile();
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Start()
        {
            // Frame-rate target
            Application.targetFrameRate = targetFps;
            QualitySettings.vSyncCount = 0;

            yield return null; // wait one frame for everything else

            SetState(GameState.MainMenu);
        }

        // -------- State transitions --------
        public void SetState(GameState newState)
        {
            if (newState == State) return;
            var old = State;
            State = newState;
            OnStateChanged?.Invoke(old, newState);

            Time.timeScale = newState == GameState.Paused ? 0f : 1f;
            Debug.Log($"[GameManager] {old} -> {newState}");
        }

        public void Pause()  => SetState(GameState.Paused);
        public void Resume() => SetState(GameState.Playing);

        // -------- Scene management --------
        public void LoadScene(string sceneName, bool networked = true)
        {
            SetState(GameState.Loading);
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            op.allowSceneActivation = false;
            while (op.progress < 0.9f) yield return null;
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
            SetState(GameState.Playing);
        }

        // -------- Performance --------
        private void ApplyPerformanceProfile()
        {
            if (!mobileOptimized && !Application.isMobilePlatform) return;
            // Mid-tier mobile profile
            QualitySettings.renderPipeline = null; // assigned via HDRP asset
            QualitySettings.antiAliasing = 1;
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 50f;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        // -------- Shutdown --------
        private void OnApplicationQuit()
        {
            SetState(GameState.ShuttingDown);
            // SaveSystem.SaveAll() hook here
        }
    }
}
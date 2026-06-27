// =============================================================================
// BattlePassManager.cs
// =============================================================================
// PURPOSE:
//   100-tier Battle Pass with free + premium tracks. XP earned via missions,
//   matches, and daily challenges. Premium grants bonus cosmetic rewards.
//
// ENGINE: Unity 6
// =============================================================================

using System;
using UnityEngine;
using ShadowEmpire.Core;

namespace ShadowEmpire.BattlePass
{
    public enum Track { Free, Premium }

    [Serializable]
    public class Reward
    {
        public Track track;
        public int tier;
        public string itemId;
        public string itemName;
        public Sprite icon;
        public string rarity; // Common, Rare, Epic, Legendary
    }

    public class BattlePassManager : MonoBehaviour
    {
        public static BattlePassManager Instance { get; private set; }

        [Header("Season")]
        public int seasonNumber = 1;
        public int totalTiers = 100;
        public int xpPerTier = 1000;

        [Header("State")]
        public int currentTier = 0;
        public int currentXp = 0;
        public bool isPremiumUnlocked = false;

        [Header("Rewards")]
        public Reward[] rewards;

        public event Action<int> OnTierUnlocked;
        public event Action OnPremiumUnlocked;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void AddXp(int amount)
        {
            currentXp += amount;
            while (currentXp >= xpPerTier)
            {
                currentXp -= xpPerTier;
                currentTier++;
                OnTierUnlocked?.Invoke(currentTier);
                EventBus.Publish(new BattlePassTierUnlockedEvent { Tier = currentTier });
            }
            Save();
        }

        public Reward[] GetRewardsForTier(int tier)
        {
            // LINQ could be used; manual loop for IL2CPP/AOT safety
            var list = new System.Collections.Generic.List<Reward>();
            foreach (var r in rewards) if (r.tier == tier) list.Add(r);
            return list.ToArray();
        }

        public bool ClaimReward(Track track, int tier)
        {
            if (track == Track.Premium && !isPremiumUnlocked) return false;
            if (tier > currentTier) return false;
            // Hook into InventoryService here
            return true;
        }

        public void UnlockPremium()
        {
            if (isPremiumUnlocked) return;
            if (ShadowEmpire.Shop.CurrencyService.Instance.Spend(ShadowEmpire.Shop.CurrencyService.GEMS, 1000))
            {
                isPremiumUnlocked = true;
                OnPremiumUnlocked?.Invoke();
                Save();
            }
        }

        // -------- Persistence --------
        private void Save()
        {
            PlayerPrefs.SetInt($"bp_tier_{seasonNumber}", currentTier);
            PlayerPrefs.SetInt($"bp_xp_{seasonNumber}", currentXp);
            PlayerPrefs.SetInt($"bp_premium_{seasonNumber}", isPremiumUnlocked ? 1 : 0);
        }
        private void Load()
        {
            currentTier = PlayerPrefs.GetInt($"bp_tier_{seasonNumber}", 0);
            currentXp   = PlayerPrefs.GetInt($"bp_xp_{seasonNumber}", 0);
            isPremiumUnlocked = PlayerPrefs.GetInt($"bp_premium_{seasonNumber}", 0) == 1;
        }
    }
}
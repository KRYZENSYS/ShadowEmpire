// =============================================================================
// CurrencyService.cs
// =============================================================================
// PURPOSE:
//   Soft + hard currency management. Tracks coins and gems, syncs with the
//   server, and broadcasts changes via EventBus.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowEmpire.Core;

namespace ShadowEmpire.Shop
{
    public class CurrencyService : MonoBehaviour
    {
        public static CurrencyService Instance { get; private set; }

        public const string COINS = "Coins";
        public const string GEMS  = "Gems";

        private readonly Dictionary<string, long> _wallets = new() { { COINS, 0 }, { GEMS, 0 } };

        public event Action<string, long> OnBalanceChanged;
        public long Get(string currency) => _wallets.TryGetValue(currency, out var v) ? v : 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public bool CanAfford(string currency, long amount) => Get(currency) >= amount;

        public void Add(string currency, long amount)
        {
            _wallets[currency] = Get(currency) + amount;
            OnBalanceChanged?.Invoke(currency, _wallets[currency]);
            EventBus.Publish(new CurrencyChangedEvent { Currency = currency, Amount = amount });
            Save();
        }

        public bool Spend(string currency, long amount)
        {
            if (!CanAfford(currency, amount)) return false;
            _wallets[currency] -= amount;
            OnBalanceChanged?.Invoke(currency, _wallets[currency]);
            EventBus.Publish(new CurrencyChangedEvent { Currency = currency, Amount = -amount });
            Save();
            return true;
        }

        // -------- Persistence --------
        private const string KEY = "wallets";
        [Serializable] private class WalletDto { public string k; public long v; }
        [Serializable] private class WalletsDto { public WalletDto[] items; }

        private void Save()
        {
            var dto = new WalletsDto
            {
                items = new WalletDto[_wallets.Count]
            };
            int i = 0;
            foreach (var kv in _wallets) dto.items[i++] = new WalletDto { k = kv.Key, v = kv.Value };
            PlayerPrefs.SetString(KEY, JsonUtility.ToJson(dto));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(KEY)) return;
            var dto = JsonUtility.FromJson<WalletsDto>(PlayerPrefs.GetString(KEY));
            if (dto?.items == null) return;
            foreach (var it in dto.items) _wallets[it.k] = it.v;
        }
    }
}
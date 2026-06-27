// =============================================================================
// InventoryService.cs
// =============================================================================
// PURPOSE:
//   Player inventory. Stack counts, equipment slots, save/load.
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using ShadowEmpire.Core;

namespace ShadowEmpire.Inventory
{
    public enum ItemType { Weapon, Outfit, Consumable, Material, Skin }

    [Serializable]
    public class ItemStack
    {
        public string itemId;
        public string displayName;
        public ItemType type;
        public int amount;
        public Sprite icon;
        public bool equipped;
    }

    public class InventoryService : MonoBehaviour
    {
        public static InventoryService Instance { get; private set; }

        public int capacity = 100;
        public List<ItemStack> items = new();

        public event Action OnInventoryChanged;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public bool Add(string itemId, string name, ItemType type, int amount = 1)
        {
            if (type != ItemType.Material && type != ItemType.Consumable)
            {
                // Non-stackable — create a new stack
                if (items.Count >= capacity) return false;
                items.Add(new ItemStack { itemId = itemId, displayName = name, type = type, amount = 1 });
            }
            else
            {
                var existing = items.Find(i => i.itemId == itemId);
                if (existing != null) existing.amount += amount;
                else
                {
                    if (items.Count >= capacity) return false;
                    items.Add(new ItemStack { itemId = itemId, displayName = name, type = type, amount = amount });
                }
            }
            EventBus.Publish(new ItemPickedUpEvent { ItemId = itemId, Amount = amount });
            OnInventoryChanged?.Invoke();
            Save();
            return true;
        }

        public bool Remove(string itemId, int amount = 1)
        {
            var stack = items.Find(i => i.itemId == itemId);
            if (stack == null || stack.amount < amount) return false;
            stack.amount -= amount;
            if (stack.amount <= 0) items.Remove(stack);
            OnInventoryChanged?.Invoke();
            Save();
            return true;
        }

        public bool Equip(string itemId)
        {
            var stack = items.Find(i => i.itemId == itemId);
            if (stack == null) return false;
            // Unequip same type
            foreach (var s in items) if (s.type == stack.type && s.equipped) s.equipped = false;
            stack.equipped = true;
            OnInventoryChanged?.Invoke();
            Save();
            return true;
        }

        // -------- Persistence --------
        private void Save()
        {
            PlayerPrefs.SetString("inventory", JsonUtility.ToJson(new Wrapper { items = items }));
        }
        private void Load()
        {
            if (!PlayerPrefs.HasKey("inventory")) return;
            var w = JsonUtility.FromJson<Wrapper>(PlayerPrefs.GetString("inventory"));
            if (w?.items != null) items = w.items;
        }
        [Serializable] private class Wrapper { public List<ItemStack> items; }
    }
}
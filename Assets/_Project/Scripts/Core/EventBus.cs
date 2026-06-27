// =============================================================================
// EventBus.cs
// =============================================================================
// PURPOSE:
//   Lightweight global pub/sub bus. Decouples systems without static singletons.
//   Use for cross-system notifications (player died, item picked up, etc.).
//
// USAGE:
//   EventBus.Subscribe<PlayerDiedEvent>(e => Debug.Log(e.Message));
//   EventBus.Publish(new PlayerDiedEvent("Hero"));
// =============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowEmpire.Core
{
    /// <summary>Marker interface for all events.</summary>
    public interface IEvent { }

    /// <summary>
    /// Static event bus. Thread-unsafe — only call from the Unity main thread.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subs = new();

        public static void Subscribe<T>(Action<T> handler) where T : IEvent
        {
            var t = typeof(T);
            if (!_subs.TryGetValue(t, out var list))
            {
                list = new List<Delegate>();
                _subs[t] = list;
            }
            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IEvent
        {
            if (_subs.TryGetValue(typeof(T), out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T evt) where T : IEvent
        {
            if (!_subs.TryGetValue(typeof(T), out var list)) return;
            // Iterate copy in case handler unsubscribes
            var copy = list.ToArray();
            foreach (var d in copy)
            {
                try { ((Action<T>)d).Invoke(evt); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        public static void Clear() => _subs.Clear();
    }

    // -------- Sample event types --------
    public struct PlayerDiedEvent : IEvent
    {
        public string Name;
        public PlayerDiedEvent(string n) { Name = n; }
    }

    public struct ItemPickedUpEvent : IEvent
    {
        public string ItemId;
        public int Amount;
    }

    public struct MissionCompletedEvent : IEvent
    {
        public string MissionId;
        public int XpReward;
    }

    public struct CurrencyChangedEvent : IEvent
    {
        public string Currency;
        public long Amount;
    }

    public struct BattlePassTierUnlockedEvent : IEvent
    {
        public int Tier;
    }
}
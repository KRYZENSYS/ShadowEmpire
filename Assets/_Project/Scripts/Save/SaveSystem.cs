// =============================================================================
// SaveSystem.cs
// =============================================================================
// PURPOSE:
//   Centralized save/load with JSON. Supports multiple slots, cloud sync hook.
// =============================================================================

using System;
using System.IO;
using UnityEngine;

namespace ShadowEmpire.Save
{
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        private const string FOLDER = "saves";
        private const string EXT = ".json";

        public string currentSlot = "slot1";

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Directory.CreateDirectory(GetFolder());
        }

        public void Save<T>(T data, string key = "state")
        {
            var dto = new SaveDto { key = key, json = JsonUtility.ToJson(data) };
            var path = GetPath(currentSlot, key);
            File.WriteAllText(path, JsonUtility.ToJson(dto));
            Debug.Log($"[Save] {currentSlot}/{key} -> {path}");
        }

        public T Load<T>(string key = "state") where T : new()
        {
            var path = GetPath(currentSlot, key);
            if (!File.Exists(path)) return new T();
            var dto = JsonUtility.FromJson<SaveDto>(File.ReadAllText(path));
            return JsonUtility.FromJson<T>(dto.json);
        }

        public void DeleteSlot(string slot)
        {
            var dir = Path.Combine(Application.persistentDataPath, FOLDER, slot);
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }

        private string GetFolder() => Path.Combine(Application.persistentDataPath, FOLDER);
        private string GetPath(string slot, string key) => Path.Combine(GetFolder(), slot, key + EXT);

        [Serializable] private class SaveDto { public string key; public string json; }
    }
}
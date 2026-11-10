using System;
using System.IO;
using UnityEngine;

namespace Project.Core
{
    public sealed class JsonSaveService : ISaveService
    {
        readonly string _directory;

        public JsonSaveService(string directory = null)
        {
            _directory = directory ?? Application.persistentDataPath;
        }

        public T Load<T>(string key) where T : class, new()
        {
            string path = PathFor(key);
            try
            {
                if (File.Exists(path))
                {
                    var data = JsonUtility.FromJson<T>(File.ReadAllText(path));
                    if (data != null) return data;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Failed to read '{key}': {e.Message}");
            }

            return new T();
        }

        public void Save<T>(string key, T data) where T : class
        {
            try
            {
                Directory.CreateDirectory(_directory);
                File.WriteAllText(PathFor(key), JsonUtility.ToJson(data));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Failed to write '{key}': {e.Message}");
            }
        }

        string PathFor(string key) => Path.Combine(_directory, key + ".json");
    }
}

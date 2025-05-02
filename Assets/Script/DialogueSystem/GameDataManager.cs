using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameDataManager
{
    private readonly Dictionary<Type, IGameData[]> _batchCache = new();
    private readonly Dictionary<Type, Dictionary<int, IGameData>> _idCache = new();

    public void AddBatch<T>(T[] items, bool forceUpdate = false) where T : IGameData
    {
        var type = typeof(T);
        if (_batchCache.ContainsKey(type) && !forceUpdate) return;

        _batchCache[type] = items.Cast<IGameData>().ToArray();

        var byId = new Dictionary<int, IGameData>();
        foreach (var item in items)
        {
            if (!byId.ContainsKey(item.ID))
                byId[item.ID] = item;
            else
                Debug.LogWarning($"[GameDataManager] {type.Name} 重複 ID {item.ID} 被跳過");
        }
        _idCache[type] = byId;
    }

    public async UniTask AddAsync<T>(IGameDataHandler handler, bool forceUpdate = false) where T : IGameData
    {
        var type = typeof(T);
        if (_batchCache.ContainsKey(type) && !forceUpdate) return;

        T[] items = await handler.LoadAsync<T>();
        AddBatch(items, forceUpdate: true);
    }

    public T[] GetAll<T>() where T : IGameData
    {
        var type = typeof(T);
        if (_batchCache.TryGetValue(type, out var arr))
            return arr.Cast<T>().ToArray();
        return Array.Empty<T>();
    }

    public T GetById<T>(int id) where T : IGameData
    {
        var type = typeof(T);
        if (_idCache.TryGetValue(type, out var map) && map.TryGetValue(id, out var item))
            return (T)item;
        return default;
    }

    /// <summary>
    /// 專門用於 DialogueData：根據場景 ID 撈該場景所有行，並依 Line 排序
    /// </summary>
    public List<DialogueData> GetDialogueByID(int sceneID)
    {
        // 先拿出所有 DialogueData，再篩 ID、排序 Line
        return GetAll<DialogueData>()
            .Where(d => d.ID == sceneID)
            .OrderBy(d => d.Line)
            .ToList();
    }

    public List<T> Query<T>(Func<T, bool> predicate) where T : IGameData
    {
        return GetAll<T>().Where(predicate).ToList();
    }

    public void Clear<T>() where T : IGameData
    {
        var type = typeof(T);
        _batchCache.Remove(type);
        _idCache.Remove(type);
    }
}

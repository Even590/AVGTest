using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

public class GameDataManager
{
    private readonly Dictionary<Type, IGameData[]> _batchCache = new();

    public void AddBatch<T>(T[] items, bool forceUpdate = false) where T : IGameData
    {
        var type = typeof(T);
        if (_batchCache.ContainsKey(type) && !forceUpdate) return;

        _batchCache[type] = items.Cast<IGameData>().ToArray();
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

    public List<T> GetById<T>(int id) where T : IGameData
    {
        return GetAll<T>()
            .Where(d => d.ID == id)
            .ToList();
    }

    public List<T> Query<T>(Func<T, bool> predicate) where T : IGameData
    {
        return GetAll<T>().Where(predicate).ToList();
    }

    public void Clear<T>() where T : IGameData
    {
        _batchCache.Remove(typeof(T));
    }
}

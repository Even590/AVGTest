using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameDataManager
{
    Dictionary<Type, IGameData[]> m_GameData = new Dictionary<Type, IGameData[]>();

    public void Add<T>(IGameData[] gameDatas, bool isForceUpdate = false) where T : IGameData
    {
        if (m_GameData.ContainsKey(typeof(T)))
        {
            if (isForceUpdate)
            {
                Remove<T>();
                RememberNewArray<T>(gameDatas);
            }
            else
            {
                Debug.LogWarning("The file already save, please makesure isForceUpdate = true");
            }
        }
        else
        {
            RememberNewArray<T>(gameDatas);
        }
    }

    public void Add<T>(IGameDataHandler handler, bool isForceUpdate = false) where T : IGameData
    {
        if (m_GameData.ContainsKey(typeof(T)))
        {
            if (isForceUpdate)
            {
                Remove<T>();
                RememberNewArray<T>(handler.Load<T>() as IGameData[]);
            }
            else
            {
                Debug.LogWarning("The file already save, please makesure isForceUpdate = true");
            }
        }
        else
        {
            RememberNewArray<T>(handler.Load<T>() as IGameData[]);
        }
    }

    public async UniTask AddAsync<T>(IGameDataHandler handler, bool isForceUpdate = false) where T : IGameData
    {
        if (m_GameData.ContainsKey(typeof(T)))
        {
            if (isForceUpdate)
            {
                Remove<T>();

                T[] data = await handler.LoadAsync<T>();
                var asIGameData = Array.ConvertAll<T, IGameData>(data, item => item);
                RememberNewArray<T>(asIGameData);
            }
            else
            {
                Debug.LogWarning("The file already save, please makesure isForceUpdate = true");
            }
        }
        else
        {
            T[] data = await handler.LoadAsync<T>();
            var asIGameData = Array.ConvertAll<T, IGameData>(data, item => item);
            RememberNewArray<T>(asIGameData);
        }
    }

    public void RememberNewArray<T>(IGameData[] array) where T : IGameData
    {
        IGameData[] newArray = new IGameData[array.Length];
        for (int i = 0; i < newArray.Length; i++) 
        { 
            newArray[i] = array[i];
        }

        m_GameData.Add(typeof(T), newArray);
    }

    public void Remove<T>() where T : IGameData
    {
        if (m_GameData.ContainsKey(typeof(T)))
        {
            m_GameData.Remove(typeof(T));
        }
    }
}

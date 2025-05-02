using System.Collections.Generic;
using UnityEngine;

public static class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return new List<T>(wrapper.values);
    }

    public static string ToJson<T>(List<T> list, bool prettyPrint = false)
    {
        var wrapper = new Wrapper<T> { values = list.ToArray() };
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    private class Wrapper<T>
    {
        public T[] values;
    }
}

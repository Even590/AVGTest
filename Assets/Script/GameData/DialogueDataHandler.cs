using AVGTest.Asset.Script;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;

public class DialogueDataHandler : IGameDataHandler
{
    private readonly DialogueDataService _service;
    public DialogueDataHandler(string sheetURL)
    {
        _service = new DialogueDataService(sheetURL);
    }
    public T[] Load<T>() where T : IGameData
    {
        throw new NotSupportedException("Sync load not supported. Use LoadAsync.");
    }
    public async UniTask<T[]> LoadAsync<T>() where T : IGameData
    {
        if(typeof(T) == typeof(DialogueData))
        {
            var list = await _service.GetDialogueListAsync(forceUpdate : false);
            return list.Cast<T>().ToArray();
        }

        return Array.Empty<T>();
    }
}

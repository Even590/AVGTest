using Cysharp.Threading.Tasks;

public interface IGameDataHandler
{
    T[] Load<T>() where T : IGameData;
    public UniTask<T[]> LoadAsync<T>() where T : IGameData;
}

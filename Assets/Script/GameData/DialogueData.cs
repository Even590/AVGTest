[System.Serializable]
public class DialogueData : IGameData
{
    public int ID;
    public int Line;
    public string Command;
    public string CharacterSide;
    public string CharacterKey;
    public string LoadMode;
    public string HightLight;
    public string BG;
    public string CG;
    public string Name;
    public string Dialogue;

    int IGameData.ID => this.ID;
}

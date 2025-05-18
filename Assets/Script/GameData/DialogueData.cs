using System.Collections.Generic;

[System.Serializable]
public class DialogueBranch
{
    public string Text;
    public int TargetBranch;
}

[System.Serializable]
public class DialogueData : IGameData
{
    public int ID;
    public int Branch;

    public string Command;
    public string CharacterSide;
    public string CharacterKey;
    public string LoadMode;
    public string HightLight;
    public string BG;
    public string CG;
    public string Name;
    public string Dialogue;

    public List<DialogueBranch> dialogueBranches;

    int IGameData.ID => this.ID;
}

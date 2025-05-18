using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueView ui;
        [SerializeField] private string sheetURL;

        private GameDataManager _dataManager;
        private Dictionary<int, Dictionary<int, List<DialogueData>>> _script;

        private async void Start()
        {
            ui = FindFirstObjectByType<DialogueView>();
            _dataManager = new GameDataManager();

            var handler = new DialogueDataHandler(sheetURL, addressableJsonKey: "");
            await _dataManager.AddAsync<DialogueData>(handler, forceUpdate: false);

            var all = _dataManager.GetAll<DialogueData>().ToList();

            _script = all
                .GroupBy(d => d.ID)
                .ToDictionary(
                    g => g.Key,
                    g => g.GroupBy(d => d.Branch)
                          .ToDictionary(bg => bg.Key, bg => bg.ToList())
                    );

            ui.FadeOutBlackScreen();
            await PlayBranch(ID: 1, branch: 0);
        }

        private async UniTask PlayBranch(int ID, int branch)
        {
            if (!_script.TryGetValue(ID, out var branchDict) || !branchDict.TryGetValue(branch, out var lines))
            {
                return;
            }

            foreach (var data in lines)
            {
                ui.UpdateDialogue(data.Name, data.Dialogue);

                if (!string.IsNullOrEmpty(data.BG)) await ui.SetBG(data.BG);
                if (!string.IsNullOrEmpty(data.CG)) await ui.SetCG(data.CG);

                bool keepGoing = await HandleCommandAsync(data, ID);
                if (!keepGoing)
                    return;
            }
        }

        private async UniTask<bool> HandleCommandAsync(DialogueData dialogueData, int ID)
        {
            switch (dialogueData.Command)
            {
                case "SetCharacter":
                    if (dialogueData.CharacterSide == "Left")
                        await ui.SetLeftCharacter(dialogueData.CharacterKey);
                    else
                        await ui.SetRightCharacter(dialogueData.CharacterKey);

                    if (dialogueData.LoadMode == "NoWait")
                        return true;
                    goto case "Say";

                case "Say":
                    HighlightCharacters(dialogueData.HightLight);
                    await ui.WaitForInput();
                    return true;

                case "CleanCharacter":
                    ui.FadeOutCharacter();
                    return true;

                case "SetOption":
                    int choice = await ui.ShowOption(dialogueData.dialogueBranches);
                    await PlayBranch(ID, choice + 1);
                    return false;

                default:
                    Debug.LogWarning($"Unknown Command: {dialogueData.Command}");
                    return true;
            }
        }

        public void HighlightCharacters(string mode)
        {
            switch (mode)
            {
                case "Left":
                    ui.HighlightLeftCharacter();
                    ui.DeHighlightRightCharacter();
                    break;
                case "Right":
                    ui.HighlightRightCharacter();
                    ui.DeHighlightLeftCharacter();
                    break;
                case "None":
                    ui.DeHighlightAllCharacter(); 
                    break;
                case "All":
                default:
                    ui.HighlightAllCharacter();
                    break;
            }
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using System.Data;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueView ui;
        [SerializeField] private string sheetURL;

        private CancellationTokenSource _playCts;

        private GameDataManager _dataManager;
        private Dictionary<int, Dictionary<int, List<DialogueData>>> _script;

        public bool pauseForInput { get; set; } = false;

        public event Action onDialougeCompelete;

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
            await PlayBranch(ID: 1, branch: 0).SuppressCancellationThrow();
        }

        private async UniTask PlayBranch(int ID, int branch)
        {
            _playCts?.Cancel();
            _playCts = new CancellationTokenSource();
            var token = _playCts.Token;

            if (_script == null)
            {
                Debug.LogError("DialogueManager: _script not initailiz!");
                return;
            }

            if (!_script.TryGetValue(ID, out var branchDict) || !branchDict.TryGetValue(branch, out var lines))
            {
                onDialougeCompelete?.Invoke();
                return;
            }

            foreach (var data in lines)
            {
                token.ThrowIfCancellationRequested();

                ui.UpdateDialogue(data.Name, data.Dialogue);

                if (!string.IsNullOrEmpty(data.BG)) 
                    await ui.SetBG(data.BG).AttachExternalCancellation(token);

                if (!string.IsNullOrEmpty(data.CG)) 
                    await ui.SetCG(data.CG).AttachExternalCancellation(token);

                bool keepGoing = await HandleCommandAsync(data, ID).AttachExternalCancellation(token);
                if (!keepGoing)
                    return;
            }

            onDialougeCompelete?.Invoke();
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

        public void CancelCurrent()
        {
            _playCts?.Cancel();
        }

        private void HighlightCharacters(string mode)
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
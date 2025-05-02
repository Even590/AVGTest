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
        private List<DialogueData> dialogueList;
        private int currentDialogueIndex = 0;
        private GameDataManager _dataManager;

        public System.Action<DialogueData> OnChangeNextDialogue;

        private async void Start()
        {
            ui = FindFirstObjectByType<DialogueView>();
            _dataManager = new GameDataManager();

            var handler = new DialogueDataHandler(sheetURL);
            await _dataManager.AddAsync<DialogueData>(handler, isForceUpdate: false);

            var array = _dataManager.Get<DialogueData>();
            dialogueList = array.ToList();

            await UniTask.Yield();

            ui.FadeOutBlackScreen();
            ui.FadeInCharacter();
            _ = ShowDialogue();
        }

        private async Task ShowDialogue()
        {
            if (currentDialogueIndex >= dialogueList.Count)
            {
                OnChangeNextDialogue?.Invoke(null);
                ui.FadeInBlackScreen();
                return;
            }

            var dlg = dialogueList[currentDialogueIndex];
            OnChangeNextDialogue?.Invoke(dlg);

            if (!string.IsNullOrEmpty(dlg.Command))
                ExecuteCommand(dlg);

            if (!string.IsNullOrEmpty(dlg.BG))
                await ui.SetBG(dlg.BG);

            if (!string.IsNullOrEmpty(dlg.CG))
                await ui.SetBG(dlg.CG);
        }

        private void ExecuteCommand(DialogueData dialogueData)
        {
            switch (dialogueData.Command)
            {
                case "SetCharacter":
                    HandleSetCharater(dialogueData);
                    break;

                case "Say":
                    OnChangeNextDialogue?.Invoke(dialogueData);
                    Process(dialogueData);
                    break;
                case "CleanCharacter":
                    ui.FadeOutCharacter();
                    break;

                default:
                    Debug.LogWarning($"UnknowCommand:{dialogueData.Command}");
                    break;
            }
        }

        private void HandleSetCharater(DialogueData dialogue)
        {
            if (dialogue.CharacterSide == "Left")
            {
                _ = ui.SetLeftCharacter(dialogue.CharacterKey);

                if (dialogue.LoadMode == "NoWait")
                {
                    NextDialogue();
                }
            }
            if (dialogue.CharacterSide == "Right")
            {
                _ = ui.SetRightCharacter(dialogue.CharacterKey);

                if (dialogue.LoadMode == "NoWait")
                {
                    NextDialogue();
                }
            }
        }

        public void NextDialogue()
        {
            currentDialogueIndex++;
            _ = ShowDialogue();
        }

        public void StartDialogue()
        {
            currentDialogueIndex = 0;
            _ = ShowDialogue();
        }

        public void Process(DialogueData dialogueData)
        {
            switch (dialogueData.HightLight)
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
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;

namespace AVGTest.Asset.Script.DialogueSystem
{

    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueView ui;
        [SerializeField] private string sheetURL;
        private List<DialogueData> dialogueList = new List<DialogueData>();

        private int currentDialogueIndex = 0;

        public System.Action<DialogueData> OnChangeNextDialogue;

        private async void Start()
        {
            ui = FindFirstObjectByType<DialogueView>();
            ui.FadeOutBlackScreen();
            ui.FadeInCharacter();

            await LoadDialogueData();
            _ = ShowDialogue();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnClickForceRefresh();
            }
        }
        public async UniTask<string> DownloadCsvHttpClient(string url)
        {
            using (var cilent = new HttpClient())
            {
                HttpResponseMessage resp = await cilent.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                string CsvText = await resp.Content.ReadAsStringAsync();
                return CsvText;
            }
        }

        private async UniTask LoadDialogueData(bool isForceUpdate = false)
        {
            string jsonPath = Path.Combine(Application.persistentDataPath, "dialogue.json");

            if (File.Exists(jsonPath) && !isForceUpdate) 
            {
                Debug.Log("Json is being found, use json file");
                string json = await File.ReadAllTextAsync(jsonPath);
                dialogueList = JsonHelper.FromJson<DialogueData>(json);
                return;
            }

            string csvText = await DownloadCsvHttpClient(sheetURL);

            dialogueList.Clear();
            ParseCSV(csvText);

            string outJson = JsonHelper.ToJson(dialogueList, true);
            await File.WriteAllTextAsync(jsonPath, outJson);
            Debug.Log($"Exported Json to {jsonPath}");
        }

        private void ParseCSV(string csv)
        {
            string[] lines = csv.Split('\n');

            Debug.Log($"[ParseCSV] total lines (含 header): {lines.Length}");

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] fields = lines[i].Split(',');

                if (fields.Length < 10)
                {
                    Debug.LogWarning($"Line {i} has insufficient fields ({fields.Length}); skipped: {lines[i]}");
                    continue;
                }

                for (int f = 0; f < fields.Length; f++)
                {
                    fields[f] = fields[f].Trim();
                }

                int id;

                if (!int.TryParse(fields[0], out id))
                {
                    Debug.LogWarning($"Failed to parse ID on line {i}; skipping! Content was: [{fields[0]}]");
                    continue;
                }

                DialogueData data = new DialogueData
                {
                    ID = id,
                    Command = fields[1],
                    CharacterSide = fields[2],
                    CharacterKey = fields[3],
                    LoadMode = fields[4],
                    HightLight = fields[5],
                    BG = fields[6],
                    CG = fields[7],
                    Name = fields[8],
                    Dialogue = fields[9],
                };

                dialogueList.Add(data);
            }
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

        public async void OnClickForceRefresh()
        {
            await LoadDialogueData(isForceUpdate: true);
            Debug.Log("Force update completed");
            currentDialogueIndex = 0;
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
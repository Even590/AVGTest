using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUIController ui;
        [SerializeField] private string sheetURL;
        private string CacheFileName = "DialogueCache.csv";
        private List<DialogueData> dialogueList = new List<DialogueData>();

        private int currentDialogueIndex = 0;

        public System.Action<DialogueData> OnChangeNextDialogue;

        private async void Start()
        {
            ui = FindFirstObjectByType<DialogueUIController>();

            await LoadDialogueData();
            ShowDialogue();
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
            string cachePath = Path.Combine(Application.persistentDataPath, CacheFileName);

            string csvText = "";

            int cachevalidDays = 1;

            bool needUpdate = true;

            if (File.Exists(cachePath))
            {
                Debug.Log("Found local cache file, checking whether it’s expired.");

                if (!isForceUpdate)
                {
                    System.DateTime lastWriteTime = File.GetLastWriteTime(cachePath);
                    System.TimeSpan timeSinceLastUpdate = System.DateTime.Now - lastWriteTime;

                    Debug.Log($"快取檔案路徑：{cachePath}");

                    Debug.Log($"Cache file was last updated at {lastWriteTime}; it’s been {timeSinceLastUpdate} since then.");

                    if (timeSinceLastUpdate.Days <= cachevalidDays)
                    {
                        Debug.Log("Cache file is still valid; continuing to use it!");
                        csvText = await File.ReadAllTextAsync(cachePath);
                        needUpdate = false;
                    }
                    else
                    {
                        Debug.Log("Cache file has expired; proceeding to download phase!");
                    }
                }
                else
                {
                    Debug.Log("Force update requested; now entering download phase!");
                }

            }

            if (needUpdate || isForceUpdate)
            {
                Debug.Log("Performing network download");
                csvText = await DownloadCsvHttpClient(sheetURL);
                await File.WriteAllTextAsync(cachePath, csvText);
                Debug.Log($"Cache saved at {cachePath}");
            }

            dialogueList.Clear();
            ParseCSV(csvText);
        }

        private void ParseCSV(string csv)
        {
            string[] lines = csv.Split('\n');

            Debug.Log($"[ParseCSV] total lines (含 header): {lines.Length}");

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] fields = lines[i].Split(',');

                if (fields.Length < 9)
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
                    Arg1 = fields[2],
                    Arg2 = fields[3],
                    Arg3 = fields[4],
                    BG = fields[5],
                    CG = fields[6],
                    Speaker = fields[7],
                    Line = fields[8],
                };

                dialogueList.Add(data);
            }
        }

        private async Task ShowDialogue()
        {
            if (currentDialogueIndex >= dialogueList.Count)
            {
                OnChangeNextDialogue?.Invoke(null);
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
                    break;

                default:
                    Debug.LogWarning($"UnknowCommand:{dialogueData.Command}");
                    break;
            }
        }

        private void HandleSetCharater(DialogueData dialogue)
        {
            if (dialogue.Arg1 == "Left")
            {
                _ = ui.SetLeftCharacter(dialogue.Arg2);

                if (dialogue.Arg3 == "NoWait")
                {
                    NextDialogue();
                }
            }
            if (dialogue.Arg1 == "Right")
            {
                _ = ui.SetRightCharacter(dialogue.Arg2);

                if (dialogue.Arg3 == "NoWait")
                {
                    NextDialogue();
                }
            }
        }

        public void NextDialogue()
        {
            currentDialogueIndex++;
            ShowDialogue();
        }

        public async void OnClickForceRefresh()
        {
            await LoadDialogueData(isForceUpdate: true);
            Debug.Log("Force update completed");
            currentDialogueIndex = 0;
            ShowDialogue();
        }

        public void StartDialogue()
        {
            currentDialogueIndex = 0;
            ShowDialogue();
        }
    }
}
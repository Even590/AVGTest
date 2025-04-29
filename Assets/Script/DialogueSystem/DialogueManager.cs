using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.IO;

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
            await LoadDialogueData();
            ShowDialogue();
        }

        private void Update()
        {
            //測試用強制更新網路CSV檔
            if (Input.GetKeyDown(KeyCode.K))
            {
                OnClickForceRefresh();
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
                Debug.Log("找到本地快取檔案，檢查是否過期");

                if (!isForceUpdate)
                {
                    System.DateTime lastWriteTime = File.GetLastWriteTime(cachePath);
                    System.TimeSpan timeSinceLastUpdate = System.DateTime.Now - lastWriteTime;

                    Debug.Log($"快取檔案上次更新時間{lastWriteTime}，距離現在經過{timeSinceLastUpdate}");

                    if (timeSinceLastUpdate.Days <= cachevalidDays)
                    {
                        Debug.Log("快取檔案仍有效，可以接著使用！");
                        csvText = await File.ReadAllTextAsync(cachePath);
                        needUpdate = false;
                    }
                    else
                    {
                        Debug.Log("快取檔案已過期，進入下載階段！");
                    }
                }
                else
                {
                    Debug.Log("已收到強制更新要求，現在進入下載階段！");
                }

            }
            
            if(needUpdate || isForceUpdate)
            {
                Debug.Log("執行網路下載");
                using (UnityWebRequest www = UnityWebRequest.Get(sheetURL))
                {
                    await www.SendWebRequest();

                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("下載失敗！請重新尋找資料！");
                        return;
                    }
                    csvText = www.downloadHandler.text;

                    await File.WriteAllTextAsync(cachePath, csvText);
                    Debug.Log($"已儲存快取{cachePath}");
                }
            }

            ParseCSV(csvText);
        }

        private void ParseCSV(string csv)
        {
            string[] lines = csv.Split('\n');

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;

                string[] fields = lines[i].Split(',');

                if (fields.Length < 7)
                {
                    Debug.LogWarning($"第 {i} 行欄位數不足 ({fields.Length})，已跳過：{lines[i]}");
                    continue;
                }

                for (int f = 0; f < fields.Length; f++)
                {
                    fields[f] = fields[f].Trim();
                }

                int id;

                if (!int.TryParse(fields[0], out id))
                {
                    Debug.LogWarning($"第{i}行ID解析失敗，跳過！內容是：[{fields[0]}]");
                    continue;
                }

                DialogueData data = new DialogueData
                {
                    ID = id,
                    Command = fields[1],
                    Arg1 = fields[2],
                    Arg2 = fields[3],
                    Arg3 = fields[4],
                    Speaker = fields[5],
                    Line = fields[6],
                };

                dialogueList.Add(data);
            }
        }

        private void ShowDialogue()
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
                    Debug.LogWarning($"未知指令:{dialogueData.Command}");
                    break;
            }
        }

        private void HandleSetCharater(DialogueData dialogue)
        {
            if(dialogue.Arg1 == "Left")
            {
                _ = ui.SetLeftCharacter(dialogue.Arg2);

                if(dialogue.Arg3 == "NoWait")
                {
                    NextDialogue();
                }
            }
            if(dialogue.Arg1 == "Right")
            {
                _ = ui.SetRightCharacter(dialogue.Arg2);

                if(dialogue.Arg3 == "NoWait")
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
            await LoadDialogueData(isForceUpdate : true);
            Debug.Log("強制更新已完成");
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

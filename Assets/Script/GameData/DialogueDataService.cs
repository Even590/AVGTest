using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AVGTest.Asset.Script
{
    public class DialogueDataService
    {
        private readonly string _sheetURL;
        private readonly string _addressableJsonKey;
        private readonly HttpClient _httpClient = new HttpClient();

        public DialogueDataService(string sheetURL, string addressableJsonKey = null)
        {
            _sheetURL = sheetURL;
            _addressableJsonKey = string.IsNullOrWhiteSpace(addressableJsonKey)
                ? null
                : addressableJsonKey;
        }

        public async UniTask<List<DialogueData>> GetDialogueListAsync()
        {
            if (!string.IsNullOrEmpty(_addressableJsonKey))
            {
                try
                {
                    var locHandle = Addressables.LoadResourceLocationsAsync(_addressableJsonKey);
                    await locHandle.Task;

                    if (locHandle.Status == AsyncOperationStatus.Succeeded && locHandle.Result.Count > 0)
                    {
                        var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableJsonKey);
                        await handle.Task;
                        if(handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                        {
                            var list = JsonHelper.FromJson<DialogueData>(handle.Result.text);
                            if (list != null && list.Count > 0)
                                return list;
                        }
                    }
                    Addressables.Release(locHandle);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DialogueDataService] Addressables Load Ex：{ex.Message}");
                }
            }

            Debug.Log("[DialogueDataService] Download Form CSV");
            string csv = await DownloadCsvAsync();
            return ParseCSV(csv);
        }

        private async UniTask<string> DownloadCsvAsync()
        {
            var resp = await _httpClient.GetAsync(_sheetURL);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }

        private List<DialogueData> ParseCSV(string csv)
        {
            var result = new List<DialogueData>();
            var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                var f = lines[i].Split(',');
                if (f.Length < 11) continue;

                if (!int.TryParse(f[0].Trim(), out int id)) continue;
                if (!int.TryParse(f[1].Trim(), out int branch)) continue;

                var data = new DialogueData
                {
                    ID = id,
                    Branch = branch,
                    Command = f[2].Trim(),
                    CharacterSide = f[3].Trim(),
                    CharacterKey = f[4].Trim(),
                    LoadMode = f[5].Trim(),
                    HightLight = f[6].Trim(),
                    BG = f[7].Trim(),
                    CG = f[8].Trim(),
                    Name = f[9].Trim(),
                    Dialogue = f[10].Trim(),
                    dialogueBranches = null
                };

                if (data.Command == "SetOption" && f.Length >= 15)
                {
                    data.dialogueBranches = new List<DialogueBranch>
                    {
                        new DialogueBranch {
                            Text         = f[11].Trim(),
                            TargetBranch = int.Parse(f[12].Trim())
                        },
                    new DialogueBranch {
                            Text         = f[13].Trim(),
                            TargetBranch = int.Parse(f[14].Trim())
                        },
                    };
                }

                result.Add(data);
            }

            return result;
        }
    }
}

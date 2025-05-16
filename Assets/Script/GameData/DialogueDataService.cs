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
            _addressableJsonKey = addressableJsonKey;
        }

        public async UniTask<List<DialogueData>> GetDialogueListAsync()
        {
            if (!string.IsNullOrEmpty(_addressableJsonKey))
            {
                try
                {
                    var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableJsonKey);
                    await handle.Task; 

                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    {
                        Debug.Log($"[DialogueDataService] Form Addressables reading JSON：{_addressableJsonKey}");
                        var json = handle.Result.text;
                        Addressables.Release(handle);
                        var list = JsonHelper.FromJson<DialogueData>(json);
                        if (list != null && list.Count > 0)
                            return list;
                    }
                    else
                    {
                        Debug.LogWarning($"[DialogueDataService] Addressables key Can't Find：{_addressableJsonKey}");
                    }
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
                var fields = lines[i].Split(',');
                if (fields.Length < 10) continue;
                // trim & parse...
                if (!int.TryParse(fields[0], out int id)) continue;
                result.Add(new DialogueData
                {
                    ID = id,
                    Command = fields[1].Trim(),
                    CharacterSide = fields[2].Trim(),
                    CharacterKey = fields[3].Trim(),
                    LoadMode = fields[4].Trim(),
                    HightLight = fields[5].Trim(),
                    BG = fields[6].Trim(),
                    CG = fields[7].Trim(),
                    Name = fields[8].Trim(),
                    Dialogue = fields[9].Trim()
                });
            }
            return result;
        }
    }
}

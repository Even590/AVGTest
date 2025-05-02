using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using UnityEngine;


namespace AVGTest.Asset.Script
{
    public class DialogueDataService
    {
        private readonly string _sheetURL;
        private readonly string _jsonFilePath;
        private readonly HttpClient _httpClient = new HttpClient();

        public DialogueDataService(string sheetURL, string jsonFileName = "dialogue.json")
        {
            _sheetURL = sheetURL;
            _jsonFilePath = Path.Combine(Application.persistentDataPath, jsonFileName);
        }

        public async UniTask<string> DownloadCsvHttpClient()
        {
            var resp = await _httpClient.GetAsync(_sheetURL);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }

        public async UniTask<List<DialogueData>> GetDialogueListAsync(bool forceUpdate = false)
        {
            if (File.Exists(_jsonFilePath) && !forceUpdate)
            {
                try
                {
                    Debug.Log("Find json file, now use json");
                    string json = await File.ReadAllTextAsync(_jsonFilePath);
                    var list = JsonHelper.FromJson<DialogueData>(json);
                    if (list != null && list.Count > 0)
                    {
                        Debug.Log("[DialogueDataService] 成功從 JSON 快取讀取資料");
                        return list;
                    }
                    else
                    {
                        Debug.LogWarning("[DialogueDataService] JSON 快取為空，準備退回下載 CSV");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[DialogueDataService] 解析 JSON 快取失敗：{ex.Message}，準備退回下載 CSV");
                }

                File.Delete(_jsonFilePath);
                
            }

            string csv = await DownloadCsvHttpClient();
            var listcsv = ParseCSV(csv);

            string outJson = JsonHelper.ToJson(listcsv, prettyPrint: true);
            await File.WriteAllTextAsync(_jsonFilePath, outJson);
            Debug.Log($"File already save cache json");

            return listcsv;
        }


        private List<DialogueData> ParseCSV(string csv)
        {
            var result = new List<DialogueData>();
            var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 1; i < lines.Length; i++)
            {
                var fields = lines[i].Split(',');
                if (fields.Length < 10) continue;
                for (int j = 0; j < fields.Length; j++)
                    fields[j] = fields[j].Trim();
                if (!int.TryParse(fields[0], out int id)) continue;

                result.Add(new DialogueData
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
                    Dialogue = fields[9]
                });
            }
            return result;
        }
    }
}

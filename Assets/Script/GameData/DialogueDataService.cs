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
                string json = await File.ReadAllTextAsync(_jsonFilePath);
                return JsonHelper.FromJson<DialogueData>(json);
            }

            string csv = await DownloadCsvHttpClient();
            var list = ParseCSV(csv);

            return list;
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

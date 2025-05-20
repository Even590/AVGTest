using AVGTest.Asset.Script.DialogueSystem;
using Cysharp.Threading.Tasks;
using System;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AVGTest.Asset.Script
{
    public class DialogueScriptTest : MonoBehaviour
    {
        [Header("測試UI")]
        [Tooltip("輸入章節或章節分支(如1或2.1)")]
        [SerializeField] private TMP_InputField command_Input;
        [SerializeField] private Button         runButton;

        private DialogueManager _manager;
        private MethodInfo      _playBranchMethod;

        // Start is called before the first frame update
        void Awake()
        {
            _manager = FindObjectOfType<DialogueManager>();
            if( _manager == null)
            {
                Debug.Log("Can't Find DialogueManager!");
                enabled = false;
                return;
            }

            _playBranchMethod = typeof(DialogueManager)
                .GetMethod("PlayBranch",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), typeof(int) },
                    null);

            if( _playBranchMethod == null )
            {
                Debug.LogError("Can't Method PlayBranch");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            runButton.onClick.AddListener(() => RunCommand().Forget());
        }

        private async UniTask RunCommand()
        {
            var text = command_Input.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            // 解析 "章節" 或 "章節.分支"
            var parts = text.Split('.');
            if (!int.TryParse(parts[0], out int chapter))
            {
                Debug.LogWarning($"無效的章節格式：{parts[0]}");
                return;
            }
            int branch = 0;
            if (parts.Length > 1 && !int.TryParse(parts[1], out branch))
            {
                Debug.LogWarning($"無效的分支格式：{parts[1]}");
                return;
            }

            try
            {
                // 透過反射呼叫 PlayBranch
                var result = _playBranchMethod.Invoke(_manager, new object[] { chapter, branch });
                // PlayBranch 回傳 UniTask，所以這裡 cast 並 await
                if (result is UniTask ut)
                {
                    await ut;
                }
                else if (result is Task t)
                {
                    await t;
                }
                else
                {
                    // 如果方法簽名變動，不會回傳我們預期的類型
                    Debug.LogWarning("PlayBranch 回傳值非 UniTask/Task，無法等待。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"執行 PlayBranch 失敗：{ex.Message}");
            }
        }
    }
}

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
        [SerializeField] private GameObject TesterView;
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
            TesterView.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Equals))
            {
                bool now = !TesterView.activeSelf;
                TesterView.SetActive(now);

                _manager.pauseForInput = now;
            }
        }

        private async UniTask RunCommand()
        {
            _manager.CancelCurrent();

            var text = command_Input.text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            var parts = text.Split('.');
            if (!int.TryParse(parts[0], out int chapter))
            {
                Debug.LogWarning($"Invalid chapter format：{parts[0]}");
                return;
            }
            int branch = 0;
            if (parts.Length > 1 && !int.TryParse(parts[1], out branch))
            {
                Debug.LogWarning($"Invalid branch format：{parts[1]}");
                return;
            }

            try
            {
                var result = _playBranchMethod.Invoke(_manager, new object[] { chapter, branch });
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
                    Debug.LogWarning("PlayBranch returned a non-UniTask/Task value and cannot be awaited.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to execute PlayBranch：{ex.Message}");
            }
        }
    }
}

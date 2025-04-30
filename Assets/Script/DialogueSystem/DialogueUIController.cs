using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueUIController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private Image LeftCharaterImage;
        [SerializeField] private Image RightCharaterImage;
        [SerializeField] private Image BG;
        [SerializeField] private Image CG;

        private DialogueManager dialogueManager;

        void Start()
        {
            dialogueManager = FindAnyObjectByType<DialogueManager>();

            if (dialogueManager == null) 
            {
                Debug.LogError("Can't find DialogueManager！");
                return;
            }

            dialogueManager.OnChangeNextDialogue += UpdateDialogue;

            dialogueManager.StartDialogue();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                dialogueManager.NextDialogue();
            }
        }

        private void UpdateDialogue(DialogueData data)
        {
            if (data == null) 
            {
                Debug.Log("Dialogue is finish");
                nameText.text = "";
                dialogueText.text = "";
                return;
            }

            nameText.text = data.Speaker;
            dialogueText.text = data.Line;
        }

        public async UniTask<Sprite> LoadCharacterSpriteAsync(string spriteName)
        {
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>($"Character/{spriteName}");
            Sprite sprite = await handle.Task;

            return sprite;
        }

        public async Task SetLeftCharacter(string spriteName, Action onCompleted = null)
        {
            Sprite sprite = await LoadCharacterSpriteAsync(spriteName);
            SetLeftCharacter(sprite, onCompleted);
        }

        public void SetLeftCharacter(Sprite sprite, Action onCompleted = null)
        {
            LeftCharaterImage.sprite = sprite;
            LeftCharaterImage.color = Color.white;

            onCompleted?.Invoke();
        }

        public async Task SetRightCharacter(string spriteName, Action onCompleted = null)
        {
            Sprite sprite = await LoadCharacterSpriteAsync(spriteName);
            SetRightCharacter(sprite, onCompleted);
        }

        public void SetRightCharacter(Sprite sprite, Action onCompleted = null)
        {
            RightCharaterImage.sprite = sprite;
            RightCharaterImage.color = Color.white;

            onCompleted?.Invoke();
        }

        public async UniTask<Sprite> LoadBGSpriteAsync(string spriteName)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"BG/{spriteName}");
            return await handle.Task;
        }

        public async Task SetBG(string spriteName, Action onCompleted = null)
        {
            var sprite = await LoadBGSpriteAsync(spriteName);
            BG.sprite = sprite;
            BG.color = Color.white;
            onCompleted?.Invoke();
        }

        public async UniTask<Sprite> LoadCGSpriteAsync(string spriteName)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"CG/{spriteName}");
            return await handle.Task;
        }

        public async Task SetCG(string spriteName, Action onCompleted = null)
        {
            var sprite = await LoadCGSpriteAsync(spriteName);
            CG.sprite = sprite;
            CG.color = Color.white;
            onCompleted?.Invoke();
        }
    }
}

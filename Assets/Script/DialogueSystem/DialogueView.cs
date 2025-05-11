using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using DG.Tweening;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueView : MonoBehaviour
    {
        [Header("UI物件")]
        [Tooltip("對話介面")]
        [SerializeField] private GameObject dialogueUI;
        [Tooltip("CG介面")]
        [SerializeField] private GameObject CGUI;
        [Header("對話框，包含普通對話介面與CG介面")]
        [Tooltip("對話介面用名稱TEXT")]
        [SerializeField] private TextMeshProUGUI nameText;
        [Tooltip("對話介面用劇本TEXT")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [Tooltip("CG介面用名稱TEXT")]
        [SerializeField] private TextMeshProUGUI CGnameText;
        [Tooltip("CG介面用劇本TEXT")]
        [SerializeField] private TextMeshProUGUI CGdialogueText;
        [Header("左右人物物件與插畫位置")]
        [SerializeField] private Transform leftCharaterTransfrom;
        [SerializeField] private Transform rightCharaterTransfrom;
        [SerializeField] private Image leftCharaterImage;
        [SerializeField] private Image rightCharaterImage;
        [Header("背景圖位置")]
        [SerializeField] private Image BG;
        [Header("CG插畫位置")]
        [SerializeField] private Image CG;
        [Header("黑幕")]
        [SerializeField] private Image blackScreen;

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

            CGUI.SetActive(false);
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

            nameText.text = data.Name;
            dialogueText.text = data.Dialogue;
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
            leftCharaterImage.sprite = sprite;
            leftCharaterImage.color = Color.white;
            leftCharaterImage.CrossFadeAlpha(1f, 0.5f, true);

            onCompleted?.Invoke();
        }

        public async Task SetRightCharacter(string spriteName, Action onCompleted = null)
        {
            Sprite sprite = await LoadCharacterSpriteAsync(spriteName);
            SetRightCharacter(sprite, onCompleted);
        }

        public void SetRightCharacter(Sprite sprite, Action onCompleted = null)
        {
            rightCharaterImage.sprite = sprite;
            rightCharaterImage.color = Color.white;
            rightCharaterImage.CrossFadeAlpha(1f, 0.5f, true);

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

        public void FadeInCharacter()
        {
            leftCharaterImage.CrossFadeAlpha(1f, 0.5f, true);
            rightCharaterImage.CrossFadeAlpha(1f, 0.5f, true);
        }

        public void FadeOutCharacter()
        {
            leftCharaterImage.CrossFadeAlpha(0f, 0.5f, true);
            rightCharaterImage.CrossFadeAlpha(0f, 0.5f, true);
        }

        public void FadeInBlackScreen()
        {
            
            blackScreen.CrossFadeAlpha(1f, 2.5f, true);
        }

        public void FadeOutBlackScreen()
        {
            var c = blackScreen.color;
            c.a = 1f;
            blackScreen.color = c;

            blackScreen.CrossFadeAlpha(0f, 2.5f, true);
        }

        public void HighlightLeftCharacter(Action onCompleted = null)
        {
            HighlightCharacter(leftCharaterImage, leftCharaterTransfrom, onCompleted);
        }

        public void HighlightRightCharacter(Action onCompleted = null)
        {
            HighlightCharacter(rightCharaterImage, rightCharaterTransfrom, onCompleted);
        }

        public void HighlightCharacter(Image image, Transform root, Action onCompleted = null)
        {
            root.DOScale(1f, 0.5f);
            image.DOColor(new Color(1f, 1f, 1f, 1f), 0.5f).OnComplete(() => onCompleted?.Invoke());
        }

        public void HighlightAllCharacter(Action onCompleted = null)
        {
            HighlightCharacter(rightCharaterImage, rightCharaterTransfrom, onCompleted);
            HighlightCharacter(leftCharaterImage, leftCharaterTransfrom, onCompleted);
        }

        public void DeHighlightLeftCharacter(Action onCompleted = null)
        {
            DeHighlightCharacter(leftCharaterImage, leftCharaterTransfrom, onCompleted);
        }

        public void DeHighlightRightCharacter(Action onCompleted = null)
        {
            DeHighlightCharacter(rightCharaterImage, rightCharaterTransfrom, onCompleted);
        }

        public void DeHighlightCharacter(Image image, Transform root, Action onCompleted = null)
        {
            root.DOScale(0.9f, 0.5f);
            image.DOColor(Color.gray, 0.5f).OnComplete(() => onCompleted?.Invoke());
        }

        public void DeHighlightAllCharacter(Action onCompleted = null)
        {
            DeHighlightCharacter(rightCharaterImage, rightCharaterTransfrom, onCompleted);
            DeHighlightCharacter(leftCharaterImage, leftCharaterTransfrom, onCompleted);
        }
    }
}

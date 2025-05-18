using UnityEngine.AddressableAssets;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;

namespace AVGTest.Asset.Script.DialogueSystem
{
    public class DialogueView : MonoBehaviour
    {
        [Header("UI物件")]
        [Tooltip("對話介面")]
        [SerializeField] private GameObject dialogueUI;
        [Header("對話框，包含普通對話介面與CG介面")]
        [Tooltip("對話介面用名稱TEXT")]
        [SerializeField] private TextMeshProUGUI nameText;
        [Tooltip("對話介面用劇本TEXT")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [Header("左右人物物件與插畫位置")]
        [SerializeField] private Transform leftCharaterTransfrom;
        [SerializeField] private Transform rightCharaterTransfrom;
        [SerializeField] private Image leftCharaterImage;
        [SerializeField] private Image rightCharaterImage;
        [Header("BG跟CG圖片位置")]
        [SerializeField] private Image CGImage;

        [Header("黑幕")]
        [SerializeField] private Image blackScreen;

        [Header("選項面板")]
        [Tooltip("選項介面")]
        [SerializeField] private GameObject optionPanel;
        [Tooltip("選擇按鈕A")]
        [SerializeField] private Button optionButtonA;
        [Tooltip("選擇按鈕B")]
        [SerializeField] private Button optionButtonB;
        [Tooltip("選擇按鈕A的文字")]
        [SerializeField] private TextMeshProUGUI optionTextA;
        [Tooltip("選擇按鈕B的文字")]
        [SerializeField] private TextMeshProUGUI optionTextB;

        [Header("主畫面")]
        [Tooltip("主介面物件")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private Button[] buttons;


        private DialogueManager dialogueManager;

        public void UpdateDialogue(string name, string dialogue)
        {
            optionPanel.SetActive(false);
            nameText.text = name;
            dialogueText.text = dialogue;
        }

        public UniTask WaitForInput()
        {
            return UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space) ||Input.GetMouseButtonDown(0));
        }

        public UniTask<int> ShowOption(List<DialogueBranch> branches)
        {
            var tcs = new UniTaskCompletionSource<int>();

            optionPanel.SetActive(true);
            optionTextA.text = branches[0].Text;
            optionTextB.text = branches[1].Text;

            void OnA()
            {
                CleanUp();
                tcs.TrySetResult(0);
            }

            void OnB()
            {
                CleanUp();
                tcs.TrySetResult(1);
            }

            void CleanUp()
            {
                optionButtonA.onClick.RemoveListener(OnA);
                optionButtonB.onClick.RemoveListener(OnB);
                optionPanel.SetActive(false);
            }

            optionButtonA.onClick.AddListener(OnA);
            optionButtonB.onClick.AddListener(OnB);
            return tcs.Task;
        }

        public async Task SetLeftCharacter(string spriteName)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"Character/{spriteName}");
            var sprite = await handle.Task;
            leftCharaterImage.sprite = sprite;
            leftCharaterImage.CrossFadeAlpha(1f, 0.5f, true);
        }

        public async Task SetRightCharacter(string spriteName)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"Character/{spriteName}");
            var sprite = await handle.Task;
            rightCharaterImage.sprite = sprite;
            rightCharaterImage.CrossFadeAlpha(1f, 0.5f, true);
        }

        public async UniTask<Sprite> LoadBGSpriteAsync(string spriteName)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>($"BG/{spriteName}");
            return await handle.Task;
        }

        public async Task SetBG(string spriteName, Action onCompleted = null)
        {
            var sprite = await LoadBGSpriteAsync(spriteName);
            CGImage.sprite = sprite;
            CGImage.color = Color.white;
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
            FadeOutCharacter();
            CGImage.sprite = sprite;
            CGImage.color = Color.white;
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

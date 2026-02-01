using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace UnityADV.UI
{
    /// <summary>
    /// ADV UI控制器
    /// 管理所有UI元素的显示和交互
    /// </summary>
    public class ADVUIController : MonoBehaviour
    {
        [Header("Text Display")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _dialogueWindowText;
        [SerializeField] private GameObject _keyWaitIcon;
        [SerializeField] private RectTransform _keyWaitIconTransform;
        [SerializeField] private Tween[] _keyWaitIconTween = new Tween[2];
        public GameObject DialoguePanel => _dialoguePanel;

        [Header("Layers")]
        [SerializeField] private Transform _backgroundLayer;    // Level 1
        [SerializeField] private Transform _characterLayer;     // Level 2
        [SerializeField] private Transform _overlayLayer;       // Level 3
        [SerializeField] private Transform _wipeLayer;          // Level 4

        [Header("Menu Panel")]
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private GameObject[] _menuButtonArray;

        [Header("Novel Panel")]
        [SerializeField] private GameObject _novelModeFilter;
        [SerializeField] private GameObject _novelModeWindow;
        [SerializeField] private TextMeshProUGUI _novelModeText;
        [SerializeField] private GameObject _novelModeKeyIcon;

        [Header("Effects")]
        [SerializeField] private GameObject _sepiaFilter;
        [SerializeField] private GameObject _fadeFilter;
        private Tween _fadeTween;

        [Header("Log Panel")]
        [SerializeField] private GameObject _backLogPanel;

        // Properties
        public Transform BackgroundLayer => _backgroundLayer;
        public Transform CharacterLayer => _characterLayer;
        public Transform OverlayLayer => _overlayLayer;
        public TextMeshProUGUI DialogueWindowText => _dialogueWindowText;
        public TextMeshProUGUI CharacterNameText => _characterNameText;
        public GameObject KeyWaitIcon => _keyWaitIcon;

        #region Text Window Control

        /// <summary>
        /// 显示文本窗口
        /// </summary>
        public void ShowTextWindow(bool isShow)
        {
            if (_dialoguePanel != null)
            {
                _dialoguePanel.SetActive(isShow);
            }
        }

        /// <summary>
        /// 设置对话文本
        /// </summary>
        public void SetDialogueText(string text)
        {
            if (_dialogueWindowText != null)
                _dialogueWindowText.text = text;
        }

        /// <summary>
        /// 设置角色名
        /// </summary>
        public void SetNameText(string name)
        {
            if (_characterNameText != null)
            {
                _characterNameText.text = name;
            }
        }

        #endregion

        #region Key Wait Icon

        /// <summary>
        /// 显示等待图标
        /// </summary>
        public void ShowKeyWaitIcon(bool show = true)
        {
            if (_keyWaitIcon != null)
            {
                _keyWaitIcon.SetActive(show);

                if (!show)
                {
                    return;
                }
            }

            _keyWaitIconTween[0]?.Kill();
            _keyWaitIconTween[1]?.Kill();
            var rectTransform = _keyWaitIconTransform;
            var image = _keyWaitIcon.GetComponent<Image>();

            var anchoredPosition = rectTransform.anchoredPosition;
            anchoredPosition.y = 5;
            rectTransform.anchoredPosition = anchoredPosition;

            var color = image.color;
            color.a = 1;
            image.color = color;

            _keyWaitIconTween[0] = rectTransform.DOAnchorPosY(0, 0.66f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            _keyWaitIconTween[1] = image.DOFade(0.5f, 0.66f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// 隐藏等待图标
        /// </summary>
        public void HideKeyWaitIcon()
        {
            ShowKeyWaitIcon(false);
        }

        /// <summary>
        /// 设置等待图标位置
        /// </summary>
        public void SetKeyWaitIconPosition(Vector2 position)
        {
            if (_keyWaitIconTransform != null)
                _keyWaitIconTransform.anchoredPosition = position;
        }

        #endregion

        #region Menu Control

        /// <summary>
        /// 显示菜单
        /// </summary>
        public void ShowMenuButton(bool show = true)
        {
            if (_menuButtonArray != null)
            {
                foreach (var button in _menuButtonArray)
                {
                    if (button != null)
                    {
                        button.SetActive(show);
                    }
                }
            }
        }

        /// <summary>
        /// 隐藏菜单
        /// </summary>
        public void HideMenuButton()
        {
            ShowMenuButton(false);
        }

        /// <summary>
        /// 显示回顾日志
        /// </summary>
        public void ShowBackLog(bool show = true)
        {
            if (_backLogPanel != null)
                _backLogPanel.SetActive(show);
        }

        #endregion

        #region Novel Mode

        /// <summary>
        /// 显示小说模式窗口
        /// </summary>
        public void ShowNovelMode(bool show = true)
        {
            if (_novelModeFilter != null)
                _novelModeFilter.SetActive(show);
            if (_novelModeWindow != null)
                _novelModeWindow.SetActive(show);
        }

        /// <summary>
        /// 设置小说模式文本
        /// </summary>
        public void SetNovelModeText(string text)
        {
            if (_novelModeText != null)
                _novelModeText.text = text;
        }

        #endregion

        #region Effects

        /// <summary>
        /// 显示棕褐色滤镜
        /// </summary>
        public void ShowSepiaFilter(bool show = true)
        {
            if (_sepiaFilter != null)
                _sepiaFilter.SetActive(show);
        }

        public void SetFadeIn(float duration = 0.5f)
        {
            if (_fadeFilter == null)
            {
                return;
            }

            _fadeFilter.SetActive(true);
            var fadeImage = _fadeFilter.GetComponent<Image>();

            _fadeTween?.Kill();
            _fadeTween = null;

            if (duration <= 0f)
            {
                fadeImage.enabled = false;
                var fadeImageColor = fadeImage.color;
                fadeImageColor.a = 0f;
                fadeImage.color = fadeImageColor;
            }
            else
            {
                fadeImage.enabled = true;
                fadeImage.raycastTarget = false;
                _fadeTween = fadeImage.DOFade(0f, duration).SetEase(Ease.Linear);

                _fadeTween.onComplete = () =>
                {
                    fadeImage.enabled = false;
                };
            }
        }

        public void SetFadeOut(float duration = 0.5f)
        {
            if (_fadeFilter == null)
            {
                return;
            }

            _fadeFilter.SetActive(true);
            var fadeImage = _fadeFilter.GetComponent<Image>();

            _fadeTween?.Kill();
            _fadeTween = null;

            if (duration <= 0f)
            {
                fadeImage.enabled = true;
                fadeImage.raycastTarget = true;
                var fadeImageColor = fadeImage.color;
                fadeImageColor.a = 1f;
                fadeImage.color = fadeImageColor;
            }
            else
            {
                fadeImage.enabled = true;
                fadeImage.raycastTarget = true;
                _fadeTween = fadeImage.DOFade(1f, duration).SetEase(Ease.Linear);
            }
        }

        /// <summary>
        /// 显示/隐藏UI层
        /// </summary>
        public void ShowMenuPanel(bool show = true)
        {
            if (_menuPanel != null)
            {
                _menuPanel.SetActive(show);
            }
        }

        #endregion

        #region Layer Management

        /// <summary>
        /// 根据层级获取Transform
        /// </summary>
        public Transform GetLayerTransform(int level)
        {
            switch (level)
            {
                case 1: return _backgroundLayer;
                case 2: return _characterLayer;
                case 3: return _overlayLayer;
                case 4: return _wipeLayer;
                default: return _characterLayer;
            }
        }

        /// <summary>
        /// 清理指定层的所有对象
        /// </summary>
        public void ClearLayer(int level)
        {
            Transform layer = GetLayerTransform(level);
            if (layer != null)
            {
                foreach (Transform child in layer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// 清理所有层
        /// </summary>
        public void ClearAllLayers()
        {
            ClearLayer(1);
            ClearLayer(2);
            ClearLayer(3);
        }

        #endregion
    }
}

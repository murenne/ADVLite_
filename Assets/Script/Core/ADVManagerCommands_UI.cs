using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using UnityADV.Core;
using DG.Tweening;
using UnityADV.Character;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace UnityADV.Core
{
    public partial class ADVManager
    {
        #region Special Effect

        public void DoCommandFadeIn(float duration)
        {
            _currentState = ADVState.WaitTime;
            _waitRestTime = duration;
            _uiController.SetFadeIn(duration);
        }

        public void DoCommandFadeOut(float duration)
        {
            _currentState = ADVState.WaitTime;
            _waitRestTime = duration;
            _uiController.SetFadeOut(duration);
        }

        #endregion

        #region Move 
        public void DoCommandObjectMovePosX(int id, float x, float duration)
        {
            if (!_objectViewDictionary.TryGetValue(id, out var objectView) || objectView.uiGameObject == null)
                return;

            var rectTransform = objectView.uiGameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                objectView.tween?.Complete();
                objectView.tween = DOTweenModuleUI.DOAnchorPosX(rectTransform, x, duration);
            }
        }

        public void DoCommandObjectMovePosY(int id, float y, float duration, int easeIndex)
        {
            if (!_objectViewDictionary.TryGetValue(id, out var objectView) || objectView.uiGameObject == null)
                return;

            var rectTransform = objectView.uiGameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Ease easeType = (Ease)easeIndex;
                objectView.tween?.Complete();

                var sequence = DOTween.Sequence();
                sequence.Append(DOTweenModuleUI.DOAnchorPosY(rectTransform, rectTransform.anchoredPosition.y + y, duration).SetEase(easeType));
                objectView.tween = sequence;
            }
        }
        #endregion

        #region Text

        public void DoCommandSetText(int textId, int charaId, string text)
        {
            // 获取角色名
            string characterName = _localizationManager.GetCharacterName(charaId);

            // 获取翻译文本（如果不是日语）
            string translatedText = _localizationManager.GetScenarioText(_currentChapterId, textId);
            if (!string.IsNullOrEmpty(translatedText))
            {
                text = translatedText;
            }

            _currentText = text;
            _currentCharacterName = characterName;
            _textStartTime = Time.time;
            _displayedCharCount = 0;
            _currentState = ADVState.WaitText;

            // 更新UI
            _uiController.SetNameText(characterName);
            _uiController.SetDialogueText(text);

            // 设置目标角色
            _targetCharacterId = charaId;

            // 添加到回顾日志
            _backLogItemList.Add(new BackLogItem
            {
                CharaId = charaId,
                CharaName = characterName,
                TextId = textId,
                Text = text
            });

            // 停止Spine身体动作
            StopAllBodyMotions();
        }

        public void DoCommandTextWindowOpen()
        {
            _uiController.ShowTextWindow(true);
        }

        public void DoCommandTextWindowClose()
        {
            _uiController.ShowTextWindow(false);
        }

        #endregion

        #region  Shake

        public void DoCommandSetUniversalShake(int id, string axis, Vector2 shakeStrength, float shakeDuration, float delayDuration, int shakeCount, int easeIndex)
        {
            if (!_objectViewDictionary.TryGetValue(id, out var objectView) || objectView.uiGameObject == null)
                return;

            var rectTransform = objectView.uiGameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                DoCommandSetShakeCore(axis, shakeStrength, shakeDuration, delayDuration, shakeCount, easeIndex, objectView, rectTransform);
            }
        }

        public void DoCommandSetTextWindowShake(string axis, Vector2 shakeStrength, float shakeDuration, float delayDuration, int shakeCount, int easeIndex)
        {
            GameObject textWindowObject = _uiController.DialoguePanel;
            var rectTransform = textWindowObject.GetComponent<RectTransform>();

            DoCommandSetShakeCore(axis, shakeStrength, shakeDuration, delayDuration, shakeCount, easeIndex, null, rectTransform);
        }

        public void DoCommandSetShakeCore(string axis, Vector2 shakeStrength, float shakeDuration, float delayDuration, int shakeCount, int easeIndex, ADVObjectView objectView, RectTransform rectTransform)
        {
            Ease easeType = (Ease)easeIndex;//ease
            float segmentDuration = shakeDuration / 4;// 時間を4分割
            var initialPosition = rectTransform.anchoredPosition; // 最初の位置

            var loopSequence = DOTween.Sequence();
            if (axis == "x")
            {
                loopSequence.Append(rectTransform.DOAnchorPosX(initialPosition.x + shakeStrength.x, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosX(initialPosition.x, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosX(initialPosition.x - shakeStrength.x, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosX(initialPosition.x, segmentDuration).SetEase(easeType));
            }
            else if (axis == "y")
            {
                loopSequence.Append(rectTransform.DOAnchorPosY(initialPosition.y + shakeStrength.y, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosY(initialPosition.y, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosY(initialPosition.y - shakeStrength.y, segmentDuration).SetEase(easeType));
                loopSequence.Append(rectTransform.DOAnchorPosY(initialPosition.y, segmentDuration).SetEase(easeType));
            }
            else
            {
                Debug.LogError("don't have this axis");
                return;
            }
            loopSequence.SetLoops(shakeCount, LoopType.Restart);

            var mainSequence = DOTween.Sequence();
            mainSequence.SetDelay(delayDuration);
            mainSequence.Append(loopSequence);

            if (objectView != null)
            {
                objectView.tween = mainSequence;
            }
        }

        #endregion
    }
}

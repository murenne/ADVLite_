using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using UnityADV.Core;
using DG.Tweening;
using UnityADV.Character;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV管理器 - Effect
    /// </summary>
    public partial class ADVManager
    {
        public void DoCommandSetFlash(int id, float waitDuration, float endDuration)
        {
            if (!_objectViewDictionary.TryGetValue(id, out var objectView) || objectView.uiGameObject == null)
                return;

            var rectTransform = objectView.uiGameObject.GetComponent<RectTransform>();
            var image = objectView.uiGameObject.GetComponent<Image>();
            if (rectTransform == null || image == null)
                return;

            objectView.tween?.Complete();
            var sequence = DOTween.Sequence();
            sequence.AppendInterval(waitDuration);
            sequence.Append(image.DOFade(0f, endDuration));

            sequence.AppendCallback(() =>
            {
                DoCommandObjectDelete(id, null);
            });

            objectView.tween = sequence;
        }


    }
}

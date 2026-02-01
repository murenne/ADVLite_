using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityADV.Core;
using UnityADV.Resource;
using UnityADV.Script;
using UnityADV.Localization;
using UnityADV.Audio;
using UnityADV.Character;
using UnityADV.UI;
using DG.Tweening;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityADV
{
    /// <summary>
    /// ADV对象视图
    /// </summary>
    public class ADVObjectView
    {
        public GameObject uiGameObject;
        public ADVObjectType objectType;
        public AdvObjectState objectState;
        public int objectLevel; // 层级（前景、中景、背景）
        public int objectOrder; // 同一层级的物体渲染顺序

        // tween专用
        public Tween tween;

        // unitask专用
        public CancellationTokenSource cancellationTokenSource; // 发起取消

        // sprite专用
        public ADVResource<Sprite> sprite;

        // prefab专用
        public ADVResource<GameObject> prefab;

        // Shader专用
        public ADVResource<Shader> shader;

        // spine专用
        public GameObject spineGameObject;
        public ADVResource<SkeletonDataAsset> skeletonDataAsset;
        public RenderTexture renderTexture;
        public SpineCharacterController spineController;

        public void Cancel()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        public void Dispose(AddressableResourceManager resourceManager, RenderTexturePool renderTexturePool)
        {
            // 取消异步操作
            Cancel();

            // 销毁 GameObject
            if (uiGameObject != null)
            {
                Object.Destroy(uiGameObject);
                uiGameObject = null;
            }

            if (spineGameObject != null)
            {
                Object.Destroy(spineGameObject);
                spineGameObject = null;
            }

            // 释放 RenderTexture
            if (renderTexture != null)
            {
                renderTexturePool.Release(renderTexture);
                renderTexture = null;
            }

            if (sprite != null)
            {
                sprite.Dispose();
                sprite = null;
            }

            if (prefab != null)
            {
                prefab.Dispose();
                prefab = null;
            }

            if (shader != null)
            {
                shader.Dispose();
                shader = null;
            }

            if (skeletonDataAsset != null)
            {
                skeletonDataAsset.Dispose();
                skeletonDataAsset = null;
            }

            tween?.Kill();
            tween = null;
        }
    }
}

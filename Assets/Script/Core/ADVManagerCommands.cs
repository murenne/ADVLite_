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
    /// ADV管理器 - 命令实现部分
    /// </summary>
    public partial class ADVManager
    {
        #region System

        public void DoCommandWaitTime(float duration)
        {
            _currentState = ADVState.WaitTime;
            _waitRestTime = duration;
        }

        public void DoCommandWaitTask()
        {
            _currentState = ADVState.WaitTask;
        }

        public void DoCommandWaitKey()
        {
            _currentState = ADVState.WaitKey;
            _uiController.ShowKeyWaitIcon(true);
        }

        public void StopStartSkip()
        {
            _startSkip = false;
        }
        #endregion

        #region Prepare / Release  Resource
        public void PrepareResource<T>(string file) where T : class
        {
            var resource = new ADVResource<T>(_resourceManager, _resourceManager.LoadAssetAsync<T>(file));
            _preparedResourceList.Add(resource);

            if (!resource.GetHandleBase().IsDone)
            {
                _uniTasks.Add(resource.GetHandleBase().ToUniTask(cancellationToken: _cancellationToken));
            }
        }

        public void ReleasePreparedResource()
        {
            foreach (var advResource in _preparedResourceList)
            {
                advResource.Dispose();
            }
            _preparedResourceList.Clear();
        }

        #endregion

        #region Create Resource 
        public void DoCommandObjectCreateSprite(int id, string spriteFile, LuaObjectParam param)
        {
            var loadTask = UniTask.Create(async () =>
            {
                try
                {
                    DoCommandObjectDelete(id, null);

                    param ??= new LuaObjectParam();

                    // 创建视图对象
                    var objectView = _objectViewDictionary[id] = new ADVObjectView
                    {
                        objectType = ADVObjectType.Sprite,
                        objectLevel = param.Level,
                        objectOrder = param.Order,
                        cancellationTokenSource = new CancellationTokenSource()
                    };

                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(objectView.cancellationTokenSource.Token, _cancellationToken).Token;
                    var handle = _resourceManager.LoadAssetAsync<Sprite>(spriteFile);
                    objectView.sprite = new ADVResource<Sprite>(_resourceManager, handle);
                    await objectView.sprite.GetHandle().ToUniTask(cancellationToken: linkedToken);

                    if (objectView.sprite.Result == null)
                    {
                        objectView.sprite.Dispose();
                        _objectViewDictionary.Remove(id);
                        Debug.LogError($"Failed to load sprite: {spriteFile}");
                        return;
                    }

                    var spriteObj = new GameObject($"ADV_Sprite_{id}");
                    spriteObj.transform.SetParent(_uiController.GetLayerTransform(param.Level), false);

                    // 添加Image组件
                    var rectTransform = spriteObj.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(param.PosX, param.PosY);

                    var image = spriteObj.AddComponent<Image>();
                    image.sprite = objectView.sprite.Result;
                    image.SetNativeSize();
                    image.raycastTarget = false;

                    objectView.uiGameObject = spriteObj;

                    // 淡入效果
                    if (param.FadeTime > 0)
                    {
                        image.color = new Color(1, 1, 1, 0);
                        objectView.tween = image.DOFade(1f, param.FadeTime);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception in CreateSpriteObject: {ex.Message}\n{ex.StackTrace}");
                }
            });

            // 检查任务状态
            if (loadTask.Status == UniTaskStatus.Pending)
            {
                Debug.LogWarning($"[ADV] 创建Sprite任务异步执行，资源可能未预加载: {spriteFile}");

                // 添加到任务管理列表（如果有的话）
                _uniTasks.Add(loadTask);
            }
        }

        public void DoCommandObjectCreateSpine(int id, string spineFile, LuaObjectParam param)
        {
            var loadTask = UniTask.Create(async () =>
            {
                try
                {
                    DoCommandObjectDelete(id, null);

                    param ??= new LuaObjectParam();

                    // 创建视图对象
                    var objectView = _objectViewDictionary[id] = new ADVObjectView
                    {
                        objectType = ADVObjectType.Spine,
                        objectLevel = param.Level,
                        objectOrder = param.Order,
                        cancellationTokenSource = new CancellationTokenSource()
                    };

                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(objectView.cancellationTokenSource.Token, _cancellationToken).Token;
                    var handle = _resourceManager.LoadAssetAsync<SkeletonDataAsset>(spineFile);
                    objectView.skeletonDataAsset = new ADVResource<SkeletonDataAsset>(_resourceManager, handle);
                    await objectView.skeletonDataAsset.GetHandle().ToUniTask(cancellationToken: linkedToken);

                    if (objectView.skeletonDataAsset.Result == null)
                    {
                        objectView.skeletonDataAsset.Dispose();
                        _objectViewDictionary.Remove(id);
                        Debug.LogError($"Failed to load skeleton data: {spineFile}");
                        return;
                    }

                    // 使用RenderTexture模式
                    objectView.renderTexture = _renderTexturePool.Acquire();

                    // 创建显示对象
                    var displaySpineObj = Instantiate(_renderTexturePrefab, _uiController.GetLayerTransform(param.Level));
                    displaySpineObj.name = $"ADV_DisplaySpine_{id}";
                    var rectTransform = displaySpineObj.GetComponent<RectTransform>();
                    rectTransform.anchoredPosition = new Vector2(param.PosX, param.PosY);
                    displaySpineObj.GetComponent<RawImage>().texture = objectView.renderTexture;
                    objectView.uiGameObject = displaySpineObj;

                    // 创建元对象
                    var originalSpineObj = new GameObject($"ADV_OriginalSpine_{id}");
                    originalSpineObj.transform.SetParent(transform, false);
                    originalSpineObj.transform.localPosition = new Vector3(param.RenderPosX, param.RenderPosY, 0);
                    originalSpineObj.transform.localScale = Vector3.one * param.RenderScale;
                    originalSpineObj.layer = LayerMask.NameToLayer("SpineHidden");

                    var spineController = originalSpineObj.AddComponent<SpineCharacterController>();
                    spineController.Initialize(objectView.skeletonDataAsset.Result, id);
                    objectView.spineController = spineController;
                    objectView.spineGameObject = originalSpineObj;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception in CreateSpineObject: {ex.Message}\n{ex.StackTrace}");
                }
            });

            // 检查任务状态
            if (loadTask.Status == UniTaskStatus.Pending)
            {
                Debug.LogWarning($"[ADV] 创建Spine任务异步执行，资源可能未预加载: {spineFile}");

                // 添加到任务管理列表（如果有的话）
                _uniTasks.Add(loadTask);
            }
        }

        public void DoCommandObjectCreatePrefab(int id, string prefabFile, LuaObjectParam param)
        {
            var loadTask = UniTask.Create(async () =>
            {
                try
                {
                    DoCommandObjectDelete(id, null);

                    param ??= new LuaObjectParam();

                    // 创建视图对象
                    var objectView = _objectViewDictionary[id] = new ADVObjectView
                    {
                        objectType = ADVObjectType.Prefab,
                        objectLevel = param.Level,
                        objectOrder = param.Order,
                        cancellationTokenSource = new CancellationTokenSource()
                    };

                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(objectView.cancellationTokenSource.Token, _cancellationToken).Token;
                    var handle = _resourceManager.LoadAssetAsync<GameObject>(prefabFile);
                    objectView.prefab = new ADVResource<GameObject>(_resourceManager, handle);
                    await objectView.prefab.GetHandle().ToUniTask(cancellationToken: linkedToken);

                    if (objectView.prefab.Result == null)
                    {
                        objectView.prefab.Dispose();
                        _objectViewDictionary.Remove(id);
                        Debug.LogError($"Failed to load prefab data: {prefabFile}");
                        return;
                    }

                    var prefabObj = Instantiate(objectView.prefab.Result, _uiController.GetLayerTransform(param.Level));
                    prefabObj.name = $"ADV_Prefab_{id}";
                    var rectTransform = prefabObj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = new Vector2(param.PosX, param.PosY);
                    }
                    objectView.uiGameObject = prefabObj;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Exception in CreatePrefabObject: {ex.Message}\n{ex.StackTrace}");
                }
            });

            // 检查任务状态
            if (loadTask.Status == UniTaskStatus.Pending)
            {
                Debug.LogWarning($"[ADV] 创建Prefab任务异步执行，资源可能未预加载: {prefabFile}");

                // 添加到任务管理列表（如果有的话）
                _uniTasks.Add(loadTask);
            }
        }

        #endregion

        #region Delete Resource
        public void DoCommandObjectDelete(int id, LuaObjectParam param)
        {
            if (!_objectViewDictionary.TryGetValue(id, out var objectView))
                return;

            if (objectView.objectState == AdvObjectState.Deleting)
            {
                objectView.objectState = AdvObjectState.None;

                objectView.Dispose(_resourceManager, _renderTexturePool);
                _objectViewDictionary.Remove(id);

                return;
            }

            objectView.Cancel();

            // 检查是否需要淡出动画
            if (param != null && param.FadeTime > 0)
            {
                // 设置为删除中状态
                objectView.objectState = AdvObjectState.Deleting;

                // 创建淡出任务
                var deleteTask = UniTask.Create(async () =>
                {
                    try
                    {
                        // 创建新的取消令牌
                        objectView.cancellationTokenSource = new CancellationTokenSource();
                        var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(objectView.cancellationTokenSource.Token, _cancellationToken).Token;

                        // 对 RawImage 或 Image 执行淡出动画
                        if (objectView.uiGameObject != null)
                        {
                            var rawImageComponent = objectView.uiGameObject.GetComponent<RawImage>();
                            if (rawImageComponent != null)
                            {
                                objectView.tween = DOVirtual.Float(1, 0, param.FadeTime, value =>
                                {
                                    if (rawImageComponent != null)
                                    {
                                        var color = rawImageComponent.color;
                                        color.a = value;
                                        rawImageComponent.color = color;
                                    }
                                    else
                                    {
                                        objectView.tween?.Kill();
                                    }
                                });
                            }

                            var imageComponent = objectView.uiGameObject.GetComponent<Image>();
                            if (imageComponent != null)
                            {
                                imageComponent.color = Color.white;
                                objectView.tween = imageComponent.DOColor(new Color(1, 1, 1, 0), param.FadeTime);
                            }
                        }

                        // 等待淡出完成
                        await UniTask.Delay((int)(param.FadeTime * 1000), cancellationToken: linkedToken);

                        // 释放所有资源
                        objectView.Dispose(_resourceManager, _renderTexturePool);

                        // 从字典移除
                        _objectViewDictionary.Remove(id);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.Log($"[ADV] 删除对象被取消: {id}");

                        objectView.Dispose(_resourceManager, _renderTexturePool);
                        _objectViewDictionary.Remove(id);
                    }
                });

                if (deleteTask.Status == UniTaskStatus.Pending)
                {
                    deleteTask.Forget();
                }
            }
            else
            {
                // 没有淡出动画，直接删除
                objectView.Dispose(_resourceManager, _renderTexturePool);
                _objectViewDictionary.Remove(id);
            }
        }
        #endregion

        #region Update View

        public void DoCommandObjectSetOrder(int id, int order)
        {
            if (_objectViewDictionary.TryGetValue(id, out var objectView))
            {
                objectView.objectOrder = order;
            }
        }

        public void DoCommandObjectSetTargetChara(int id)
        {
            _targetCharacterId = id;
        }

        public void DoCommandUpdateView()
        {
            // 获取有效的视图（Level = 2 的角色层）
            var list = _objectViewDictionary.Where(view => view.Value != null && view.Value.objectLevel == 2).ToList();

            // 按 Order 排序（Order 小 → Order 大）
            list.Sort((a, b) => a.Value.objectOrder - b.Value.objectOrder);

            var order = 0;
            GameObject targetObject = null;

            foreach (var (id, objectView) in list)
            {
                if (objectView.uiGameObject == null)
                    continue;

                // 根据位置 X 坐标决定层级
                var rectTransform = objectView.uiGameObject.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float positionX = rectTransform.anchoredPosition.x;

                    if (positionX == 300 || positionX == 450)
                    {
                        objectView.uiGameObject.transform.SetAsFirstSibling();
                    }
                    else if (positionX == -300 || positionX == -450)
                    {
                        objectView.uiGameObject.transform.SetAsLastSibling();
                    }
                    else
                    {
                        // Hierarchy 的中间
                        objectView.uiGameObject.transform.SetSiblingIndex(order / 2);
                    }
                }

                // 目标角色以外的灰色处理
                if (_targetCharacterId > 0 && id != _targetCharacterId)
                {
                    // var renderer = objectView.GameObject.GetComponent<CanvasRenderer>();
                    // renderer.SetColor(Color.gray);
                }
                else
                {
                    targetObject = objectView.uiGameObject;
                }

                order++;
            }

            // 设置目标角色（正在说话的角色）
            if (targetObject != null)
            {
                // var renderer = targetObject.GetComponent<CanvasRenderer>();
                // renderer.SetColor(Color.white);

                targetObject.transform.SetAsLastSibling();
            }
        }

        #endregion

        #region Spine Commands

        public void DoCommandObjectSetSpineBreath(int charaId)
        {
            if (_objectViewDictionary.TryGetValue(charaId, out var view) && view.spineController != null)
            {
                view.spineController.SetBreathAnimation();
            }
        }

        public void DoCommandObjectSetSpineBody(int charaId, int bodyMotionId)
        {
            if (_objectViewDictionary.TryGetValue(charaId, out var view) && view.spineController != null)
            {
                view.spineController.SetBodyMotion(bodyMotionId);
            }
        }

        public void DoCommandObjectSetSpineEye(int charaId, int eyeMotionId)
        {
            if (_objectViewDictionary.TryGetValue(charaId, out var view) && view.spineController != null)
            {
                view.spineController.SetEyeMotion(eyeMotionId);
            }
        }

        public void DoCommandObjectSetSpineLipSync(int charaId, int lipMotionId)
        {
            if (_objectViewDictionary.TryGetValue(charaId, out var view) && view.spineController != null)
            {
                view.spineController.SetLipSync(false, lipMotionId);
            }
        }

        private void StopAllBodyMotions()
        {
            foreach (var view in _objectViewDictionary.Values)
            {
                if (view.objectType == ADVObjectType.Spine && view.spineController != null)
                {
                    if (view.spineController.CanStopBodyMotion)
                    {
                        view.spineController.StopBodyMotion(0.5f);
                    }
                }
            }
        }

        #endregion
    }
}

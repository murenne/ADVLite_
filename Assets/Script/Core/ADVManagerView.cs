using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV Manager - view 部分
    /// </summary>
    public partial class ADVManager
    {
        private void UpdateViewFrame()
        {
            // 更新spine，表情，口型等，更新text
            //_uiController.SetCharacterName(_currentCharacterName);

            // 2. 更新对话文本
            //_uiController.SetDialogueText(_realText);

            // 3. 更新口型同步
            UpdateLipSync();

            // 4. 更新 Spine 眼睛
            UpdateSpineEye();

            // 5. 渲染 Spine 对象
            SpineCharactersRenderUpdate();
        }


        /// <summary>
        /// 渲染Spine角色到RenderTexture
        /// 每帧调用，将3D空间的Spine角色渲染到RenderTexture，然后显示在UI上
        /// （因为只有一个spine 相机，所以要用一个相机渲染所有角色，
        /// 就要让一个角色渲染完后就切换到其他hidden，这样渲染下一个角色的时候就看不到其他角色了）
        /// </summary>
        private void SpineCharactersRenderUpdate()
        {
            if (_spineRenderCamera == null)
            {
                return;
            }

            foreach (var objectView in _objectViewDictionary.Values)
            {
                // 只处理Spine类型且有RenderTexture的对象
                if (objectView.objectType == ADVObjectType.Spine && objectView.renderTexture != null && objectView.spineGameObject != null)
                {
                    // 设置相机渲染目标
                    _spineRenderCamera.targetTexture = objectView.renderTexture;

                    // 临时设置对象到SpineRender层（让相机能看到）
                    int originalLayer = objectView.spineGameObject.layer;
                    SetObjectLayer(objectView.spineGameObject, LayerMask.NameToLayer("SpineRender"));

                    // 渲染
                    _spineRenderCamera.Render();

                    // 恢复原始层级（隐藏对象）
                    SetObjectLayer(objectView.spineGameObject, originalLayer);
                }
            }

            // 清除相机目标
            _spineRenderCamera.targetTexture = null;
        }

        private void UpdateSpineEye()
        {
            // foreach (var objectView in _objectViewDictionary.Values)
            // {
            //     if (objectView.objectType == ADVObjectType.Spine && objectView.spineController != null)
            //     {
            //         // 更新眨眼计时器
            //         if (objectView.eyeBlinkTimer != 0)
            //         {
            //             if (objectView.eyeBlinkTimer > 0)
            //             {
            //                 objectView.eyeBlinkTimer -= _deltaTime; // ✅ 使用 _deltaTime
            //             }
            //             else
            //             {
            //                 objectView.eyeBlinkTimer = 0;

            //                 // 触发眨眼
            //                 objectView.spineController.TriggerEyeBlink();

            //                 // 设置下次眨眼时间
            //                 objectView.eyeBlinkTimer = Random.Range(1f, 6f);
            //             }
            //         }

            //         // 更新眨眼动画
            //         objectView.spineController.UpdateEyeBlink(_deltaTime);
            //     }
            // }
        }

        private void UpdateLipSync()
        {
            // if (_audioManager == null) return;

            // var lipSyncValue = _audioManager.GetVoiceLipSyncValue();
            // var isVoicePlaying = _audioManager.IsVoicePlaying() && lipSyncValue > 0.01f;

            // // 目标角色改变时，停止前一个角色的口型
            // if (_targetCharacterId != _previousTargetCharacterId)
            // {
            //     if (_objectViewDictionary.TryGetValue(_previousTargetCharacterId, out var prevView))
            //     {
            //         if (prevView.spineController != null)
            //         {
            //             prevView.spineController.SetLipSync(false);
            //             prevView.realLipSyncPlaying = false;
            //         }
            //     }
            //     _previousTargetCharacterId = _targetCharacterId;
            // }

            // // 更新当前角色的口型
            // foreach (var objectView in _objectViewDictionary.Values)
            // {
            //     if (objectView.objectType == ADVObjectType.Spine)
            //     {
            //         // 只更新目标角色
            //         if (objectView.charaId != _targetCharacterId)
            //             continue;

            //         // 口型需要更新
            //         if ((objectView.currentFaceMotionId != objectView.realLipSyncId || isVoicePlaying != objectView.realLipSyncPlaying) &&
            //             !objectView.isInnerVoice)
            //         {
            //             objectView.realLipSyncId = objectView.currentFaceMotionId;
            //             objectView.realLipSyncPlaying = isVoicePlaying;

            //             if (objectView.spineController != null)
            //             {
            //                 objectView.spineController.SetLipSync(isVoicePlaying);
            //             }
            //         }
            //     }
            // }
        }

        /// <summary>
        /// 设置GameObject及其所有子对象的Layer
        /// </summary>
        private void SetObjectLayer(GameObject obj, int layer)
        {
            if (obj == null)
            {
                return;
            }

            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                SetObjectLayer(child.gameObject, layer);
            }
        }

    }
}

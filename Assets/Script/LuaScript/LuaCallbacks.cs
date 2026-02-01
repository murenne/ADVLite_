using XLua;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityADV.Core;
using UnityADV.Audio;

namespace UnityADV.Script
{
    /// <summary>
    /// Lua回调接口
    /// 提供Lua脚本调用C#功能的桥梁
    /// </summary>
    [LuaCallCSharp]
    public class LuaCallbacks
    {
        private Core.ADVManager _manager;
        private bool _returnValue = false;

        public LuaCallbacks(Core.ADVManager manager)
        {
            _manager = manager;
        }

        public bool GetBoolValue()
        {
            return _returnValue;
        }
        public void SetBoolValue(bool value)
        {
            _returnValue = value;
        }

        public void Include(string file)
        {
            _manager.LuaEngine.LoadScript(file);
        }

        #region Text Commands

        /// <summary>
        /// 显示文本
        /// </summary>
        public void SetText(int textId, int charaId, string text)
        {
            _manager.DoCommandSetText(textId, charaId, text);
        }

        /// <summary>
        /// 等待用户输入
        /// </summary>
        public void WaitKey()
        {
            _manager.DoCommandWaitKey();
        }

        /// <summary>
        /// 打开文本窗口
        /// </summary>
        public void TextWindowOpen()
        {
            _manager.DoCommandTextWindowOpen();
        }

        /// <summary>
        /// 关闭文本窗口
        /// </summary>
        public void TextWindowClose()
        {
            _manager.DoCommandTextWindowClose();
        }

        public void SetTextWindowShake(string axis, Vector2 shakeStrength, float shakeDuration, float delayDuration, int shakeCount, int easeIndex)
        {
            _manager.DoCommandSetTextWindowShake(axis, shakeStrength, shakeDuration, delayDuration, shakeCount, easeIndex);
        }

        public void SetUniversalShake(int id, string axis, Vector2 shakeStrength, float shakeDuration, float delayDuration, int shakeCount, int easeIndex)
        {
            _manager.DoCommandSetUniversalShake(id, axis, shakeStrength, shakeDuration, delayDuration, shakeCount, easeIndex);
        }

        #endregion

        #region Object Commands

        /// <summary>
        /// 创建Sprite对象
        /// </summary>
        public void ObjectCreateSprite(int id, string file, LuaObjectParam param)
        {
            _manager.DoCommandObjectCreateSprite(id, file, param);
        }

        /// <summary>
        /// 创建Spine对象
        /// </summary>
        public void ObjectCreateSpine(int id, string spineFile, LuaObjectParam param)
        {
            _manager.DoCommandObjectCreateSpine(id, spineFile, param);
        }

        /// <summary>
        /// 创建Prefab对象
        /// </summary>
        public void ObjectCreatePrefab(int id, string file, LuaObjectParam param)
        {
            _manager.DoCommandObjectCreatePrefab(id, file, param);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        public void ObjectDelete(int id, LuaObjectParam param)
        {
            _manager.DoCommandObjectDelete(id, param);
        }

        /// <summary>
        /// 移动对象X位置
        /// </summary>
        public void ObjectMovePosX(int id, float x, float duration)
        {
            _manager.DoCommandObjectMovePosX(id, x, duration);
        }

        /// <summary>
        /// 移动对象Y位置
        /// </summary>
        public void ObjectMovePosY(int id, float y, float duration, int easeIndex = 0)
        {
            _manager.DoCommandObjectMovePosY(id, y, duration, easeIndex);
        }

        /// <summary>
        /// 设置对象排序
        /// </summary>
        public void ObjectSetOrder(int id, int order)
        {
            _manager.DoCommandObjectSetOrder(id, order);
        }

        /// <summary>
        /// 设置目标角色
        /// </summary>
        public void ObjectSetTargetChara(int id)
        {
            _manager.DoCommandObjectSetTargetChara(id);
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        public void UpdateView()
        {
            _manager.DoCommandUpdateView();
        }

        #endregion

        #region Spine Commands

        /// <summary>
        /// 设置Spine呼吸动画
        /// </summary>
        public void ObjectSetSpineBreath(int charaId)
        {
            _manager.DoCommandObjectSetSpineBreath(charaId);
        }

        /// <summary>
        /// 设置Spine身体动作
        /// </summary>
        public void ObjectSetSpineBody(int charaId, int bodyMotionId)
        {
            _manager.DoCommandObjectSetSpineBody(charaId, bodyMotionId);
        }

        /// <summary>
        /// 设置Spine眼睛表情
        /// </summary>
        public void ObjectSetSpineEye(int charaId, int eyeMotionId)
        {
            _manager.DoCommandObjectSetSpineEye(charaId, eyeMotionId);
        }

        /// <summary>
        /// 设置Spine口型同步
        /// </summary>
        public void ObjectSetSpineLipSync(int charaId, int lipMotionId)
        {
            _manager.DoCommandObjectSetSpineLipSync(charaId, lipMotionId);
        }

        #endregion

        #region Audio Commands

        /// <summary>
        /// 播放语音
        /// </summary>
        public void PlayVoice(string file)
        {
            _manager.DoCommandVoiceStart(file);
        }

        /// <summary>
        /// 停止语音
        /// </summary>
        public void StopVoice()
        {
            _manager.DoCommandStopVoice();
        }

        /// <summary>
        /// 播放BGM
        /// </summary>
        public void PlayBGM(string file)
        {
            _manager.DoCommandPlayBGM(file);
        }

        /// <summary>
        /// 停止BGM
        /// </summary>
        public void StopBGM()
        {
            _manager.DoCommandStopBGM();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public long PlaySound(string file, LuaTable param)
        {
            var soundParam = ParseSoundParam(param);
            return _manager.DoCommandPlaySound(file, soundParam);
        }

        /// <summary>
        /// 停止音效
        /// </summary>
        public void StopSound(long playerId)
        {
            _manager.DoCommandStopSound(playerId);
        }

        #endregion

        #region Effect Commands

        public void WaitTime(float duration)
        {
            _manager.DoCommandWaitTime(duration);
        }

        public void WaitTask()
        {
            _manager.DoCommandWaitTask();
        }

        public void FadeIn(float duration)
        {
            _manager.DoCommandFadeIn(duration);
        }

        public void FadeOut(float duration)
        {
            _manager.DoCommandFadeOut(duration);
        }

        public void SetFlash(int id, float waitDuration, float endDuration)
        {
            _manager.DoCommandSetFlash(id, waitDuration, endDuration);
        }

        #endregion

        #region Resource Commands

        /// <summary>
        /// 预加载Sprite资源
        /// </summary>
        public void PrepareAssetSprite(string file)
        {
            _manager.PrepareResource<Sprite>(file);
        }

        /// <summary>
        /// 预加载Prefab资源
        /// </summary>
        public void PrepareAssetPrefab(string file)
        {
            _manager.PrepareResource<GameObject>(file);
        }

        /// <summary>
        /// 预加载Spine资源
        /// </summary>
        public void PrepareAssetSpine(string file)
        {
            _manager.PrepareResource<SkeletonDataAsset>(file);
        }

        /// <summary>
        /// 预加载音频资源
        /// </summary>
        public void PrepareChapterAudio(string chapterName)
        {
            _manager.PrepareChapterAudio(chapterName);
        }

        /// <summary>
        /// 释放所有预加载资源
        /// </summary>
        public void ReleaseAllPreparedAsset()
        {
            _manager.ReleasePreparedResource();
        }

        #endregion

        #region System Commands

        /// <summary>
        /// 停止开始跳过
        /// </summary>
        public void StopStartSkip()
        {
            _manager.StopStartSkip();
        }

        /// <summary>
        /// 调试日志
        /// </summary>
        public void DebugLog(string text)
        {
            Debug.Log($"[Lua] {text}");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 解析Lua对象参数
        /// </summary>
        private LuaObjectParam ParseObjectParam(LuaTable table)
        {
            if (table == null)
                return new LuaObjectParam();

            var param = new LuaObjectParam();
            table.Get("Show", out param.Show);
            table.Get("Level", out param.Level);
            table.Get("Order", out param.Order); // 即使 Lua 没传，这里也不会报错了
            table.Get("PosX", out param.PosX);
            table.Get("PosY", out param.PosY);
            table.Get("FadeTime", out param.FadeTime);
            table.Get("RenderPosX", out param.RenderPosX);
            table.Get("RenderPosY", out param.RenderPosY);
            table.Get("RenderScale", out param.RenderScale);
            table.Get("IsWipe", out param.IsWipe);
            table.Get("IsSafeView", out param.IsSafeView);

            return param;
        }

        /// <summary>
        /// 解析Lua音频参数
        /// </summary>
        private LuaSoundParam ParseSoundParam(LuaTable table)
        {
            if (table == null)
                return new LuaSoundParam();

            var param = new LuaSoundParam();
            param.Loop = table.Get<bool>("Loop");
            param.Volume = table.Get<float>("Volume");

            return param;
        }

        #endregion





        // /// <summary>
        // /// 播放环境音
        // /// </summary>
        // public long PlayAmbiSound(string file, LuaTable param)
        // {
        //     var soundParam = ParseSoundParam(param);
        //     return _manager.DoCommandPlayAmbiSound(file, soundParam);
        // }

        // /// <summary>
        // /// 停止环境音
        // /// </summary>
        // public void StopAmbiSound(long playerId)
        // {
        //     _manager.DoCommandStopAmbiSound(playerId);
        // }



    }
}

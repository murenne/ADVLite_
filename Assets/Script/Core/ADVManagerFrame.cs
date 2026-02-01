using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV Manager - Frame Loop 部分
    /// </summary>
    public partial class ADVManager
    {
        /// <summary>
        /// 帧循环（对齐原框架 _FrameLoop）
        /// </summary>
        private async UniTask FrameLoop()
        {
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, _cancellationToken);

                _yieldFlag = false;

                // 特殊状态处理
                ProcessSpecialStates();

                // 清理完成的任务
                CleanupCompletedTweens();
                CleanupCompletedUniTasks();

                // 状态循环
                if (_exState == ADVExState.None)
                {
                    ProcessStateLoop();
                }

                // 脚本结束时退出
                if (_currentState == ADVState.End)
                    break;

                await UniTask.Yield(PlayerLoopTiming.PreLateUpdate, _cancellationToken);

                // 更新视图
                UpdateViewFrame();
            }
        }

        /// <summary>
        /// 状态循环（对齐原框架 _StateLoop）
        /// </summary>
        private void ProcessStateLoop()
        {
            // 计时系统
            _prevTime = _nowTime;
            _deltaTime = Time.deltaTime;
            _nowTime += _deltaTime;

            // 检查跳过状态
            UpdateSkipState();

            // 状态处理循环
            while (!_yieldFlag)
            {
                switch (_currentState)
                {
                    case ADVState.Script:
                        if (_luaEngine.IsScriptDead())
                        {
                            _currentState = ADVState.End;
                        }
                        else
                        {
                            ResumeScript();
                        }
                        break;

                    case ADVState.WaitTask:
                        if (_uniTasks.Count == 0 && _tweenTasks.Count == 0)
                        {
                            NextState();
                        }
                        else
                        {
                            _yieldFlag = true;
                        }
                        break;

                    case ADVState.WaitText:
                        ProcessWaitText();
                        break;

                    case ADVState.WaitKey:
                        ProcessWaitKey();
                        break;

                    case ADVState.WaitTime:
                        ProcessWaitTime();
                        break;

                    case ADVState.End:
                        _yieldFlag = true;
                        break;

                    default:
                        _yieldFlag = true;
                        break;
                }
            }
        }

        // 特殊状态处理
        private void ProcessSpecialStates()
        {
            // 摘要
            if (_exState == ADVExState.Summary)
            {
                // TODO: 实现摘要显示
                Debug.Log("Summary Mode");
                _exState = ADVExState.None;
            }

            // 选项
            if (_exState == ADVExState.Option)
            {
                // TODO: 实现选项显示
                Debug.Log("Option Mode");
                _exState = ADVExState.None;
            }

            // 回顾日志
            if (_exState == ADVExState.BackLog)
            {
                // TODO: 实现回顾日志
                Debug.Log("BackLog Mode");
                _exState = ADVExState.None;
            }

            // 跳过到标题
            if (_exState == ADVExState.Skip)
            {
                Debug.Log("Skip to Title");
                StopADV();
                _exState = ADVExState.None;
            }
        }

        /// <summary>
        /// 恢复脚本执行
        /// </summary>
        private void ResumeScript()
        {
            // 设置 DeltaTime
            _luaEngine.SetGlobal("reservedDeltaTime", Time.deltaTime);

            try
            {
                _luaEngine.Resume();
                _yieldFlag = true;
            }
            catch (XLua.LuaException e)
            {
                var errorFile = _luaEngine.GetGlobal<string>("errorFile");
                var errorLine = _luaEngine.GetGlobal<int>("errorLine");
                Debug.LogError($"Lua Error {errorFile}({errorLine})\n{e.Message}");
                _currentState = ADVState.End;
            }
        }

        /// <summary>
        /// 处理文本等待状态
        /// </summary>
        private void ProcessWaitText()
        {
            float textSpeed = _config.textSpeed;
            int targetCharCount = Mathf.FloorToInt((_nowTime - _textStartTime) * textSpeed);
            targetCharCount = Mathf.Min(targetCharCount, _currentText.Length);

            if (_isHardSkip || _isSoftSkip || Input.GetMouseButtonDown(0))
            {
                targetCharCount = _currentText.Length;
            }

            _displayedCharCount = targetCharCount;
            _uiController.SetDialogueText(_currentText.Substring(0, _displayedCharCount));

            if (_displayedCharCount >= _currentText.Length)
            {
                NextState();
            }
            else
            {
                _yieldFlag = true;
            }
        }

        /// <summary>
        /// 处理按键等待状态
        /// </summary>
        private void ProcessWaitKey()
        {
            var next = false;

            _autoModeRestTime -= _deltaTime;

            if (_autoModeFlag && _autoModeRestTime < _config.autoModeMinWait && _audioManager.IsVoicePlaying())
            {
                _autoModeRestTime = _config.autoModeMinWait;
            }

            if (_isHardSkip || _isSoftSkip || Input.GetMouseButtonDown(0))
            {
                next = true;
            }
            else if (_autoModeFlag && _autoModeRestTime <= 0f)
            {
                next = true;
            }

            if (next)
            {
                _uiController.HideKeyWaitIcon();
                NextState();
            }
            else
            {
                _yieldFlag = true;
            }
        }

        /// <summary>
        /// 处理时间等待状态
        /// </summary>
        private void ProcessWaitTime()
        {
            _waitRestTime -= _deltaTime;

            if (_waitRestTime <= 0 || _isHardSkip || _startSkip)
            {
                NextState();
            }
            else
            {
                _yieldFlag = true;
            }
        }

        /// <summary>
        /// 更新跳过状态
        /// </summary>
        private void UpdateSkipState()
        {
            _isHardSkip = _startSkip || (Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift));
            _isSoftSkip = Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift);
        }


        // ✅ 清理完成的 UniTask
        private void CleanupCompletedUniTasks()
        {
            for (var i = _uniTasks.Count - 1; i >= 0; i--)
            {
                if (_uniTasks[i].Status != UniTaskStatus.Pending)
                {
                    _uniTasks.RemoveAt(i);
                }
            }
        }

        // ✅ 9. 清理完成的 Tween 任务
        private void CleanupCompletedTweens()
        {
            for (var i = _tweenTasks.Count - 1; i >= 0; i--)
            {
                if (_tweenTasks[i] == null || !_tweenTasks[i].IsActive())
                {
                    _tweenTasks.RemoveAt(i);
                }
            }
        }

        // ✅ 添加 Tween 到任务列表
        public void AddTweenTask(Tween tween)
        {
            if (tween != null && tween.IsActive())
            {
                _tweenTasks.Add(tween);
            }
        }

        /// <summary>
        /// 下一个状态
        /// </summary>
        private void NextState()
        {
            if (_stateQueue.Count >= 1)
            {
                _currentState = _stateQueue[0];
                _stateQueue.RemoveAt(0);
            }
            else
            {
                _currentState = ADVState.Script;
            }
        }
    }
}
using UnityEngine;
using Spine.Unity;

namespace UnityADV.Character
{
    /// <summary>
    /// Spine角色控制器
    /// 管理Spine角色的动画、口型同步、眨眼等
    /// </summary>
    public class SpineCharacterController : MonoBehaviour
    {
        private SkeletonAnimation _skeletonAnimation;
        private int _characterId;
        private int _currentFaceMotionId;
        private float _eyeBlinkTimer;
        private bool _isLipSyncPlaying;
        private bool _canStopBodyMotion = false;

        // 动画轨道定义
        private const int TRACK_BREATH = 0;
        private const int TRACK_BODY = 1;
        private const int TRACK_EYE = 2;
        private const int TRACK_LIP = 3;

        public int CharacterId => _characterId;
        public int CurrentFaceMotionId => _currentFaceMotionId;
        public bool CanStopBodyMotion { get => _canStopBodyMotion; set => _canStopBodyMotion = value; }

        /// <summary>
        /// 初始化Spine角色
        /// </summary>
        public void Initialize(SkeletonDataAsset skeletonData, int characterId)
        {
            _characterId = characterId;

            // 创建SkeletonAnimation组件
            _skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
            if (_skeletonAnimation == null)
            {
                _skeletonAnimation = gameObject.AddComponent<SkeletonAnimation>();
            }

            _skeletonAnimation.skeletonDataAsset = skeletonData;
            _skeletonAnimation.Initialize(false);

            // 设置默认呼吸动画
            //SetBreathAnimation();

            Debug.Log($"Initialized Spine character: {characterId}");
        }

        /// <summary>
        /// 设置呼吸动画
        /// </summary>
        public void SetBreathAnimation()
        {
            if (_skeletonAnimation == null) return;

            try
            {
                _skeletonAnimation.state.SetAnimation(TRACK_BREATH, "breath", true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set breath animation: {e.Message}");
            }
        }

        /// <summary>
        /// 设置身体动作
        /// </summary>
        public void SetBodyMotion(int motionId)
        {
            if (_skeletonAnimation == null) return;

            try
            {
                string motionName = $"body_{motionId:D3}";
                var trackEntry = _skeletonAnimation.state.SetAnimation(TRACK_BODY, motionName, false);

                if (trackEntry != null)
                {
                    trackEntry.Complete += (entry) =>
                    {
                        _canStopBodyMotion = false;
                    };
                }

                _canStopBodyMotion = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set body motion {motionId}: {e.Message}");
            }
        }

        /// <summary>
        /// 停止身体动作
        /// </summary>
        public void StopBodyMotion(float mixDuration = 0.5f)
        {
            if (_skeletonAnimation == null || !_canStopBodyMotion) return;

            try
            {
                var trackEntry = _skeletonAnimation.state.SetEmptyAnimation(TRACK_BODY, mixDuration);
                if (trackEntry != null)
                {
                    trackEntry.Complete += (entry) =>
                    {
                        _canStopBodyMotion = false;
                    };
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to stop body motion: {e.Message}");
            }
        }

        /// <summary>
        /// 设置眼睛表情
        /// </summary>
        public void SetEyeMotion(int motionId)
        {
            if (_skeletonAnimation == null) return;

            _currentFaceMotionId = motionId;

            try
            {
                string eyeMotionName = $"eye_blink/eye_blink_1001{motionId:D3}";
                _skeletonAnimation.state.SetAnimation(TRACK_EYE, eyeMotionName, false);

                // 设置随机眨眼间隔
                _eyeBlinkTimer = Random.Range(1f, 6f);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set eye motion {motionId}: {e.Message}");
            }
        }

        /// <summary>
        /// 设置口型同步
        /// </summary>
        public void SetLipSync(bool isPlaying, int motionId = 0)
        {
            if (_skeletonAnimation == null) return;

            if (motionId > 0)
            {
                _currentFaceMotionId = motionId;
            }

            _isLipSyncPlaying = isPlaying;

            try
            {
                string lipMotionName = isPlaying
                    ? $"lip_voice/lip_voice_1001{_currentFaceMotionId:D3}"
                    : $"lip/lip_1001{_currentFaceMotionId:D3}";

                _skeletonAnimation.state.SetAnimation(TRACK_LIP, lipMotionName, true);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to set lip sync: {e.Message}");
            }
        }

        /// <summary>
        /// 更新眨眼逻辑（每帧调用）
        /// </summary>
        public void UpdateEyeBlink(float deltaTime)
        {
            if (_skeletonAnimation == null || _currentFaceMotionId == 0)
                return;

            var animationState = _skeletonAnimation.state;
            var currentTrack = animationState.GetCurrent(TRACK_EYE);

            // 检查眼睛轨道是否有动画且不是空动画
            if (currentTrack != null && currentTrack.Animation.Name == "<empty>")
            {
                if (_eyeBlinkTimer > 0)
                {
                    _eyeBlinkTimer -= deltaTime;

                    if (_eyeBlinkTimer <= 0)
                    {
                        // 重新播放眨眼动画
                        SetEyeMotion(_currentFaceMotionId);
                    }
                }
            }
        }

        /// <summary>
        /// 更新口型同步（每帧调用）
        /// </summary>
        public void UpdateLipSync(bool isVoicePlaying, float lipSyncValue)
        {
            if (_skeletonAnimation == null || _currentFaceMotionId == 0)
                return;

            // 判断是否需要更新口型
            bool shouldPlayLipSync = isVoicePlaying && lipSyncValue > 0.01f;

            if (shouldPlayLipSync != _isLipSyncPlaying)
            {
                SetLipSync(shouldPlayLipSync);
            }
        }

        /// <summary>
        /// 播放自定义动画
        /// </summary>
        public void PlayAnimation(int trackIndex, string animationName, bool loop)
        {
            if (_skeletonAnimation == null) return;

            try
            {
                _skeletonAnimation.state.SetAnimation(trackIndex, animationName, loop);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to play animation {animationName}: {e.Message}");
            }
        }

        /// <summary>
        /// 获取SkeletonAnimation组件
        /// </summary>
        public SkeletonAnimation GetSkeletonAnimation()
        {
            return _skeletonAnimation;
        }
    }
}

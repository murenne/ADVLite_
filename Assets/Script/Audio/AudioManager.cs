using UnityEngine;
using System.Collections.Generic;

namespace UnityADV.Audio
{
    /// <summary>
    /// 音频管理器
    /// 管理BGM、语音、音效的播放
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private AudioSource[] _seSources = new AudioSource[8];

        [Header("Settings")]
        [SerializeField] private float _bgmVolume = 0.7f;
        [SerializeField] private float _voiceVolume = 1.0f;
        [SerializeField] private float _seVolume = 0.8f;

        private Dictionary<long, AudioSource> _sePlayerMap = new Dictionary<long, AudioSource>();
        private long _nextPlayerId = 1;

        private void Awake()
        {
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (_bgmSource == null)
            {
                var bgmObj = new GameObject("BGM Source");
                bgmObj.transform.SetParent(transform);
                _bgmSource = bgmObj.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.volume = _bgmVolume;
            }

            if (_voiceSource == null)
            {
                var voiceObj = new GameObject("Voice Source");
                voiceObj.transform.SetParent(transform);
                _voiceSource = voiceObj.AddComponent<AudioSource>();
                _voiceSource.loop = false;
                _voiceSource.volume = _voiceVolume;
            }

            for (int i = 0; i < _seSources.Length; i++)
            {
                if (_seSources[i] == null)
                {
                    var seObj = new GameObject($"SE Source {i}");
                    seObj.transform.SetParent(transform);
                    _seSources[i] = seObj.AddComponent<AudioSource>();
                    _seSources[i].loop = false;
                    _seSources[i].volume = _seVolume;
                }
            }
        }

        #region BGM Control

        public void PlayBGM(AudioClip clip, bool loop = true, float volume = -1)
        {
            if (clip == null) return;

            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = volume >= 0 ? volume : _bgmVolume;
            _bgmSource.Play();
        }

        public void StopBGM()
        {
            _bgmSource.Stop();
        }

        #endregion

        #region Voice Control

        public void PlayVoice(AudioClip clip)
        {
            if (clip == null)
                return;

            _voiceSource.clip = clip;
            _voiceSource.Play();
        }

        public void StopVoice()
        {
            _voiceSource.Stop();
        }

        public bool IsVoicePlaying()
        {
            return _voiceSource.isPlaying;
        }

        public float GetVoiceLipSyncValue()
        {
            if (!_voiceSource.isPlaying)
                return 0f;

            // 获取音频输出数据
            float[] samples = new float[256];
            _voiceSource.GetOutputData(samples, 0);

            // 计算平均音量
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }

            return sum / samples.Length;
        }

        #endregion

        #region Sound Control

        public long PlaySound(AudioClip clip, bool loop = false, float volume = 1f)
        {
            if (clip == null) return -1;

            AudioSource source = GetAvailableSoundSource();
            if (source != null)
            {
                source.clip = clip;
                source.loop = loop;
                source.volume = volume * _seVolume;
                source.Play();

                long playerId = _nextPlayerId++;
                _sePlayerMap[playerId] = source;
                return playerId;
            }

            Debug.LogWarning("No available SE source");
            return -1;
        }

        public void StopSound(long playerId)
        {
            if (_sePlayerMap.TryGetValue(playerId, out var source))
            {
                source.Stop();
                _sePlayerMap.Remove(playerId);
            }
        }

        public void StopAllSound()
        {
            foreach (var source in _seSources)
            {
                source.Stop();
            }
            _sePlayerMap.Clear();
        }

        private AudioSource GetAvailableSoundSource()
        {
            foreach (var source in _seSources)
            {
                if (!source.isPlaying)
                    return source;
            }
            return null;
        }

        #endregion

        #region Volume Control

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _bgmVolume;
        }

        public void SetVoiceVolume(float volume)
        {
            _voiceVolume = Mathf.Clamp01(volume);
            _voiceSource.volume = _voiceVolume;
        }

        public void SetSEVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
            foreach (var source in _seSources)
            {
                source.volume = _seVolume;
            }
        }

        #endregion

        public void ClearAll()
        {
            StopBGM();
            StopVoice();
            StopAllSound();
            _sePlayerMap.Clear();
        }
    }
}

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
using System.IO;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV管理器 - Sound
    /// </summary>
    public partial class ADVManager
    {
        // 加载音频配置文件
        private void LoadAudioConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, "AudioConfig", "ADVChapterAudio.json");
                Debug.Log($"[AudioConfig] Loading from: {configPath}");

                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    Debug.Log($"[AudioConfig] JSON content length: {json.Length}");

                    _audioConfig = JsonUtility.FromJson<AudioConfigData>(json);

                    if (_audioConfig != null && _audioConfig.chapters != null)
                    {
                        Debug.Log($"[AudioConfig] Loaded successfully! Found {_audioConfig.chapters.Count} chapters");
                        foreach (var chapter in _audioConfig.chapters)
                        {
                            Debug.Log($"[AudioConfig] - Chapter: {chapter.chapterName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[AudioConfig] Config loaded but chapters is null");
                        _audioConfig = new AudioConfigData();
                    }
                }
                else
                {
                    Debug.LogWarning($"[AudioConfig] File not found: {configPath}");
                    _audioConfig = new AudioConfigData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AudioConfig] Failed to load: {ex.Message}\n{ex.StackTrace}");
                _audioConfig = new AudioConfigData();
            }
        }

        // 预加载章节音频资源
        public void PrepareChapterAudio(string chapterName)
        {
            if (_audioConfig == null || _audioConfig.chapters == null)
            {
                Debug.LogWarning($"Audio config not loaded");
                return;
            }

            // 使用LINQ从列表中查找章节
            var chapterEntry = _audioConfig.chapters.FirstOrDefault(c => c.chapterName == chapterName);
            if (chapterEntry == null || chapterEntry.audioData == null)
            {
                Debug.LogWarning($"No audio config for chapter: {chapterName}");
                return;
            }

            var chapterData = chapterEntry.audioData;

            // 预加载BGM
            foreach (var bgm in chapterData.bgm)
            {
                PrepareAudioClip(bgm);
            }

            // 预加载Voice
            foreach (var voice in chapterData.voice)
            {
                PrepareAudioClip(voice);
            }

            // 预加载Sound
            foreach (var sound in chapterData.sound)
            {
                PrepareAudioClip(sound);
            }

            Debug.Log($"Preparing audio for chapter: {chapterName}");
        }

        // 通用预加载音频资源
        public void PrepareAudioClip(string file)
        {
            if (_audioClipCache.ContainsKey(file))
            {
                Debug.Log($"Audio already loaded: {file}");
                return;
            }

            PrepareAudioClipAsync(file).Forget();
        }

        private async UniTask PrepareAudioClipAsync(string file)
        {
            try
            {
                var clip = await _resourceManager.LoadAssetAsync<AudioClip>(file);
                if (clip != null && !_audioClipCache.ContainsKey(file))
                {
                    _audioClipCache[file] = clip;
                    Debug.Log($"Preloaded audio: {file}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to preload audio {file}: {ex.Message}");
            }
        }

        // BGM
        public void DoCommandPlayBGM(string file)
        {
            if (!_audioClipCache.TryGetValue(file, out AudioClip cachedClip))
            {
                Debug.LogError($"[AudioManager] BGM not preloaded: {file}");
                return;
            }

            _audioManager.PlayBGM(cachedClip);
        }

        public void DoCommandStopBGM()
        {
            _audioManager.StopBGM();
        }

        //  Voice
        public void DoCommandVoiceStart(string file)
        {
            if (!_audioClipCache.TryGetValue(file, out AudioClip cachedClip))
            {
                Debug.LogError($"[AudioManager] Voice not preloaded: {file}");
                return;
            }

            _audioManager.PlayVoice(cachedClip);
        }

        public void DoCommandStopVoice()
        {
            _audioManager.StopVoice();
        }


        // Sound Effect
        public long DoCommandPlaySound(string file, LuaSoundParam param)
        {
            if (_audioClipCache.TryGetValue(file, out AudioClip cachedClip))
            {
                long soundId = _audioManager.PlaySound(cachedClip, param.Loop, param.Volume);
                _sePlayerIds.Add(soundId);
                return soundId;
            }
            else
            {
                Debug.LogWarning($"AudioClip not preloaded: {file}, loading async...");
                return -1;
            }
        }

        public void DoCommandStopSound(long playerId)
        {
            _audioManager.StopSound(playerId);
            _sePlayerIds.Remove(playerId);
        }
    }
}


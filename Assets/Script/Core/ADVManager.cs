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
using Cysharp.Threading.Tasks;
using System;

namespace UnityADV.Core
{
    /// <summary>
    /// ADV主管理器
    /// Unity原生实现，不依赖自定义框架
    /// </summary>
    public partial class ADVManager : MonoBehaviour
    {
        private static ADVManager _instance;
        public static ADVManager Instance => _instance;

        [Header("References")]
        [SerializeField] private ADVUIController _uiController;
        [SerializeField] private AudioManager _audioManager;
        [SerializeField] private ADVConfig _config;

        [Header("Cameras")]
        [SerializeField] private Camera _advCamera;
        [SerializeField] private Camera _spineRenderCamera;

        [Header("Prefabs")]
        [SerializeField] private GameObject _renderTexturePrefab;

        // 核心系统
        private LuaScriptEngine _luaEngine;
        private AddressableResourceManager _resourceManager;
        private LocalizationManager _localizationManager;
        private RenderTexturePool _renderTexturePool;

        // ADV状态
        private ADVState _currentState = ADVState.None;
        private ADVExState _exState = ADVExState.None;
        private bool _yieldFlag = false;
        private List<ADVState> _stateQueue = new List<ADVState>();

        // 计时系统
        private float _prevTime = 0f;
        private float _nowTime = 0f;
        private float _deltaTime = 0f;

        // 文本相关
        private string _currentText = "";
        private string _currentCharacterName = "";
        private float _textStartTime = 0f;
        private int _displayedCharCount = 0;
        private int _currentChapterId = 1; // 默认章节1

        // 对象管理
        private Dictionary<int, ADVObjectView> _objectViewDictionary = new Dictionary<int, ADVObjectView>();
        private int _targetCharacterId = 0;
        private int _previousTargetCharacterId = 0;

        // 跳过控制
        private bool _startSkip = false;
        private bool _isHardSkip = false;
        private bool _isSoftSkip = false;

        // ✅ 2. Auto Mode（自动播放）
        private bool _autoModeFlag = false;
        private float _autoModeRestTime = 0f;

        // ✅ 9. 任务管理
        private List<Tween> _tweenTasks = new List<Tween>();
        private List<UniTask> _uniTasks = new List<UniTask>();

        // 其他
        private List<BackLogItem> _backLogItemList = new List<BackLogItem>();
        private float _waitRestTime = 0f;

        // 音频管理
        private List<long> _sePlayerIds = new List<long>();
        private AudioConfigData _audioConfig;
        private Dictionary<string, AudioClip> _audioClipCache = new Dictionary<string, AudioClip>();

        public ADVUIController UIController => _uiController;
        public AudioManager AudioManager => _audioManager;

        public LocalizationManager LocalizationManager => _localizationManager;



        public CancellationToken _cancellationToken; // 接受取消
        private readonly List<PsAdvResourceBase> _preparedResourceList = new();

        public LuaScriptEngine LuaEngine => _luaEngine;

        private void Awake()
        {
            // 单例模式
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);

                PrepareAsync().Forget();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化ADV系统
        /// </summary>
        private async UniTask PrepareAsync()
        {
            Debug.Log("=== ADV Manager Initializing ===");

            var tasks = new List<UniTask>();

            // 任务1：初始化 Lua
            tasks.Add(UniTask.Create(async () =>
            {
                _luaEngine = new LuaScriptEngine(this);
                await _luaEngine.Prepare();
            }));

            // 任务2：初始化资源管理器
            //tasks.Add(UniTask.Create(async () =>
            //{
            _resourceManager = new AddressableResourceManager();
            //}));

            // 任务3：初始化 RenderTexture 池
            //tasks.Add(UniTask.Create(async () =>
            //{
            _renderTexturePool = new RenderTexturePool(_config.renderTextureSize);
            _renderTexturePool.Prepare(5);
            //}));

            // 任务4：初始化本地化
            _localizationManager = new LocalizationManager();
            _localizationManager.CurrentLanguage = LocalizationManager.GetInitialLanguage();  // 优先使用保存的设置
            _localizationManager.LoadData();  // 加载JSON数据（基础+覆盖）

            // 任务5：初始化音频配置
            LoadAudioConfig();

            // 等待所有任务完成
            await UniTask.WhenAll(tasks);

            // 设置相机
            SetupCameras();

            StartADV("Chapter01", 1, 0);// TODO for test

            Debug.Log("=== ADV Manager Initialized ===");
        }

        /// <summary>
        /// 设置ADV相机系统
        /// </summary>
        private void SetupCameras()
        {
            // 1. 设置ADV主相机
            if (_advCamera == null)
            {
                _advCamera = Camera.main;
                if (_advCamera == null)
                {
                    Debug.LogError("ADV Camera not assigned and no Main Camera found!");
                    return;
                }
            }

            // 2. 设置Canvas的渲染相机
            if (_uiController != null)
            {
                var canvas = _uiController.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.worldCamera = _advCamera;
                    canvas.planeDistance = 10f; // 设置Canvas距离相机的距离
                    Debug.Log("Canvas worldCamera set to ADV Camera");
                }
            }

            // 3. 设置Spine渲染相机
            if (_spineRenderCamera != null)
            {
                // 确保Spine渲染相机的配置正确
                _spineRenderCamera.clearFlags = CameraClearFlags.SolidColor;
                _spineRenderCamera.backgroundColor = new Color(0, 0, 0, 0); // 透明背景
                _spineRenderCamera.cullingMask = LayerMask.GetMask("SpineRender"); // 只渲染SpineRender层
                _spineRenderCamera.depth = -1; // 比主相机低
                _spineRenderCamera.enabled = false; // 手动控制渲染
                Debug.Log("Spine Render Camera configured");
            }
            else
            {
                Debug.LogWarning("Spine Render Camera not assigned! Spine characters may not render correctly.");
            }
        }

        private void Update()
        {
            // Lua Tick
            _luaEngine?.Tick();
        }

        /// <summary>
        /// 开始播放ADV
        /// </summary>
        public async UniTask PlayADV(string scriptName, int startLine, CancellationToken cancellationToken)
        {
            try
            {
                _cancellationToken = cancellationToken;

                // 初始化模型
                _startSkip = startLine > 0;

                Debug.Log($"Starting ADV: {scriptName}");

                // 加载脚本
                if (startLine > 0)
                {
                    _luaEngine.LoadScriptWithSkip(scriptName, startLine);
                }
                else
                {
                    _luaEngine.LoadScript(scriptName);
                }

                // 获取启动函数
                var startFunc = _luaEngine.GetFunction<System.Action>("ADV_Start");
                if (startFunc == null)
                {
                    Debug.LogError($"ADV_Start function not found in script: {scriptName}!");
                    return;
                }

                // 切换到 Script 状态
                _currentState = ADVState.Script;

                // 启动脚本
                StartScript(startFunc);

                // 进入帧循环
                await FrameLoop();

                Debug.Log("ADV Ended");

                // 停止音频
                _audioManager?.StopBGM();
                _audioManager?.StopVoice();

                // 安全等待
                await UniTask.Delay(100, cancellationToken: cancellationToken);

                // TODO: return to title or other logic
            }
            catch (OperationCanceledException)
            {
                Debug.Log("ADV Cancelled");
                throw;
            }
            catch (XLua.LuaException e)
            {
                Debug.LogError($"Lua Exception: {e.Message}\n{e.StackTrace}");
                throw;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception in Play: {e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// 启动脚本
        /// </summary>
        private void StartScript(System.Action startFunc)
        {
            try
            {
                startFunc();// = startFunc.Invoke();
            }
            catch (XLua.LuaException e)
            {
                var errorFile = _luaEngine.GetGlobal<string>("errorFile");
                var errorLine = _luaEngine.GetGlobal<int>("errorLine");
                Debug.LogError($"Lua Error {errorFile}({errorLine})\n{e.Message}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception in StartScript: {e.Message}");
            }
        }

        public void StartADV(string scriptName, int chapterId = 1, int startLine = 0)
        {
            _currentChapterId = chapterId;
            PlayADV(scriptName, startLine, _cancellationToken).Forget();
        }

        public void StopADV()
        {
            _currentState = ADVState.End;
        }

        public void SetAutoMode(bool enable)
        {
            _autoModeFlag = enable;
        }
    }
}

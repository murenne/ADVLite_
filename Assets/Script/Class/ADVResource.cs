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
    public abstract class PsAdvResourceBase
    {
        public abstract void Dispose();
        public abstract AsyncOperationHandle GetHandleBase();
    }

    /// <summary>
    /// Addressables 资源包装类
    /// </summary>
    public class ADVResource<T> : PsAdvResourceBase where T : class
    {
        private AsyncOperationHandle<T> _handle;
        private AddressableResourceManager _resourceManager;

        public T Result => _handle.Result;

        public ADVResource(AddressableResourceManager resourceManager, AsyncOperationHandle<T> handle)
        {
            _resourceManager = resourceManager;
            _handle = handle;
        }

        public override AsyncOperationHandle GetHandleBase()
        {
            return _handle;
        }

        public override void Dispose()
        {
            if (_handle.IsValid())
            {
                _resourceManager.Release(_handle);
            }
        }

        public AsyncOperationHandle<T> GetHandle()
        {
            return _handle;
        }
    }
}

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UnityADV.Resource
{
    /// <summary>
    /// Addressables资源管理器
    /// 替代原框架的GsAddressableStock
    /// </summary>
    public class AddressableResourceManager
    {
        private class ResourceValue
        {
            public string Address;
            public int RefCount;
        }

        private Dictionary<AsyncOperationHandle, ResourceValue> _handleDictionary = new();

        /// <summary>
        /// 加载资源
        /// </summary>
        public AsyncOperationHandle<T> LoadAssetAsync<T>(string key) where T : class
        {
            var handle = Addressables.LoadAssetAsync<T>(key);

            if (_handleDictionary.TryGetValue(handle, out var value))
            {
                value.RefCount++;
            }
            else
            {
                _handleDictionary.Add(handle, new ResourceValue { Address = key, RefCount = 1 });
            }

            return handle;
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        public AsyncOperationHandle<T> AddRef<T>(AsyncOperationHandle<T> handle) where T : class
        {
            if (!handle.IsValid())
                return handle;

            Addressables.ResourceManager.Acquire(handle);

            return handle;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Release(AsyncOperationHandle handle)
        {
            if (!handle.IsValid())
                return;

            Addressables.Release(handle);

            // 引用计数 -1
            if (_handleDictionary.TryGetValue(handle, out var value))
            {
                value.RefCount--;
                if (value.RefCount <= 0)
                {
                    _handleDictionary.Remove(handle);
                }
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            // 如果还有未释放的句柄，则输出日志
            if (_handleDictionary.Count > 0)
            {
                foreach (var value in _handleDictionary.Values)
                {
                    Debug.LogError($"[ResourceManager] 未释放的资源: {value.Address} (RefCount: {value.RefCount})");
                }
            }
            else
            {
                Debug.Log("[ResourceManager] 没有未释放的资源");
            }
        }
    }
}

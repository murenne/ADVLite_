using UnityEngine;
using System.Collections.Generic;

namespace UnityADV.Resource
{
    /// <summary>
    /// RenderTexture对象池
    /// 用于Spine角色离屏渲染
    /// </summary>
    public class RenderTexturePool
    {
        private Queue<RenderTexture> _availableTextures = new Queue<RenderTexture>();
        private HashSet<RenderTexture> _activeTextures = new HashSet<RenderTexture>();
        private int _textureSize;
        private RenderTextureFormat _format;

        public RenderTexturePool(int textureSize = 1200, RenderTextureFormat format = RenderTextureFormat.ARGB32)
        {
            _textureSize = textureSize;
            _format = format;
        }

        /// <summary>
        /// 预创建指定数量的纹理
        /// </summary>
        public void Prepare(int count = 5)
        {
            for (int i = 0; i < count; i++)
            {
                var texture = CreateTexture();
                _availableTextures.Enqueue(texture);
            }
        }

        /// <summary>
        /// 获取一个RenderTexture
        /// </summary>
        public RenderTexture Acquire()
        {
            RenderTexture texture;

            if (_availableTextures.Count > 0)
            {
                texture = _availableTextures.Dequeue();
            }
            else
            {
                texture = CreateTexture();
                Debug.Log($"RenderTexturePool: Created new texture. Active count: {_activeTextures.Count + 1}");
            }

            _activeTextures.Add(texture);
            return texture;
        }

        /// <summary>
        /// 归还RenderTexture到池中
        /// </summary>
        public void Release(RenderTexture texture)
        {
            if (texture == null)
                return;

            if (_activeTextures.Remove(texture))
            {
                // 清理纹理
                texture.Release();
                _availableTextures.Enqueue(texture);
            }
        }

        /// <summary>
        /// 清理所有纹理
        /// </summary>
        public void Clear()
        {
            // 清理可用纹理
            while (_availableTextures.Count > 0)
            {
                var texture = _availableTextures.Dequeue();
                if (texture != null)
                {
                    texture.Release();
                    Object.Destroy(texture);
                }
            }

            // 清理活动纹理
            foreach (var texture in _activeTextures)
            {
                if (texture != null)
                {
                    texture.Release();
                    Object.Destroy(texture);
                }
            }
            _activeTextures.Clear();
        }

        /// <summary>
        /// 创建新的RenderTexture
        /// </summary>
        private RenderTexture CreateTexture()
        {
            var texture = new RenderTexture(_textureSize, _textureSize, 0, _format);
            texture.name = $"ADV_RenderTexture_{System.Guid.NewGuid()}";
            texture.Create();
            return texture;
        }

        /// <summary>
        /// 获取池状态信息
        /// </summary>
        public string GetPoolInfo()
        {
            return $"RenderTexturePool - Available: {_availableTextures.Count}, Active: {_activeTextures.Count}";
        }
    }
}

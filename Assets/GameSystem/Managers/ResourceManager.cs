using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameSystem.Managers
{
    /// <summary>
    /// Addressables 기반의 리소스 관리 매니저
    /// 초기화, 에셋 로드 중복 방지, 인스턴스 생명주기 관리를 수행합니다.
    /// </summary>
    public class ResourceManager
    {
        private readonly object _initGate = new();
        private readonly object _loadGate = new();

        private volatile bool _isInitialized;
        private Task _initTask;

        private readonly Dictionary<object, Task<UnityEngine.Object>> _loadingTasks = new();
        private readonly Dictionary<object, AsyncOperationHandle> _assetHandles = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

        public UniTask InitAsync()
        {
            if (_isInitialized)
                return UniTask.CompletedTask;

            lock (_initGate)
            {
                if (_isInitialized)
                    return UniTask.CompletedTask;

                _initTask ??= DoInitAsync().AsTask();
                return _initTask.AsUniTask();
            }
        }

        private async UniTask DoInitAsync()
        {
            try
            {
                var handle = Addressables.InitializeAsync();
                await handle.ToUniTask();

                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception("[Addressables] Initialization failed.");

                _isInitialized = true;
            }
            catch
            {
                lock (_initGate)
                {
                    _initTask = null;
                }
                throw;
            }
        }

        public async UniTask<T> LoadAssetAsync<T>(object key, CancellationToken ct = default)
            where T : UnityEngine.Object
        {
            await InitAsync();

            Task<UnityEngine.Object> sharedTask;

            lock (_loadGate)
            {
                if (_assetHandles.TryGetValue(key, out var loadedHandle))
                {
                    if (loadedHandle.IsValid())
                    {
                        if (loadedHandle.Result is T loadedAsset)
                            return loadedAsset;

                        throw new InvalidOperationException(
                            $"[ResourceManager] Asset '{key}' is already loaded as different type.");
                    }

                    _assetHandles.Remove(key);
                }

                if (!_loadingTasks.TryGetValue(key, out sharedTask))
                {
                    sharedTask = ExecuteLoadAsync<T>(key);
                    _loadingTasks[key] = sharedTask;
                }
            }

            var result = await sharedTask.AsUniTask().AttachExternalCancellation(ct);

            if (result is T typedResult)
                return typedResult;

            throw new InvalidOperationException(
                $"[ResourceManager] Type mismatch for '{key}'. Requested: {typeof(T).Name}");
        }

        private async Task<UnityEngine.Object> ExecuteLoadAsync<T>(object key)
            where T : UnityEngine.Object
        {
            var handle = Addressables.LoadAssetAsync<T>(key);

            try
            {
                T asset = await handle.Task;

                if (handle.Status != AsyncOperationStatus.Succeeded || asset == null)
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);

                    throw new Exception($"[Addressables] Load failed: {key}");
                }

                lock (_loadGate)
                {
                    _assetHandles[key] = handle;
                    _loadingTasks.Remove(key);
                }

                return asset;
            }
            catch
            {
                if (handle.IsValid())
                    Addressables.Release(handle);

                lock (_loadGate)
                {
                    _loadingTasks.Remove(key);
                }

                throw;
            }
        }

        public bool TryGetLoadedAsset<T>(object key, out T asset)
            where T : UnityEngine.Object
        {
            asset = null;

            lock (_loadGate)
            {
                if (!_assetHandles.TryGetValue(key, out var handle))
                    return false;

                if (!handle.IsValid())
                {
                    _assetHandles.Remove(key);
                    return false;
                }

                asset = handle.Result as T;
                return asset != null;
            }
        }

        public async UniTask<GameObject> InstantiateAsync(
            object key,
            Transform parent = null,
            bool worldSpace = false,
            CancellationToken ct = default)
        {
            await InitAsync();

            var handle = Addressables.InstantiateAsync(key, parent, worldSpace);

            try
            {
                GameObject instance = await handle.ToUniTask(cancellationToken: ct);

                if (instance == null || handle.Status != AsyncOperationStatus.Succeeded)
                {
                    if (handle.IsValid())
                        Addressables.ReleaseInstance(handle);

                    throw new InvalidOperationException($"[Addressables] Instantiate failed: {key}");
                }

                instance.name = instance.name.Replace("(Clone)", "");

                lock (_loadGate)
                {
                    _instanceHandles[instance] = handle;
                }

                return instance;
            }
            catch
            {
                if (handle.IsValid())
                    Addressables.ReleaseInstance(handle);

                throw;
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null)
                return;

            lock (_loadGate)
            {
                if (_instanceHandles.Remove(instance, out var handle))
                {
                    if (handle.IsValid())
                        Addressables.ReleaseInstance(handle);
                }
                else
                {
                    UnityEngine.Object.Destroy(instance);
                }
            }
        }

        /// <summary>
        /// 이미 로드 완료된 에셋만 해제합니다.
        /// 현재 로딩 중인 작업은 취소하지 않습니다.
        /// </summary>
        public void ReleaseAsset(object key)
        {
            lock (_loadGate)
            {
                if (_assetHandles.Remove(key, out var handle))
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }
            }
        }

        /// <summary>
        /// 현재 추적 중인 인스턴스와 로드 완료 에셋을 해제합니다.
        /// 로딩 중인 작업 자체를 강제로 취소하지는 않으며, 로딩 작업 추적 정보만 제거합니다.
        /// </summary>
        public void ReleaseAll()
        {
            lock (_loadGate)
            {
                foreach (var handle in _instanceHandles.Values)
                {
                    if (handle.IsValid())
                        Addressables.ReleaseInstance(handle);
                }
                _instanceHandles.Clear();

                foreach (var handle in _assetHandles.Values)
                {
                    if (handle.IsValid())
                        Addressables.Release(handle);
                }
                _assetHandles.Clear();

                _loadingTasks.Clear();
            }
        }
    }
}
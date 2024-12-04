﻿// Assets/Scripts/WebSocketStream.cs

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using PinionCore.Network;
using PinionCore.Remote;
using UnityEngine;
using UnityEngine.Scripting;

namespace PinionCore.NetSync.Web
{
    public class WebSocketStream : IStreamable
    {
        // 回调委托定义
        public delegate void OnOpenCallback(int instanceId);
        public delegate void OnMessageCallback(int instanceId, IntPtr bufferPtr, int length);
        public delegate void OnErrorCallback(int instanceId, IntPtr errorPtr);
        public delegate void OnCloseCallback(int instanceId, int closeCode);

        // 导入 JavaScript 函数
        [DllImport("__Internal")]
        public static extern void WebSocketSetOnOpen(OnOpenCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnMessage(OnMessageCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnError(OnErrorCallback callback);

        [DllImport("__Internal")]
        public static extern void WebSocketSetOnClose(OnCloseCallback callback);

        [DllImport("__Internal")]
        public static extern int WebSocketAllocate([MarshalAs(UnmanagedType.LPStr)] string url);

        [DllImport("__Internal")]
        public static extern void WebSocketAddSubProtocol(int instanceId, [MarshalAs(UnmanagedType.LPStr)] string subprotocol);

        [DllImport("__Internal")]
        public static extern int WebSocketConnect(int instanceId);

        [DllImport("__Internal")]
        public static extern int WebSocketClose(int instanceId, int code, [MarshalAs(UnmanagedType.LPStr)] string reason);

        [DllImport("__Internal")]
        public static extern int WebSocketSend(int instanceId, byte[] data, int length);

        [DllImport("__Internal")]
        public static extern int WebSocketGetState(int instanceId);

        // 实例映射，方便根据 ID 获取实例
        private static readonly Dictionary<int, WebSocketStream> _instances = new Dictionary<int, WebSocketStream>();

        private readonly int _instanceId;
        private readonly PinionCore.Network.Stream _stream;
        private readonly IStreamable _streamable;

        // 事件，供用户订阅
        public event Action OnOpen;
        public event Action<string> OnError;
        public event Action OnClose;

        // 静态构造函数，初始化回调函数
        static WebSocketStream()
        {
            WebSocketSetOnOpen(WebSocketOnOpen);
            WebSocketSetOnMessage(WebSocketOnMessage);
            WebSocketSetOnError(WebSocketOnError);
            WebSocketSetOnClose(WebSocketOnClose);
        }

        public WebSocketStream(string url)
        {
            _instanceId = WebSocketAllocate(url);

            lock (_instances)
            {
                _instances[_instanceId] = this;
            }

            _stream = new PinionCore.Network.Stream();
            _streamable = _stream;
        }

        public void Connect()
        {
            WebSocketConnect(_instanceId);
        }

        public void Close()
        {
            WebSocketClose(_instanceId, 1000, "Normal Closure");

            lock (_instances)
            {
                _instances.Remove(_instanceId);
            }
        }

        public IWaitableValue<int> Receive(byte[] buffer, int offset, int count)
        {
            UnityEngine.Debug.Log($"Receive: {count}");
            return _streamable.Receive(buffer, offset, count);
        }

        public IWaitableValue<int> Send(byte[] buffer, int offset, int count)
        {
            return _Send(buffer, offset, count).ToWaitableValue();
        }

        async Task<int> _Send(byte[] buffer, int offset, int count)
        {
            UnityEngine.Debug.Log($"Send: {count}");
            var sended = await _streamable.Send(buffer, offset, count);
            var newBuf = new byte[sended];
            sended = await _stream.Pop(newBuf, 0, newBuf.Length);
            if (sended == 0)
            {
                UnityEngine.Debug.Log($"Send count == 0");
                return 0;
            }
            WebSocketSend(data: newBuf, length: sended, instanceId: _instanceId);
            return sended;
        }

        

        // 回调方法
        [MonoPInvokeCallback(typeof(OnOpenCallback))]
        [Preserve]
        private static void WebSocketOnOpen(int instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                UnityEngine.Debug.Log("WebSocketOnOpen");
                instance.OnOpen?.Invoke();
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageCallback))]
        [Preserve]
        private static async void WebSocketOnMessage(int instanceId, IntPtr bufferPtr, int length)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                UnityEngine.Debug.Log($"WebSocketOnMessage: {length}");
                byte[] data = new byte[length];
                Marshal.Copy(bufferPtr, data, 0, length);
                var pushCount = await instance._stream.Push(data, 0, length);
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorCallback))]
        [Preserve]
        private static void WebSocketOnError(int instanceId, IntPtr errorPtr)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                string errorMessage = Marshal.PtrToStringUTF8(errorPtr);
                instance.OnError?.Invoke(errorMessage);
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseCallback))]
        [Preserve]
        private static void WebSocketOnClose(int instanceId, int closeCode)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                instance.OnClose?.Invoke();
            }
        }
    }
}

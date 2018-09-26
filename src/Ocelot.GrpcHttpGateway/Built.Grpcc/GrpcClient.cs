using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Built.Grpcc
{
    public class GrpcClient : ClientBase<GrpcClient>
    {
        public GrpcClient(Channel channel) : base(channel)
        {
        }

        public GrpcClient(CallInvoker callInvoker) : base(callInvoker)
        {
        }

        public GrpcClient() : base()
        {
        }

        protected GrpcClient(ClientBaseConfiguration configuration) : base(configuration)
        {
        }

        public K Invoke<T, K>(MethodDescriptor method, T request) where T : class, IMessage<T> where K : class, IMessage<K>
        {
            var _method = GrpcMethod<T, K>.GetMethod(method);
            return CallInvoker.BlockingUnaryCall(_method, null, GetDefaultCallOptions(), request);
        }

        public AsyncUnaryCall<K> InvokeAsync<T, K>(MethodDescriptor method, T request) where T : class, IMessage<T> where K : class, IMessage<K>
        {
            var _method = GrpcMethod<T, K>.GetMethod(method);
            return CallInvoker.AsyncUnaryCall(_method, null, GetDefaultCallOptions(), request);
        }

        public virtual string Invoke<T, K>(MethodDescriptor method, string message) where T : class, IMessage<T> where K : class, IMessage<K>
        {
            var req = ArgsParser<T>.Parser.ParseJson(message);
            var _method = GrpcMethod<T, K>.GetMethod(method);
            return CallInvoker.BlockingUnaryCall(_method, null, GetDefaultCallOptions(), req).ToString();
        }

        public virtual async Task<string> InvokeAsync<T, K>(MethodDescriptor method, string message)
            where T : class, IMessage<T> where K : class, IMessage<K>
        {
            return await InvokeAsync<T, K>(method, message, GetDefaultCallOptions());
        }

        public virtual async Task<string> InvokeAsync<T, K>(MethodDescriptor method, string message, CallOptions callOptions)
            where T : class, IMessage<T> where K : class, IMessage<K>
        {
            return await InvokeAsync<T, K>(method, message, GetDefaultCallOptions(), null);
        }

        public virtual async Task<string> InvokeAsync<T, K>(MethodDescriptor methodDescript, string message, CallOptions callOptions, string host)
            where T : class, IMessage<T> where K : class, IMessage<K>
        {
            var request = ArgsParser<T>.Parser.ParseJson(message);
            var method = GrpcMethod<T, K>.GetMethod(methodDescript);
            K response = await CallInvoker.AsyncUnaryCall(method, host, callOptions, request);
            return response.ToString();
        }

        protected override GrpcClient NewInstance(ClientBaseConfiguration configuration)
        {
            return new GrpcClient(configuration);
        }

        ///需要添加CallOptions参数,为请求添加更多header
        public object InvokeMethod(MethodDescriptor method, string message)
        {
            object[] obj = { method, message };
            string name = "Invoke";
            Type[] parameterType = { typeof(MethodDescriptor), typeof(string) };
            return InvokeMethod(this, method, parameterType, name, obj);
        }

        public async Task<string> InvokeMethodAsync(MethodDescriptor method, string message)
        {
            return await InvokeMethodAsync(method, message, GetDefaultCallOptions());
        }

        public async Task<string> InvokeMethodAsync(MethodDescriptor method, string message, IDictionary<string, string> headers)
        {
            return await InvokeMethodAsync(method, message, CreateCallOptions(headers));
        }

        public async Task<string> InvokeMethodAsync(MethodDescriptor method, string message, CallOptions callOptions)
        {
            return await InvokeMethodAsync(method, message, callOptions, null);
        }

        public async Task<string> InvokeMethodAsync(MethodDescriptor method, string message, CallOptions callOptions, string host)
        {
            object[] obj = { method, message, callOptions, host };
            string name = "InvokeAsync";
            Type[] parameterType = { typeof(MethodDescriptor), typeof(string), typeof(CallOptions), typeof(string) };
            Type[] templateTypeSet = { method.InputType.ClrType, method.OutputType.ClrType };
            var task = InvokeMethodAsync(typeof(GrpcClient), this, templateTypeSet, parameterType, name, obj);
            return await (task as Task<string>);
        }

        private static object InvokeMethod(GrpcClient grpcClient, MethodDescriptor method, Type[] parameterType, string name, params object[] args)
        {
            Type clsType = typeof(GrpcClient);
            Type[] templateTypeSet = { method.InputType.ClrType, method.OutputType.ClrType };
            MethodInfo methodInfo = clsType.GetMethod(name, parameterType);
            methodInfo = methodInfo.MakeGenericMethod(templateTypeSet);
            return methodInfo.Invoke(grpcClient, args);
        }

        /// <summary>
        /// 不转task了
        /// </summary>
        private static object InvokeMethodAsync(Type clsType, object classInstance, Type[] templateTypeSet, Type[] parameterType, string name, params object[] args)
        {
            MethodInfo method = clsType.GetMethod(name, parameterType);
            method = method.MakeGenericMethod(templateTypeSet);
            return method.Invoke(classInstance, args);
        }

        private CallOptions GetDefaultCallOptions(int second = 30)
        {
            return new CallOptions(null, DateTime.UtcNow.AddSeconds(second), default(CancellationToken));
        }

        private CallOptions CreateCallOptions(IDictionary<string, string> headers)
        {
            Metadata meta = new Metadata();
            foreach (KeyValuePair<string, string> entry in headers)
            {
                meta.Add(entry.Key, entry.Value);
            }
            CallOptions option = new CallOptions(meta);
            return option;
        }
    }
}
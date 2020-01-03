using Grpc.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Built.Grpc.Middleware
{
    /// <summary>
    /// Demo middleware.
    /// </summary>
    public class PipelineBuilder
    {
        private List<Func<PipelineDelagate, PipelineDelagate>> middlewares = new List<Func<PipelineDelagate, PipelineDelagate>>();

        /// <summary>
        /// Add a middleware
        /// </summary>
        public PipelineBuilder Use(Func<PipelineDelagate, PipelineDelagate> middleware)
        {
            middlewares.Add(middleware);
            return this;
        }

        /// <summary>
        /// Adds a middleware class to the pipeline. Must have CTOR with single parameter of type <see cref="PipelineDelagate"/>
        /// and method "Invoke" accepting <see cref="PipelineDelagate"/> and returning <see cref="Task"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PipelineBuilder Use<T>(params object[] args)
        {
            middlewares.Add(d => WrapClass<T>(d, args));
            return this;
        }

        /// <summary>
        /// Conditionally adds a middleware class to the pipeline. Must have CTOR with single parameter of type <see cref="PipelineDelagate"/>
        /// and method "Invoke" accepting <see cref="PipelineDelagate"/> and returning <see cref="Task"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <returns></returns>
        public PipelineBuilder UseWhen<T>(Func<MiddlewareContext, bool> condition, params object[] args)
        {
            middlewares.Add(d =>
            {
                return async ctx => { if (condition(ctx)) { await WrapClass<T>(d, args)(ctx); } };
            });
            return this;
        }

        /// <summary>
        /// Add a middleware delegate.
        /// </summary>
        /// <param name="middleware"></param>
        /// <returns></returns>
        public PipelineBuilder Use(Func<MiddlewareContext, PipelineDelagate, Task> middleware)
        {
            middlewares.Add(d =>
            {
                return ctx => { return middleware(ctx, d); };
            });
            return this;
        }

        private PipelineDelagate WrapClass<T>(PipelineDelagate next, params object[] args)
        {
            var ctorArgs = new object[args.Length + 1];
            ctorArgs[0] = next;
            Array.Copy(args, 0, ctorArgs, 1, args.Length);
            var type = typeof(T);
            var instance = Activator.CreateInstance(type, ctorArgs);
            MethodInfo method = type.GetMethod("Invoke");
            return (PipelineDelagate)method.CreateDelegate(typeof(PipelineDelagate), instance);
        }

        /// <summary>
        /// Builds the <see cref="Pipeline"/> for use with <see cref="Server"/>
        /// </summary>
        /// <returns></returns>
        public Pipeline Build()
        {
            PipelineDelagate pipeline = ExecuteMainHandler;
            middlewares.Reverse();
            foreach (var middleware in middlewares)
            {
                pipeline = middleware(pipeline);
            }
            return new Pipeline(pipeline);
        }

        internal static Task ExecuteMainHandler(MiddlewareContext context)
        {
            return context.HandlerExecutor();
        }
    }

    /// <summary>
    /// Built pipeline for gRPC
    /// </summary>
    public class Pipeline
    {
        private PipelineDelagate processChain;

        internal Pipeline(PipelineDelagate middlewareChain)
        {
            processChain = middlewareChain;
        }

        internal Task RunPipeline(MiddlewareContext context)
        {
            return processChain(context);
        }
    }
}
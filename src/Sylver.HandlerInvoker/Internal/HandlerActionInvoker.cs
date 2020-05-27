﻿using Sylver.HandlerInvoker.Exceptions;
using Sylver.HandlerInvoker.Internal.Transformers;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Sylver.HandlerInvoker.Internal
{
    /// <summary>
    /// Provides methods to invoke a handler action.
    /// </summary>
    internal sealed class HandlerActionInvoker : IHandlerInvoker
    {
        private readonly HandlerActionInvokerCache _invokerCache;
        private readonly IParameterTransformer _parameterTransformer;

        /// <summary>
        /// Creates a new <see cref="HandlerActionInvoker"/> instance.
        /// </summary>
        /// <param name="invokerCache">Handler action invoker cache.</param>
        /// <param name="parameterTransformer">Parameter transformer.</param>
        public HandlerActionInvoker(HandlerActionInvokerCache invokerCache, IParameterTransformer parameterTransformer)
        {
            _invokerCache = invokerCache;
            _parameterTransformer = parameterTransformer;
        }

        /// <inheritdoc />
        public object Invoke(object handlerAction, params object[] args)
        {
            HandlerActionInvokerCacheEntry handlerActionInvoker = _invokerCache.GetCachedHandlerAction(handlerAction);

            if (handlerActionInvoker == null)
            {
                throw new HandlerActionNotFoundException(handlerAction);
            }

            var targetHandler = handlerActionInvoker.HandlerFactory(handlerActionInvoker.HandlerType);

            if (targetHandler == null)
            {
                throw new HandlerTargetCreationFailedException(handlerActionInvoker.HandlerType);
            }

            object handlerResult = null;

            try
            {
                object[] handlerActionParameters = PrepareParameters(args, handlerActionInvoker.HandlerExecutor);

                handlerResult = handlerActionInvoker.HandlerExecutor.Execute(targetHandler, handlerActionParameters);
            }
            catch
            {
                throw;
            }
            finally
            {
                handlerActionInvoker.HandlerReleaser(targetHandler);
            }

            return handlerResult;
        }

        /// <inheritdoc />
        public Task InvokeAsync(object handlerAction, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Prepare the invoke parameters. Adds default values if a parameter is missing.
        /// </summary>
        /// <param name="originalParameters">Original parameters.</param>
        /// <param name="executor">Handler executor.</param>
        /// <returns>Handler parameters.</returns>
        private object[] PrepareParameters(object[] originalParameters, HandlerExecutor executor)
        {
            if (!executor.MethodParameters.Any())
            {
                return null;
            }

            var parameters = new object[executor.MethodParameters.Count()];

            for (var i = 0; i < parameters.Length; i++)
            {
                ParameterInfo methodParameterInfo = executor.MethodParameters.ElementAt(i);

                if (i < originalParameters.Length)
                {
                    Type originalObjectType = originalParameters[i]?.GetType();

                    if (!methodParameterInfo.ParameterType.IsAssignableFrom(originalObjectType))
                    {
                        object transformedParameter = _parameterTransformer.Transform(originalParameters[i], methodParameterInfo.ParameterType.GetTypeInfo());
                        
                        parameters[i] = transformedParameter ?? executor.GetDefaultValueForParameter(i);
                    }
                    else
                    {
                        parameters[i] = originalParameters[i];
                    }
                }
                else
                {
                    parameters[i] = executor.GetDefaultValueForParameter(i);
                }
            }

            return parameters;
        }
    }
}

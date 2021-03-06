﻿using System.Threading.Tasks;

namespace Sylver.HandlerInvoker
{
    /// <summary>
    /// Provides methods to invoke a handler action.
    /// </summary>
    public interface IHandlerInvoker
    {
        /// <summary>
        /// Invokes a handler action.
        /// </summary>
        /// <param name="handlerAction">Handler action.</param>
        /// <param name="args">Handler action parameters.</param>
        /// <returns>Handler action result.</returns>
        object Invoke(object handlerAction, params object[] args);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handlerAction"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        Task InvokeAsync(object handlerAction, params object[] args);
    }
}

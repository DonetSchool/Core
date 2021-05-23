using DonetSchool.QoS.Config;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.QoS
{
    public interface IQoSProcessor
    {
        Task ExecuteAsync(Func<bool, Context, QoSConfig, CancellationToken, Task> action, RequestInfo requestInfo, Dictionary<string, object> pollyContextData = null);

        Task ExecuteAsync(Func<bool, Context, QoSConfig, CancellationToken, Task> action, RequestInfo requestInfo, CancellationToken cancellationToken, Dictionary<string, object> pollyContextData = null);

        Task<TResult> ExecuteAsync<TResult>(Func<bool, Context, QoSConfig, CancellationToken, Task<TResult>> action, RequestInfo requestInfo, Dictionary<string, object> pollyContextData = null);

        Task<TResult> ExecuteAsync<TResult>(Func<bool, Context, QoSConfig, CancellationToken, Task<TResult>> action, RequestInfo requestInfo, CancellationToken cancellationToken, Dictionary<string, object> pollyContextData = null);
    }
}
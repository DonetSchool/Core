# .NetCore Web 服务器介绍

.Net Core 有两种web服务：

1. [Kestrel 服务器](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-3.1)是默认跨平台 HTTP 服务器实现。 Kestrel 提供了最佳性能和内存利用率，但它没有 HTTP.sys 中的某些高级功能。 有关详细信息，请参阅下一部分中的 [Kestrel 与 HTTP.sys](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/?view=aspnetcore-3.1&tabs=windows#korh)。
2. [HTTP.sys 服务器](https://docs.microsoft.com/zh-cn/aspnet/core/fundamentals/servers/httpsys?view=aspnetcore-3.1)是仅用于 Windows 的 HTTP 服务器，它基于 [HTTP.sys 核心驱动程序和 HTTP 服务器 API](https://docs.microsoft.com/zh-cn/windows/desktop/Http/http-api-start-page)。

大致上Htpp处理流程如下：

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E8%AF%B7%E6%B1%82%E5%A4%84%E7%90%86%E6%B5%81%E7%A8%8B.png)

因此当有大批量的慢处理堆积在ThreadPool上时，后续所有的请求都会变慢，慢慢的导致整个站点不可用。

**当有一个接口响应速度慢的时候，会导致整个站点响应速度慢，并且会影响到整个站点不可用。**

# Web服务质量（QoS）保障利器

## 简介

QoS的英文全称为"Quality of Service"，中文名为"服务质量"。QoS是网络的一种安全机制, 是用来解决网络延迟和阻塞等问题的一种技术。当网络过载或拥塞时，QoS 能确保重要业务量不受延迟或丢弃，同时保证网络的高效运行。翻译到web上来讲，当web服务器请求过载或拥塞时，QoS能确保重要业务不受延迟或丢弃，所谓弃车保帅。

## 降级

服务降级是当服务器压力剧增时，根据当前业务情况及流量对一些服务和页面有策略的降级，以此释放服务器资源以保证核心任务的正常运行。降级也会指定不同的级别，面临不同的异常等级执行不同降级措施。可以拒接服务，也可以延迟服务，也有时候可以随机服务。总体来讲 根据业务需求和服务器当前状况来采用不同的降级策略。

**自动降级**：超时、失败次数、故障、限流

 （1）配置好超时时间(异步机制探测回复情况)；

 （2）不稳的的api调用次数达到一定数量进行降级(异步机制探测回复情况)；

 （3）调用的远程服务出现故障(dns、http服务错误状态码、网络故障、Rpc服务异常)，直接进行降级。

**人工降级**：秒杀、双十一大促降级非重要的服务。

## 限流

限流可以认为服务降级的一种，限流就是限制系统的输入和输出流量已达到保护系统的目的。一般来说系统的吞吐量(tps,qps)是可以被测算的，为了保证系统的稳定运行，一旦达到的需要限制的阈值，就需要限制流量并采取一些措施以完成限制流量的目的。比如：延迟处理，拒绝处理，或者部分拒绝处理等等。

限流常见的算法有四个：**固定窗口计数器**、**滑动窗口计数器**、**漏桶算法**、**令牌桶算法**

### 固定窗口计数器算法

使用固定窗口计数器实现限流的思路为，将某一个时间段当作固定窗口，在这个时间段每请求一次，计数器加1，当请求次数超过设定阈值时，则将接下来的请求直接拒绝，不在往下走。当时间段结束后，计数器会被初始化，从0开始计算。

举个例子，假如我们限制每分钟只能访问10次。在当前一分钟内，假设为当天的12点15分，每次请求之后计数器就会+1，当在12点15分到12点16分之间大于第10个请求过来时，就会被拒绝，告诉它请求被限制了，稍后再试。在12点16分0秒时，我们会重置计数器请求个数为0，在接下来的一分钟内前10个请求会允许通过。同样第11请求以及之后就会被拒绝。

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E5%9B%BA%E5%AE%9A%E7%AA%97%E5%8F%A3%E8%AE%A1%E6%95%B0%E5%99%A8.png)

**固定窗口计数器限流算法**无法保证限流速率，因而无法保证突然激增的流量。比如我们限制一个接口一分钟只能访问500次的话，在第15分钟前半分钟一个请求没有接收，后半分钟接收了500个请求，第16分钟前半分钟接受到500个请求。那么在15分钟后30秒和16分钟前30秒累计通过了1000个请求。有可能就会让服务器处理过载状态。

### 滑动窗口计数器算法

滑动窗口计数器算法算是固定窗口计数器算法升级版。滑动窗口计数器算法相比于固定窗口计数器算法的优化在于：它把时间以一定比例分片。例如我们的接口限流每分钟处理12个请求，我们把一分钟分为6个小窗口，每6个相邻的小窗口请求数量之和不能超过12，超过第12个的请求被直接拒绝。

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E6%BB%91%E5%8A%A8%E7%AA%97%E5%8F%A3%E8%AE%A1%E6%95%B0%E5%99%A8.png)

当滑动窗口划分的越多，限流处理越是平滑也越精确。但是当窗口设置的越多，占用内存也越大，也有可能会**引发内存溢出**。

### 漏桶算法

我们可以把接受请求的动作比作成注水到桶中，我们处理请求的过程可以比喻为漏桶漏水。我们往桶中以任意速率流入水，以一定速率流出水。当水超过桶流量则丢弃，因为桶容量是不变的，保证了整体的速率。

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E6%BC%8F%E6%A1%B6%E7%AE%97%E6%B3%95.png)



漏桶算法能强行限制数据的传输速率。假如业务要求能够限制数据的平均传输速率外，还要求允许某种程度的突发传输。这时候漏桶算法可能就不合适了，令牌桶算法更为适合。

### 令牌桶算法

令牌桶算法分为2个动作，动作1(固定速率往桶中存入令牌)、动作2(客户端如果想访问请求，先从桶中获取token)。能获取到token的则可以继续往下执行，没有获取的则被拒绝。令牌桶有一个初始值，桶内令牌初始数量。假如初始数量为500，每1分钟往桶放60个令牌。它可以瞬间处理500个请求，接下来每分钟处理60个请求，超过60的则被拒绝。

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E4%BB%A4%E7%89%8C%E6%A1%B6%E7%AE%97%E6%B3%95.png)

### 总结

四种算法对比

|   算法   |           确定参数           |                 空间复杂度                 | 时间复杂度 | 限流突发流量 | 平滑限流 | 分布式下实现难度 |
| :------: | :--------------------------: | :----------------------------------------: | :--------: | :----------: | :------: | :--------------: |
| 固定窗口 | 计数周期T、周期内最大访问数N | 低O(1)（记录周期内访问次数及周期开始时间） |   低O(1)   |      否      |    否    |        低        |
| 滑动窗口 | 计数周期T、周期内最大访问数N |    高O(N)（记录每个小周期中的访问数量）    |   中O(N)   |      是      | 相对实现 |        中        |
|   漏桶   |   漏桶流出速度r、漏桶容量N   |        低O(1)（记录当前漏桶中容量）        |   高O(N)   |      是      |    是    |        高        |
|  令牌桶  |  令牌产生速度r、令牌桶容量N  |      低O(1)（记录当前令牌桶中令牌数）      |   高O(N)   |      是      |    是    |        高        |

## 熔断

服务熔断是应对微服务雪崩效应的一种链路保护机制，类似股市、保险丝。当下游服务因访问压力过大而响应变慢或失败，上游服务为了保护系统整体的可用性，可以暂时切断对下游服务的调用，直接返回，快速释放资源。如果目标服务情况好转则恢复调用。

熔断器有三种状态：

**Close**:熔断器最初是处于Close状态,所有请求会正常通过和执行，当检测到错误到达一定阈值时，便转为**Open**状态

**Open**:所有请求都会被拒绝，当到Reset time时边转为**Half open**状态

**Half open**:尝试放行一部分请求到后端，一旦检测成功便回归到**Close**状态

![](https://raw.githubusercontent.com/DonetSchool/Core/main/documents/images/qos/%E7%86%94%E6%96%AD.png)

# Polly

## 简介

Polly is a .NET resilience and transient-fault-handling library that allows developers to express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner.

Polly 是一个 .NET 弹性和瞬态故障处理库，允许开发人员以 Fluent 和线程安全的方式来实现重试、断路、超时、隔离和回退策略。

Github地址：https://github.com/App-vNext/Polly

## Timeout

乐观超时功能

```c#
// Timeout and return to the caller after 30 seconds, if the executed delegate has not completed.  Optimistic timeout: Delegates should take and honour a CancellationToken.
Policy
  .Timeout(30)

// Configure timeout as timespan.
Policy
  .Timeout(TimeSpan.FromMilliseconds(2500))

// Configure variable timeout via a func provider.
Policy
  .Timeout(() => myTimeoutProvider)) // Func<TimeSpan> myTimeoutProvider

// Timeout, calling an action if the action times out
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        // Add extra logic to be invoked when a timeout occurs, such as logging 
    });

// Eg timeout, logging that the execution timed out:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        logger.Warn($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds.");
    });

// Eg timeout, capturing any exception from the timed-out task when it completes:
Policy
  .Timeout(30, onTimeout: (context, timespan, task) => 
    {
        task.ContinueWith(t => {
            if (t.IsFaulted) logger.Error($"{context.PolicyKey} at {context.OperationKey}: execution timed out after {timespan.TotalSeconds} seconds, with: {t.Exception}.");
        });
    });
```

## Advanced Circuit Breaker

高级断路器

```c#
// Break the circuit if, within any period of duration samplingDuration, 
// the proportion of actions resulting in a handled exception exceeds failureThreshold, 
// provided also that the number of actions through the circuit in the period
// is at least minimumThroughput.

Policy
    .Handle<SomeExceptionType>()
    .AdvancedCircuitBreaker(
        failureThreshold: 0.5, // Break on >=50% actions result in handled exceptions...
        samplingDuration: TimeSpan.FromSeconds(10), // ... over any 10 second period
        minimumThroughput: 8, // ... provided at least 8 actions in the 10 second period.
        durationOfBreak: TimeSpan.FromSeconds(30) // Break for 30 seconds.
                );

// Configuration overloads taking state-change delegates are
// available as described for CircuitBreaker above.

// Circuit state monitoring and manual controls are
// available as described for CircuitBreaker above.
```

## Fallback

降级处理

```c#
// Provide a substitute value, if an execution faults.
Policy<UserAvatar>
   .Handle<FooException>()
   .OrResult(null)
   .Fallback<UserAvatar>(UserAvatar.Blank)

// Specify a func to provide a substitute value, if execution faults.
Policy<UserAvatar>
   .Handle<FooException>()
   .OrResult(null)
   .Fallback<UserAvatar>(() => UserAvatar.GetRandomAvatar()) // where: public UserAvatar GetRandomAvatar() { ... }

// Specify a substitute value or func, calling an action (eg for logging) if the fallback is invoked.
Policy<UserAvatar>
   .Handle<FooException>()
   .Fallback<UserAvatar>(UserAvatar.Blank, onFallback: (exception, context) => 
    {
        // Add extra logic to be called when the fallback is invoked, such as logging
    });
```


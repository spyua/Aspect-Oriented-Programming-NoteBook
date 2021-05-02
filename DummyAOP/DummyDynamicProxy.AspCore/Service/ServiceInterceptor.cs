using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DummyDynamicProxy.AspCore.Service
{
    public class ServiceInterceptor : AbstractInterceptorAttribute
    {
        [FromServiceContext]
        public ILogger<ServiceInterceptor> Logger { get; set; }

        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            try
            {
                Logger.LogInformation("In Dynamic Proxy - Before executing '{0}'", context.ServiceMethod.Name); 
                await next(context);  // 進入 Service 前會於此處被攔截（如果符合被攔截的規則）...
                Logger.LogInformation("In Dynamic Proxy - After executing '{0}'", context.ServiceMethod.Name);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());  // 記錄例外錯誤...
                throw;
            }
        }
    }
}

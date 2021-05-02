using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;

namespace DummyAOP
{
    public class AuthenticationProxy<T> : RealProxy
    {
        private readonly T _decorated;
        public AuthenticationProxy(T decorated)
          : base(typeof(T))
        {
            _decorated = decorated;
        }
        private void Log(string msg, object arg = null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg, arg);
            Console.ResetColor();
        }
        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;

            try
            {
                Log("User authenticated - You can execute '{0}' ",methodCall.MethodName);
                var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                return new ReturnMessage(result, null, 0,
                  methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                Log(string.Format(
                  "User authenticated - Exception {0} executing '{1}'", e),methodCall.MethodName);
                return new ReturnMessage(e, methodCall);
            }

            Log("User not authenticated - You can't execute '{0}' ",methodCall.MethodName);
            return new ReturnMessage(null, null, 0, methodCall.LogicalCallContext, methodCall);

        }
    }
}

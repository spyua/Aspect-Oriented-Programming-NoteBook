using System;
using System.Reflection;
using System.Threading;

namespace DummyDynamicProxy.UseDispatchProxy
{
    public class AuthenticationProxy<T> : DispatchProxy where T : class
    {

        public T Target { get; private set; }

        public AuthenticationProxy() : base()
        {

        }

        public static T Decorate(T target = null)
        {
            var proxy = Create<T, AuthenticationProxy<T>>() as AuthenticationProxy<T>;

            proxy.Target = target ?? Activator.CreateInstance<T>();

            return proxy as T;
        }


        private void Log(string msg, object arg = null)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(msg, arg);
            Console.ResetColor();
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Log("User authenticated  - Before executing '{0}'", targetMethod.Name);
            var result = targetMethod.Invoke(Target, args);
            Log("User authenticated  - After executing '{0}' ", targetMethod.Name);
            return result;
        }
    }
}

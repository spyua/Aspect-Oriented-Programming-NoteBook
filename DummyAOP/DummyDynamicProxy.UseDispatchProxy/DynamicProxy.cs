using System;
using System.Reflection;

namespace DummyDynamicProxy.UseDispatchProxy
{
    public class DynamicProxy<T> : DispatchProxy where T : class
    {

        public T Target { get; private set; }
        
        public DynamicProxy() : base()
        {

        }

        public static T Decorate(T target = null)
        {
            var proxy = Create<T, DynamicProxy<T>>() as DynamicProxy<T>;

            proxy.Target = target ?? Activator.CreateInstance<T>();

            return proxy as T;
        }


        private void Log(string msg, object arg = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg, arg);
            Console.ResetColor();
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            Log("In Dynamic Proxy - Before executing '{0}'", targetMethod.Name);

            try
            {
                // 使用Class Method
                var result = targetMethod.Invoke(Target, args);
                Log("In Dynamic Proxy - After executing '{0}' ", targetMethod.Name);
                return result;

            }
            catch(Exception e)
            {
                Log(string.Format("In Dynamic Proxy- Exception {0} executing '{1}'", e), targetMethod.Name);
                return null;
            }
        }
    }
}

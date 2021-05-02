using DummyDynamicProxy.Infrastructure.Model;
using DummyDynamicProxy.Infrastructure.Repository;
using System;
using System.Reflection;
using System.Security.Principal;
using System.Threading;

namespace DummyDynamicProxy.UseDispatchProxy
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var messageDispatchProxy = RepositoryFactory.Create<Customer>();
            var customer = new Customer
            {
                Id = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };
            messageDispatchProxy.Add(customer);
            messageDispatchProxy.Update(customer);
            messageDispatchProxy.Delete(customer);
        }
    }
}

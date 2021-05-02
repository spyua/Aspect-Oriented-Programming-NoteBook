using AspectCore.DynamicProxy;
using DummyDynamicProxy.Infrastructure.Model;

namespace DummyDynamicProxy.AspCore.Service
{
    public interface ICustomerService
    {
        void AddCustmoer(Customer customer);
    }
}

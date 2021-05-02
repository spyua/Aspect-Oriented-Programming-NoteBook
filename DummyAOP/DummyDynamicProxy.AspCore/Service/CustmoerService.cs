using DummyDynamicProxy.Infrastructure.Model;
using DummyDynamicProxy.Infrastructure.Repository;

namespace DummyDynamicProxy.AspCore.Service
{
    public class CustmoerService : ICustomerService
    {

        private readonly IRepository<Customer> _repo;

        public CustmoerService(IRepository<Customer> repo)
        {
            _repo = repo;
        }

        public void AddCustmoer(Customer customer)
        {
            _repo.Add(customer);
        }

    }
}

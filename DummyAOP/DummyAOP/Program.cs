using DummyDynamicProxy.Infrastructure.Model;
using DummyDynamicProxy.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DummyAOP
{
    class Program
    {
        static void Main(string[] args)
        {
            //Simple Use - No Logger
            Console.WriteLine("***\r\n Begin program - no logging\r\n");
            IRepository<Customer> customerRepository =
              new Repository<Customer>();
            var customer = new Customer
            {
                Id = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };
            customerRepository.Add(customer);
            customerRepository.Update(customer);
            customerRepository.Delete(customer);
            Console.WriteLine("\r\nEnd program - no logging\r\n***");

            //Use Dynamic Proxy 
            Console.WriteLine("***\r\n Begin program - logging with dynamic proxy\r\n");
            IRepository<Customer> customerRepoProxy = RepositoryFactory.Create<Customer>();
            var newcustomer = new Customer
            {
                Id = 1,
                Name = "New Customer ",
                Address = "New Address"
            };
            customerRepoProxy.Add(newcustomer);
            customerRepoProxy.Update(newcustomer);
            customerRepoProxy.Delete(newcustomer);
            Console.WriteLine("\r\nEnd program - logging with dynamic proxy\r\n***");
            Console.ReadLine();


        }
    }
}

using DummyDynamicProxy.Infrastructure.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace DummyDynamicProxy.UseDispatchProxy
{
    public class RepositoryFactory
    {
        public static IRepository<T> Create<T>()
        {
            var repository = new Repository<T>();
            var proxyRepo = DynamicProxy<IRepository<T>>.Decorate(repository);
            proxyRepo = AuthenticationProxy<IRepository<T>>.Decorate(proxyRepo);
            return proxyRepo;
        }
    }
}

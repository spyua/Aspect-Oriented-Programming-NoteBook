

using DummyDynamicProxy.Infrastructure.Repository;

namespace DummyAOP
{
    public class RepositoryFactory
    {
        public static IRepository<T> Create<T>()
        {
            var repository = new Repository<T>();
            
            
            var decoratedRepository =(IRepository<T>)new DynamicProxy<IRepository<T>>(repository).GetTransparentProxy();

            // Create a dynamic proxy for the class already decorated
            decoratedRepository =(IRepository<T>)new AuthenticationProxy<IRepository<T>>(decoratedRepository).GetTransparentProxy();


            return decoratedRepository;
        }
    }
}

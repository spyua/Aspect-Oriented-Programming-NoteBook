# Aspect-Oriented-Programming-NoteBook

## 簡述

近期在跟高雄軟體King討論Log紀錄的機制方式，得知了AOP的機制，於是認真稍微研究了AOP的使用與實現方式。雖然King提供的AspCore真的快速實現了AOP，不過還是想花點時間去認真看若不使用Framework該如何實現這件事情~

AOP使用橫切入方法在原Class中插入新方法的且不會破壞原Class Method的一種技巧。

> `將橫切關注點（Cross-cutting concerns）與業務主體進行進一步分離，以提高程式碼的模組化程度！`

我們直接來帶例子來瞭解比較快~

如果今天你拿到的他人已寫好Service程式碼如下

```csharp=
public interface IXXXService{
    void QueryData()
}

public class XXXService:IXXXService{
    
    public IXXXService(){
    
    }
    
    public void QueryData(){
        ....
    }
}

```

突然發現它裡面的使用方法並沒有提供Log機制，此時你想加入Log你該怎麼作?

第一種方法也許你可以直接修改裡面的程式碼

```csharp=

public interface IXXXService{
    void QueryData()
}

public class XXXService:IXXXService{
    
    private readonly ILog _log;
    
    public IXXXService(ILog log){
        _log = log;
    }
    
    public void QueryData(){
        try{
           _log.I("QueryData");
           ....
        }
        catch(Exception e)
        {
          _log.E($"Imp QueryData " + {e.ToString()});
        }
        ....
    }
}

```

我們直接對Service作修改，注入你自己的Log Fun，並在方法內加入Try-Catch機制，但這種方法很明顯會直接異動到原程式碼。而且基本上Log跟原本的邏輯是沒有直接性的關係，所以寫在一起不是一個很好的方法。

那有何方法可以不用動到裡面的程式碼加入Log呢?使原本Service保有它就是具單純處裡商業邏輯的職責?我們可以使用簡單的靜態Proxy方法去作到這件事情。

```csharp=

// 原程式碼
public interface IXXXService{
    void QueryData()
}

public class XXXService:IXXXService{
    
    public IXXXService(){
    
    }
    
    public void QueryData(){
        ....
    }
}

//使用Proxy

public class ProxyXXX : IXXXService{

    private readonly IXXXService _xxxService;
    private readonly ILog _log;
    
    public ProxyXXX(IXXXService xxxService, ILog log){
        _xxxService = xxxService;
        _log = log;
    }
    
    public void QueryData(){
        try{
            _log.I("QueryData");
            _xxxService.QueryData();
            
        }catch(Exception e){
            _log.E($"Imp QueryData " + {e.ToString()});
        }
    }

}
```


直接宣告一個Proxy Class去實作IXXXService方法，因為是透過Proxy，所以有機會在做執行動作的前後做介入(Try-Catch , Log)。這樣可以再不改原程式碼下，直接對原Fun加料，

這種概念其實就是一種將橫切關注點（Ex:Log）與業務主體進行進一步分離。

簡單來說主要是有許多程式的需求其實跟程式的邏輯沒有直接關係，但是又需要在適時插入到邏輯中，最常見的就是在程式中插入紀錄log的功能。像這類的需求可以把它稱作Aspect或是cross-cutting concern，AOP(Aspect Oriented Programming)主要的目的，就是集中處理這一類的需求，讓這一類的需求與邏輯可以拆開，但是又可以在適當的時機介入到程式邏輯中。

如下圖示意，一般我們在寫程式時，很常需要處理譬如錯誤紀錄、權限驗證，乃至於額外可能增加使用者查詢歷程等等功能，我們若使用AOP概念，就不對每個方法去實做這些事情，而是猶如灰黃紅的箭頭指向，所有方法要執行時就一定得經過權限、資料與錯誤的處裡。

![](https://i.imgur.com/8BtycBo.png)

但如果接手的程式當中，Service有上百支方法，我們不可能逐步透過靜態Proxy去實作對每個方法介入我們要增加的功能，此時我們就會使用動態Proxy來達成這個目標。

## 動態Proxy

在C#實作動態代理可以透過RealProxy與DispatchProxy兩個類別實現。前者可在一般的 .Net Framework上使用，而 .Net Core則需使用後者。 

### 實作情境
根據[MSDN](https://reurl.cc/9Z8AmV)的Dynamic Proxy教學說明情境，我們假設情境上有個Customer Model，我們要透過Repository去操作資料。

因此我們先針對Context設計基礎建設，有Customer Model與Repository相關實作，Repository就作一般的CRUD操作。

![](https://i.imgur.com/rMm9vtf.png)

#### Customer Model

```csharp=
 public class Customer
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Address { get; set; }
}
```
#### IRepository
```csharp=
 public interface IRepository<T>
{
    void Add(T entity);
    void Delete(T entity);
    void Update(T entity);
    IEnumerable<T> GetAll();
    T GetById(int id);
}
```
#### Repository
```csharp=
public class Repository<T> : IRepository<T>
{
        public void Add(T entity)
        {
            Console.WriteLine("Adding {0}", entity);
        }
        public void Delete(T entity)
        {
            Console.WriteLine("Deleting {0}", entity);
        }
        public void Update(T entity)
        {
            Console.WriteLine("Updating {0}", entity);
        }
        public IEnumerable<T> GetAll()
        {
            Console.WriteLine("Getting entities");
            return null;
        }
        public T GetById(int id)
        {
            Console.WriteLine("Getting entity {0}", id);
            return default(T);
        }
}
```

在基礎建設實作完後，在一般沒使用Proxy的情境下，可直接Create Repository Instance使用CRUD。

#### Main

```csharp=
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

        }
    }

```

輸出結果

![](https://i.imgur.com/zoVqnKj.png)

### 用RealProxy實作Dynamic Proxy
![](https://i.imgur.com/bmdzt82.png)

#### Repositroy加入Log

使用Repository是沒什麼問題，接著我們要透過RealProxy實作在每個CRUD操作介入插入Log以及Try-Catch。

RealProxy基本上實作Dynamic Proxy非常簡單，只需實作Invoke方法。底層原理大致就是用C#反射去使用被代理者的方法。有興趣看透透可以去翻Wiki-Aspect-oriented software development那篇，有提到AOP的原理大致是根據Reflection, Metaobject Protocols, Composition Filters演變過來的。

```csharp=
public class DynamicProxy<T> : RealProxy
    {
        private readonly T _decorated;
        public DynamicProxy(T decorated)
          : base(typeof(T))
        {
            _decorated = decorated;
        }
        // Log Fun
        private void Log(string msg, object arg = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg, arg);
            Console.ResetColor();
        }
        // Impleation Invoke
        public override IMessage Invoke(IMessage msg)
        {
            var methodCall = msg as IMethodCallMessage;
            var methodInfo = methodCall.MethodBase as MethodInfo;

            Log("In Dynamic Proxy - Before executing '{0}'", methodCall.MethodName);

            try
            {
                var result = methodInfo.Invoke(_decorated, methodCall.InArgs);
                Log("In Dynamic Proxy - After executing '{0}' ", methodCall.MethodName);
                return new ReturnMessage(result, null, 0, methodCall.LogicalCallContext, methodCall);
            }
            catch (Exception e)
            {
                Log(string.Format("In Dynamic Proxy- Exception {0} executing '{1}'", e),methodCall.MethodName);
                return new ReturnMessage(e, methodCall);
            }
        }
    }
```

在實作完動態代理完後，在Client端使用如下

```csharp=
var repository = new Repository<Customer>();                      
var customerRepoProxy =(IRepository<Customer>)new DynamicProxy<IRepository<Customer>>(repository);

var newcustomer = new Customer
 {
    Id = 1,
    Name = "New Customer ",
    Address = "New Address"
 };
customerRepoProxy.Add(newcustomer);
customerRepoProxy.Update(newcustomer);
customerRepoProxy.Delete(newcustomer);

```

又或是我們可以在建一個Repository工廠，能彈性產生或組裝不同代理者

```csharp=
public class RepositoryFactory
{
        public static IRepository<T> Create<T>()
        {
            var repository = new Repository<T>(); 
            var decoratedRepository =(IRepository<T>)new DynamicProxy<IRepository<T>>(repository).GetTransparentProxy();
            return decoratedRepository;
        }
}
```
![](https://i.imgur.com/l9VSl4m.png)

#### Repository加入Authentication

透過Dynamic Proxy的使用，在原本的Repository CRUD上加入Log與Try-Catch後，接著我們在嘗試建置領一個Dynamic Proxy~模擬方法作權限驗證。

實作AuthenticationProxy

```chsarp=
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
```
實作完AuthenticationProxy後，我們修改一下原先的Repository Factory

```csharp=
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
```

在實作後，在Client端使用如下

```csharp=
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
```

![](https://i.imgur.com/lXF3cxL.png)

### 使用DispatchProxy實作Dynamic Proxy
![](https://i.imgur.com/QMXWrQZ.png)

#### Repositroy加入Log

根據上述範例，我們使用DispatchProxy再實作一次。DispatchProxy操作起來差不多，只是除了要實作Invoke外，對於Create Class Instance那段我們也需要額外實作(Decorate)。

```csharp=
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
```

實作完後，Client端使用方法如下

```csharp=
 var repository = new Repository<Customer>();
 var messageDispatchProxy = DynamicProxy<IRepository<Customer>>.Decorate(repository); 
```

這邊一樣實作RepositoryFactory彈性產生或組裝不同代理者。

```csharp=
public class RepositoryFactory
{
        public static IRepository<T> Create<T>()
        {
            var repository = new Repository<T>();
            var proxyRepo = DynamicProxy<IRepository<T>>.Decorate(repository);
            return proxyRepo;
        }
}
```
Client端使用如下

```csharp=
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
```

![](https://i.imgur.com/W2FMfng.png)


#### Repositroy加入Authentication

一樣實作模擬驗證

```csharp=
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
```

修改Repository

```csharp=
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
```
Client使用不變

```csharp=
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
```
![](https://i.imgur.com/SMy6i6M.png)



## AspectCore使用

目前對於AOP， .Net Core已經有現成的Solution Framework可以使用。叫[AspectCore](https://github.com/dotnetcore/AspectCore-Framework)。針對AspectCore，個人覺得使用上Neil Tsai的[AspectCore｜.Net Core 輕量AOP實現](https://reurl.cc/l066D9)算解說的蠻清楚的。不過我再根據上述微軟MSDN教學描述的Context作延續使用。

上述我們提到在使用動態Proxy使用Repository撈取Custmoer資料。接下來我們在Web上使用AspectCore去達到這件事情。在Web架構上我們以常見的集中式架構去作設計，撰寫Customer Service取使用Repository。並使用AspCore，在Controller呼叫Service時，增加Log顯示。

### Step1:新增Web MVC專案
我們使用Visual Studio新增一MVC專案，過程沒什麼其他特別設定，這邊起始新增專案就不多加描述。

### Step2:安裝AspCore
直接使用Cli安裝AspCore，或使用NutGet套件管理員安裝AspectCore.Extensions.DependencyInjection

```
dotnet add package AspectCore.Extensions.DependencyInjection
```
### Step3:實作CustmoerService
接著我們開始實作Service，首先先新增Customer Service Interface，只實作AddCustmoer。

```csharp=
public interface ICustomerService
{
  void AddCustmoer(Customer customer);
}
```
接著撰寫實作

```csharp=
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
```

### Step4:使用AbstractInterceptorAttribute設計Service攔截器
Service撰寫完後，開始用AspCore撰寫攔截器(代理使用Service)，攔截Service呼叫並在前後介入Log顯示。

```csharp=
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
```

### Step5:設定Starup的DI及AspCore代理

#### A. 在Configure設置Services與Repository
在Starup設置Service與Repository DI設置
```csharp=
services.AddTransient<IRepository<Customer>, Repository<Customer>>();
services.AddTransient<ICustomerService, CustmoerService>();
```
### B.設置動態代理
在Starup設置DynamicProxy DI設置
```csharp=
services.ConfigureDynamicProxy(config => { config.Interceptors.AddTyped<ServiceInterceptor>(Predicates.ForService("*Service")); });
```
---
#### 常用代理規則設定：
 - 全域會被代理
 ```csharp=
   config.Interceptors.AddTyped<ServiceInterceptor>();
 ```
 - 後綴為 Service 會被代理：
 ```csharp=
   config.Interceptors.AddTyped<ServiceInterceptor>(Predicates.ForService("*Service"));
 ```
  - 前綴為 Execute 的方法會被代理：
 ```csharp=
   config.Interceptors.AddTyped<ServiceInterceptor>(Predicates.ForMethod("Execute*"));
 ```
  - App1 命名空間下的 Service 不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddNamespace("App1");
 ```
  - 最後一層為 App1 的命名空間下的 Service 不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddNamespace(".App1");
 ```
  - ICustomService 不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddService("ICustomService");
 ```
  - 後綴為 Service 不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddService("Service");
 ```
  - 命名為 Query 的方法不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddMethod("Query");
 ```
  - 後綴為 Query 的方法不會被代理：
 ```csharp=
   config.NonAspectPredicates.AddMethod("*Query");
 ```
 AspectCore 也提供 NonAspectAttribute 來使得 Service 或 Method 不會被代理。只要在Interface方法上加上[NonAspect]，Service的此方法就會被忽略不被代理
 
 ```csharp=
public interface IXXXService
{
   [NonAspect]
   void XXXMethod;
}
 ```
---

### C.在program.cs於 CreateHostBuilder 處加上 UseServiceProviderFactory

在Program.cs 的 CreateHostBuilder 處加上 UseServiceProviderFactory(new DynamicProxyServiceProviderFactory())，將預設 DI 交由 AspectCore 處理

```csharp=
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        })
        .UseServiceProviderFactory(newDynamicProxyServiceProviderFactory());
```
### Step6:在HomeController注入ICustomerService，並在Privacy API加入Service動作

為了方便Demo看到AspCore攔截Service功能，我們在 .Net Core Web初始專案的HomeController Privacy API加入AddCustmoer功能服務，當User點擊Privacy分頁，就會呼叫CustomerService功能，攔截器會攔截此呼叫，並先印出Log後，在執行Service方法~

```csharp=
private readonly ILogger<HomeController> _logger;
private readonly ICustomerService _repoService;

public HomeController(ILogger<HomeController> logger, ICustomerService repoService)
{
   _logger = logger;
   _repoService = repoService;
}

public IActionResult Privacy()
{
            var customer = new Customer
            {
                Id = 1,
                Name = "Customer 1",
                Address = "Address 1"
            };

            _repoService.AddCustmoer(customer);

            return View();
}
```

### Step7執行測試

![](https://i.imgur.com/8P1GeqG.png)


![](https://i.imgur.com/HBNR8Ie.png)


## Summary

此篇大致整理AOP的使用情境與方法，也對於AspCore使用方式簡單實作一個Demo。希望對於未聽過與使用過的人可以快速對於AOP概念有所了解。

## 參考

[Aspect-oriented software development](https://en.wikipedia.org/wiki/Aspect-oriented_software_development)

[Aspect-Oriented Programming : Aspect-Oriented Programming with the RealProxy Class](https://docs.microsoft.com/en-us/archive/msdn-magazine/2014/february/aspect-oriented-programming-aspect-oriented-programming-with-the-realproxy-class)

[Javascript面面觀：核心篇《模式-Reflection, Proxy and AOP》](https://ithelp.ithome.com.tw/articles/10031358)

[AspectCore｜.Net Core 輕量 AOP 實現](https://www.thinkinmd.com/post/2020/03/20/use-aspectcore-to-implement-aop-mechanism/)``

using OpenRace.Data.GSL.Abstractions;

namespace OpenRace.Data.GSL.Testing;

public class GenericServiceProviderFake<TService1, TService2, TService3> 
    : IGenericServiceProvider<TService1, TService2, TService3> 
    where TService1 : notnull 
    where TService2 : notnull 
    where TService3 : notnull
{
    private readonly IGenericServiceScope<TService1, TService2, TService3> _serviceScope;

    public GenericServiceProviderFake(TService1 service1, TService2 service2, TService3 service3)
    {
        _serviceScope = new GenericServiceScopeFake<TService1, TService2, TService3>(
            service1, service2, service3);
    }
    
    public GenericServiceProviderFake(IGenericServiceScope<TService1, TService2, TService3> serviceScope)
    {
        _serviceScope = serviceScope;
    }

    public IGenericServiceScope<TService1, TService2, TService3> CreateScope() => _serviceScope;
}
using System;
using OpenRace.Data.GSL.Abstractions;

namespace OpenRace.Data.GSL;

public class GenericServiceProvider<TService1, TService2, TService3> 
    : IGenericServiceProvider<TService1, TService2, TService3> 
    where TService1 : notnull 
    where TService2 : notnull 
    where TService3 : notnull
{
    private readonly IServiceProvider _serviceProvider;

    public GenericServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IGenericServiceScope<TService1, TService2, TService3> CreateScope()
    {
        return new GenericServiceScope<TService1, TService2, TService3>(_serviceProvider);
    }
}
using System;
using Microsoft.Extensions.DependencyInjection;
using OpenRace.Data.GSL.Abstractions;

namespace OpenRace.Data.GSL;

public class GenericServiceProvider<TService1, TService2, TService3> 
    : IGenericServiceProvider<TService1, TService2, TService3> 
    where TService1 : notnull
    where TService2 : notnull
    where TService3 : notnull
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public GenericServiceProvider(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory 
                               ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
    }

    public IGenericServiceScope<TService1, TService2, TService3> CreateScope()
    {
        return new GenericServiceScope<TService1, TService2, TService3>(_serviceScopeFactory);
    }
}
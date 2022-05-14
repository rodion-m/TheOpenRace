using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OpenRace.Data.GSL.Abstractions;

namespace OpenRace.Data.GSL;

internal abstract class GenericServiceScopeBase : IDisposable, IAsyncDisposable
{
    protected readonly AsyncServiceScope Scope;

    protected GenericServiceScopeBase(IServiceProvider serviceProvider)
    {
        Scope = serviceProvider.CreateAsyncScope();
    }

    public void Dispose() => Scope.Dispose();
    public ValueTask DisposeAsync() => Scope.DisposeAsync();
}

internal class GenericServiceScope<TService1, TService2, TService3>
    : GenericServiceScopeBase, 
        IGenericServiceScope<TService1, TService2, TService3>
    where TService1 : notnull
    where TService2 : notnull
    where TService3 : notnull
{
    public TService1 Service1 => Scope.ServiceProvider.GetRequiredService<TService1>();
    public TService2 Service2 => Scope.ServiceProvider.GetRequiredService<TService2>();
    public TService3 Service3 => Scope.ServiceProvider.GetRequiredService<TService3>();

    public GenericServiceScope(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void Deconstruct(
        out TService1 service1,
        out TService2 service2,
        out TService3 service3
    )
    {
        service1 = Service1;
        service2 = Service2;
        service3 = Service3;
    }
}
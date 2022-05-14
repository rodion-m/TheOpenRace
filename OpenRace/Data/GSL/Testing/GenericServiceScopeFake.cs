using System;
using System.Threading.Tasks;
using OpenRace.Data.GSL.Abstractions;

namespace OpenRace.Data.GSL.Testing;

internal class GenericServiceScopeFake<TService1, TService2, TService3>
    : IGenericServiceScope<TService1, TService2, TService3>
    where TService1 : notnull
    where TService2 : notnull
    where TService3 : notnull
{
    public GenericServiceScopeFake(TService1 service1, TService2 service2, TService3 service3)
    {
        Service1 = service1 ?? throw new ArgumentNullException(nameof(service1));
        Service2 = service2 ?? throw new ArgumentNullException(nameof(service2));
        Service3 = service3 ?? throw new ArgumentNullException(nameof(service3));
    }

    public TService1 Service1 { get; }
    public TService2 Service2 { get; }
    public TService3 Service3 { get; }
    
    public void Deconstruct(out TService1 service1, out TService2 service2, out TService3 service3)
    {
        service1 = Service1;
        service2 = Service2;
        service3 = Service3;
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
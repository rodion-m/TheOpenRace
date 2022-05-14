using System;

namespace OpenRace.Data.GSL.Abstractions;

public interface IGenericServiceScope<TService1, TService2, TService3> : IDisposable, IAsyncDisposable
{
    TService1 Service1 { get; }
    TService2 Service2 { get; }
    TService3 Service3 { get; }

    void Deconstruct(
        out TService1 service1,
        out TService2 service2,
        out TService3 service3
    );
}
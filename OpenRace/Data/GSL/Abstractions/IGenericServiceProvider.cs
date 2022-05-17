namespace OpenRace.Data.GSL.Abstractions;

public interface IGenericServiceProvider<TService1, TService2, TService3> 
    where TService1 : notnull 
    where TService2 : notnull 
    where TService3 : notnull
{
    IGenericServiceScope<TService1, TService2, TService3> CreateScope();
}
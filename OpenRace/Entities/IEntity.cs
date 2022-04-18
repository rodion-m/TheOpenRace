using System;

namespace OpenRace.Entities
{
    public interface IEntity
    {
        Guid Id { get; init; }
    }
}
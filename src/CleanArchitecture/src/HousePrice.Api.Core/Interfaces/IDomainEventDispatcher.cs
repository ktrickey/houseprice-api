using HousePrice.Api.Core.SharedKernel;

namespace HousePrice.Api.Core.Interfaces
{
    public interface IDomainEventDispatcher
    {
        void Dispatch(BaseDomainEvent domainEvent);
    }
}
using HousePrice.Api.Core.Interfaces;
using HousePrice.Api.Core.SharedKernel;

namespace CleanArchitecture.Tests
{
    public class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public void Dispatch(BaseDomainEvent domainEvent) { }
    }
}

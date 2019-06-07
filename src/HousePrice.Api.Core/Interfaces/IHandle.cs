using HousePrice.Api.Core.SharedKernel;

namespace HousePrice.Api.Core.Interfaces
{
    public interface IHandle<T> where T : BaseDomainEvent
    {
        void Handle(T domainEvent);
    }
}
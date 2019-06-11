using System;

namespace HousePrice.Api.Core.SharedKernel
{
    public interface IMongoEntity
    {
        String Id { get; set; }
    }

    public class MongoEntity : BaseEntity<string>, IMongoEntity
    {
        
    }
}
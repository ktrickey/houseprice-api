using System;
using HousePrice.Api.Core.Interfaces;
using HousePrice.Api.Core.SharedKernel;
using MongoDB.Bson.Serialization.Attributes;

namespace HousePrice.Api.Core.Entities
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class HousePriceTransaction: MongoEntity, IGeoEntity
    {
        [BsonId]
        public string TransactionId
        {
            get { return base.Id;}
            set { base.Id = value; }
        }
        public double Price { get; set; }
        public DateTime TransferDate { get; set; }
        public string Postcode { get; set; }
        public string PropertyType { get; set; }
        public string IsNew { get; set; }
        public string Duration { get; set; }
        public string PAON { get; set; }
        public string SAON { get; set; }
        public string Street { get; set; }
        public string Locality { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string County { get; set; }
        public string CategoryType { get; set; }
        public string Status { get; set; }
        public Location Location { get; set; }
        public ILocation GeoLocation => Location;
    }
}
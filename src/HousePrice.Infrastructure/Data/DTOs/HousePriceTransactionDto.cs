using System;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;
using MongoDB.Bson.Serialization.Attributes;

namespace HousePrice.Infrastructure.Data.DTOs
{
    public class HousePriceTransactionDto
    {
        public HousePriceTransactionDto(HousePriceTransaction transaction)
        {
            TransactionId = transaction.Id;
            Price = transaction.Price;
            TransferDate = transaction.TransferDate;
            Postcode = transaction.Postcode;
            PropertyType = transaction.PropertyType;
            IsNew = transaction.IsNew;
            Duration = transaction.Duration;
            PAON = transaction.PAON;
            SAON = transaction.SAON;
            Street = transaction.Street;
            Locality = transaction.Locality;
            City = transaction.City;
            District = transaction.District;
            County = transaction.County;
            CategoryType = transaction.CategoryType;
            Status = transaction.Status;
            Location = new Location(transaction.Location.Latitude, transaction.Location.Longitude);
        }
        
        [BsonId]
        public string TransactionId { get; set; }
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
        public ILocation Location { get; set; }

        public async Task<HousePriceTransaction> ToHousePriceTransaction()
        {
            return new HousePriceTransaction()
            {
                Id = TransactionId,
                CategoryType = CategoryType,
                City = City,
                County = County,
                District = District,
                Duration = Duration,
                Locality = Locality,
                Location = new HousePrice.Api.Core.Entities.Location(Location.Latitude, Location.Longitude),
                Postcode = Postcode,
                Price = Price,
                Status = Status,
                Street = Street,
                IsNew = IsNew,
                PropertyType = PropertyType,
                TransferDate = TransferDate,
                PAON = PAON,
                SAON = SAON
            };
        }
    }
    
}
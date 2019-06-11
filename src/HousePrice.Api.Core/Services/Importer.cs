using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;
using MongoDB.Bson;

namespace HousePrice.Api.Core.Services
{
    public interface IImporter
    {
        Task Import(string name, Stream csvStream);
        Task Import(IEnumerable<HousePriceTransaction> priceRecord);
    }

    internal class Importer : IImporter
    {
        private readonly IRepository _mongoRepository;
        private readonly IPostcodeLookup _postcodeLookup;

        public Importer(IRepository mongoRepository, IPostcodeLookup postcodeLookup)
        {
            _mongoRepository = mongoRepository;
            _postcodeLookup = postcodeLookup;
        }

        public async Task Import(IEnumerable<HousePriceTransaction> records)
        {
               var recordList = records.ToList();

                var uniquePostcodes = recordList.Select(r => r.Postcode.Replace(" ", string.Empty)).Distinct().ToArray();

                var locations = new Dictionary<string, IPostcodeData>(uniquePostcodes.Length);

                foreach (var postcode in uniquePostcodes)
                {
                    var postcodeInfo = await _postcodeLookup.GetByPostcode(postcode);
                    locations.Add(postcodeInfo.Postcode, postcodeInfo);
                        
                }

                foreach (var record in recordList)
                {
                    var locationData =  locations[record.Postcode];
                    record.Location = new Location(locationData.Location.Latitude, locationData.Location.Longitude);
                }

                var adds = recordList.Where(r => r.Status == "A");
                var deletes = recordList.Where(r => r.Status == "D");

                try
                {
                    await _mongoRepository.InsertMany(adds);
                    await _mongoRepository.DeleteMany(deletes);



                }
                catch (Exception ex)
                {
                 //   Log.Warning(ex.Message);
                    throw;
                }
          
        }

        public async Task Import(string name, Stream csvStream)
        {

                using (var streamReader = new StreamReader(csvStream))
                {
                    using (var csvReader = new CsvHelper.CsvReader(streamReader))
                    {
                        csvReader.Configuration.HasHeaderRecord = false;
                        csvReader.Configuration.RegisterClassMap<HousePriceMap>();
                        var batch = new List<HousePriceTransaction>();

                        while (csvReader.Read())
                        {
                            try
                            {
                                var record = csvReader.GetRecord<HousePriceTransaction>();
                                record.Postcode = record.Postcode.Replace(" ", String.Empty);
                            }
                            catch (Exception e)
                            {
                                //Log.Error(e.Message);
                                throw;
                            }
                        }

                        if (batch.Count % 1000 == 0)
                        {
                            Console.WriteLine($"{name}: {batch.Count}");
                        }

                        await _mongoRepository.InsertMany(batch);
                    }
                }

        
        }

      
    }

    // ReSharper disable once ClassNeverInstantiated.Global
}
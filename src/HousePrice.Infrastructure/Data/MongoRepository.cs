using System;
using HousePrice.Api.Core.Interfaces;
using HousePrice.Api.Core.SharedKernel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Services;
using Microsoft.CodeAnalysis;
using MongoDB.Driver;

namespace HousePrice.Infrastructure.Data
{
    public class MongoRepository : IRepository
    {
        private readonly IMongoContext _dbContext;

        public MongoRepository(IMongoContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> GetById<T>(string id) where T : MongoEntity
        {
            return await  _dbContext.ExecuteAsync<T, T>(typeof(T).Name,
                async (activeCollection) =>
                {
                    return await activeCollection.Find(_ => _.Id == id).SingleAsync();
                    
                });
        }

 
        public async Task<T> Add<T>(T entity) where T : MongoEntity
        {
            await _dbContext.ExecuteActionAsync<T>(typeof(T).Name, async (activeCollection) =>
                {
                    await activeCollection.InsertOneAsync(entity);
                });

            return entity;
        }

        public async Task Delete<T>(T entity) where T : MongoEntity
        {
            await _dbContext.ExecuteActionAsync<T>(typeof(T).Name, async (activeCollection) =>
            {
                await activeCollection.DeleteOneAsync(x=>x.Id == entity.Id);
            });
        }

        public async Task DeleteMany<T>(IEnumerable<T> entities) where T : MongoEntity
        {
            await _dbContext.ExecuteActionAsync<T>(typeof(T).Name, async (activeCollection) =>
            {
                await activeCollection.DeleteManyAsync(Builders<T>.Filter.In(x=>x.Id, entities.Select(x=>x.Id)));
            });
        }

        public async Task InsertMany<T>(IEnumerable<T> entities) where T : MongoEntity
        {
            await _dbContext.ExecuteActionAsync<T>(typeof(T).Name, async (activeCollection) =>
            {
                await activeCollection.InsertManyAsync(entities);
            });
        }

        public async Task Update<T>(T entity) where T : MongoEntity
        {
            await _dbContext.ExecuteActionAsync<T>(typeof(T).Name, async (activeCollection) =>
            {
                await activeCollection.ReplaceOneAsync(z=>z.Id == entity.Id, entity);
            });
        }

        public async Task<PagedResult<T>> FindWithinArea<T>(LocationFilter filter, Expression<Func<T, object>> field, int skip) where T:IGeoEntity
        {

            return await _dbContext.ExecuteAsync<T, PagedResult<T>>(typeof(T).Name, async (activeCollection) =>
            {
                var filterDef = new FilterDefinitionBuilder<T>().GeoWithinCenterSphere(
                    tag => tag.GeoLocation,
                    filter.Location.Longitude.Value,
                    filter.Location.Latitude.Value,
                    (filter.Radius / 1000) / 6371);
                var set = await activeCollection.FindAsync<T>(filterDef);

                // hard code the sort for now...
                
                var sort = new SortDefinitionBuilder<T>().Descending(field);

                var query = activeCollection.Find(filterDef);

                var prices = await query.Sort(sort).Skip(skip).Limit(51).ToListAsync();
                return new PagedResult<T>(prices.Count==51, prices.Skip(0).Take(50).ToArray() );


                
            });
        }
    }
}

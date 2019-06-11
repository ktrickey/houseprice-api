using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HousePrice.Api.Core.Services;
using HousePrice.Api.Core.SharedKernel;

namespace HousePrice.Api.Core.Interfaces
{
    public interface IRepository
    {
        Task<T> GetById<T>(string id) where T : IMongoEntity;

        Task<T> Add<T>(T entity) where T : IMongoEntity;
        Task Update<T>(T entity) where T : IMongoEntity;
        Task Delete<T>(T entity) where T : IMongoEntity;
        Task DeleteMany<T>(IEnumerable<T> entities) where T : IMongoEntity;
        Task InsertMany<T>(IEnumerable<T> entities) where T : IMongoEntity;

        Task<PagedResult<T>> FindWithinArea<T>(LocationFilter filter, Expression<Func<T, object>> field, int skip)
            where T : IGeoEntity;

    }
}

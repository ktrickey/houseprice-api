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
        Task<T> GetById<T>(string id) where T : MongoEntity;

        Task<T> Add<T>(T entity) where T : MongoEntity;
        Task Update<T>(T entity) where T : MongoEntity;
        Task Delete<T>(T entity) where T : MongoEntity;
        Task DeleteMany<T>(IEnumerable<T> entities) where T : MongoEntity;
        Task InsertMany<T>(IEnumerable<T> entities) where T : MongoEntity;

        Task<PagedResult<T>> FindWithinArea<T>(LocationFilter filter, Expression<Func<T, object>> field, int skip)
            where T : IGeoEntity;

    }
}

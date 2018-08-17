using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace HousePrice.Api.Services
{
    public class MongoRunner
    {
	    private IMongoCollection<T> GetCollection<T>()
	    {
		    var client = new MongoClient("mongodb://localhost:32768");
		    var database = client.GetDatabase("HousePrice");
		    //     database.DropCollection("Transactions");
		    return database.GetCollection<T>("Transactions");
	    }

	    public TO RunInMongo<TO, T>(Func<IMongoCollection<T>, TO> action)
	    {

		    return action(GetCollection<T>());
	    }
	    public void RunInMongo<T>(Action<IMongoCollection<T>> action)
	    {
		    action(GetCollection<T>());
	    }
	    public async Task RunInMongoAsync<T>(Func<IMongoCollection<T>, Task> action)
	    {
		    await action(GetCollection<T>());
	    }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.Services;
using MongoDB.Driver;

namespace HousePrice.Api.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Populating lookup");
            PostcodeLookup.GetByPostcode("");//force population of lookup
           // var importer = new HousePrice.Api.Services.Importer();
           // var tasks = new List<Task>();
           // for (int year = 1995; year <= 2018; year++)
           // {
           //     Console.WriteLine($"Importing {year}");
           //     var filename = $@"C:\Dev\HousePrice\data\{year}.csv";
           //     tasks.Add(Task.Run(()=>importer.Import(new FileStream(filename,
           //         FileMode.Open, FileAccess.Read))));
           // }

           //Task.WaitAll(tasks.ToArray());
          

			
			var searcher = new Importer();
	        //Console.WriteLine("Adding index");
	        //await searcher.AddIndex();
	        var items = searcher.Find("CB233NY", 3000);


        }
    }
}
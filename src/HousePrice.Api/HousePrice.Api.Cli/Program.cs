using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HousePrice.Api.Services;

namespace HousePrice.Api.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Populating lookup");
            var isReady = PostcodeLookup.IsReady;
            // var importer = new HousePrice.Api.Services.Importer();
//            var tasks = new List<Task>();
//            for (int year = 1995; year <= 2018; year++)
//            {
//                Console.WriteLine($"Importing {year}");
//                var filename = $@"C:\Dev\HousePrice\data\{year}.csv";
//                tasks.Add(Task.Run(()=>importer.Import(new FileStream(filename,
//                    FileMode.Open, FileAccess.Read))));
//            }
//
//           Task.WaitAll(tasks.ToArray());
//           Console.WriteLine("Adding index");
//           await importer.AddIndex();

            //await importer.AddPostcodeIndex();
            //await importer.AddTransferDateIndex();

            Console.ReadLine();
            var stuff = await new Lookup().GetLookups("ZZ999ZZ", 1000);
        }
    }
}
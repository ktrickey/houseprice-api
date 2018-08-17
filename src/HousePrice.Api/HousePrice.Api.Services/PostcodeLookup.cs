﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HousePrice.Api.Services
{
	public static class PostcodeLookup
	{
		[Serializable]
		public class PostcodeData
		{
			private string _postcode;

			public long Id { get; set; }

			public string Postcode
			{
				get => _postcode;
				set => _postcode = value.Replace(" ", string.Empty);
			}

			public double? Latitude { get; set; }
			public double? Longitude { get; set; }
		}

		private static Dictionary<string, PostcodeData> postcodeLookup;

		public static bool IsReady => PopulateTask.IsCompleted;
		private static readonly Task PopulateTask;

		static PostcodeLookup()
		{
			PopulateTask = new Task(() =>
			{
				using (var streamReader =
					new StreamReader(new FileStream(@"c:\mongo\ukpostcodes.csv", FileMode.Open, FileAccess.Read)))
				{
					using (var csvReader = new CsvHelper.CsvReader(streamReader))
					{
						csvReader.Configuration.BufferSize = 131072;
						var recs = csvReader.GetRecords<PostcodeData>();
						var timer = Stopwatch.StartNew();
						postcodeLookup = recs.ToDictionary(p => p.Postcode);
						timer.Stop();
						Console.WriteLine(timer.ElapsedMilliseconds);

					}
				}
			});
			PopulateTask.Start();
		}

		public static PostcodeData GetByPostcode(string postcode)
		{
			PopulateTask.Wait();

			var found = postcodeLookup.TryGetValue(postcode, out PostcodeData postcodeData);
			return found ? postcodeData:null;
		}
	}
}
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HousePrice.Api.Services
{
	public static class PostcodeLookup
	{
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
		static PostcodeLookup()
		{
			using (var streamReader = new StreamReader(new FileStream(@"c:\mongo\ukpostcodes.csv", FileMode.Open, FileAccess.Read)))
			{
				using (var csvReader = new CsvHelper.CsvReader(streamReader))
				{
					csvReader.Configuration.BufferSize = 65536;
					var recs = csvReader.GetRecords<PostcodeData>();
					postcodeLookup = recs.ToDictionary(p => p.Postcode);
  
				}
			}
		}

		public static PostcodeData GetByPostcode(string postcode)
		{
			var found = postcodeLookup.TryGetValue(postcode, out PostcodeData postcodeData);
			return found? postcodeData:null;
		}
	}
}
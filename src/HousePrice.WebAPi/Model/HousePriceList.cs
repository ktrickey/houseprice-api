using System.Collections.Generic;
using System.Linq;

namespace HousePrice.WebAPi.Model
{
    public class YearRange
    {
        public int First { get; set; }
        public int Last { get; set; }
    }
    public class HousePriceList
    {
        private readonly PagedResult<HousePrice> _results;

        public HousePriceList( PagedResult<HousePrice> results)
        {
            _results = results;
            YearRange = new YearRange(){First = 1995,Last = 2019};
        }
        public YearRange YearRange { get;  }
        public bool MoreAvailable => _results.MoreAvailable;
        public IEnumerable<HousePrice> Results => _results.Results;

    }
}
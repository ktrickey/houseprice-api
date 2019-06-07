using System.Collections.Generic;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.SharedKernel;

namespace HousePrice.Api.ApiModels
{
    public class YearRange
    {
        public int First { get; set; }
        public int Last { get; set; }
    }
    public class HousePriceList
    {
        private readonly PagedResult<HousePriceTransaction> _results;

        public HousePriceList( PagedResult<HousePriceTransaction> results)
        {
            _results = results;
            YearRange = new YearRange(){First = 1995,Last = 2019};
        }
        public YearRange YearRange { get;  }
        public bool MoreAvailable => _results.MoreAvailable;
        public IEnumerable<HousePriceTransaction> Results => _results.Results;

    }
}
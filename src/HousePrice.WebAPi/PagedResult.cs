using System.Collections.Generic;

namespace HousePrice.WebAPi
{
    public class PagedResult<T>
    {
        public PagedResult(long totalRows, IEnumerable<T> results)
        {
            _totalRows = totalRows;
            _results = results;
        }
        private readonly long _totalRows;
        private readonly IEnumerable<T> _results;
        public long TotalRows => _totalRows;
        public IEnumerable<T> Results => _results;
    }
}
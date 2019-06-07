using System.Collections.Generic;

namespace HousePrice.Api.Core.SharedKernel
{
    public class PagedResult<T>
    {
        public PagedResult(bool moreAvailable, IEnumerable<T> results)
        {
            _moreAvailable = moreAvailable;
            _results = results;
        }
        private readonly bool _moreAvailable;
        private readonly IEnumerable<T> _results;
        public bool MoreAvailable => _moreAvailable;
        public IEnumerable<T> Results => _results;
    }
}
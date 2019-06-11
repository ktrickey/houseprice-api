using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Services
{
    internal interface IPostcodeLookup
    {
        Task<IPostcodeData> GetByPostcode(string postcode);
    }

    internal class PostcodeLookup : IPostcodeLookup
    {
        private readonly IPostcodeRepository _postcodeRepo;

        public PostcodeLookup(IPostcodeRepository postcodeRepo)
        {
            _postcodeRepo = postcodeRepo;

        }
        public async Task<IPostcodeData> GetByPostcode(string postcode)
        {
            return await _postcodeRepo.GetPostcode(postcode);

        }
    }
}
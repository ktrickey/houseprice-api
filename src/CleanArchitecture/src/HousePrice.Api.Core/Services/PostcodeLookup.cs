using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;
using HousePrice.Api.Core.Interfaces;

namespace HousePrice.Api.Core.Services
{
    public interface IPostcodeLookup
    {
        Task<PostcodeData> GetByPostcode(string postcode);
    }

    public class PostcodeLookup : IPostcodeLookup
    {
        private readonly IPostcodeRepository _postcodeRepo;

        public PostcodeLookup(IPostcodeRepository postcodeRepo)
        {
            _postcodeRepo = postcodeRepo;

        }
        public async Task<PostcodeData> GetByPostcode(string postcode)
        {
            return await _postcodeRepo.GetPostcode(postcode);

        }
    }
}
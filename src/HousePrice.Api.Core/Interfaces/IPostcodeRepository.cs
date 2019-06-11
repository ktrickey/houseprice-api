using System.Threading.Tasks;
using HousePrice.Api.Core.Entities;

namespace HousePrice.Api.Core.Interfaces
{
    public interface IPostcodeRepository
    {
        Task<IPostcodeData> GetPostcode(string postcode);
    }

}
using System.Threading.Tasks;
using AutoEstate.Data.Entities;
using AutoEstate.Web.Models;

namespace AutoEstate.Web.Interfaces
{
    public interface IAccountsService
    {
         Task<ApplicationUser> CreateUserAsync(RegisterModel model);
    }
}
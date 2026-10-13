using GT.Factory.Common.Models;
using GT.Factory.Common.Models.ProductModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.GTUser.Repository
{
    public interface ILocalUserService
    {

        Task RecordLoginAsync(User user, string loginIp = null);




        Task InsertNewUsersInforAsync(User user);

        Task<List<User>> GetAllAsync();

        //List<User> GetAll();

        Task<User> GetByUIDAsync(string cardId);

        Task<User> GetByJobAsync(string jobNub, string password);

        Task<User> GetByRoleIdAsync(string username, string password);

        Task UpdateAsync(User user);

        Task DeleteAsync(string jobNub);
    }
}

using GT.Factory.Common.Interfaces;
using GT.Factory.Common.Models;
using GT.Factory.GTUser.Repository;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GT.Factory.GTUser.Repository
{
    public class LocalUserService : ILocalUserService
    {
        private readonly ILocalUserService _repository;

        public LocalUserService(ILocalUserService repository)
        {
            _repository = repository;
        }

        public async Task InsertNewUsersInforAsync(User user)
        {
             await _repository.InsertNewUsersInforAsync(user);
        }

        public async Task<User> GetByUIDAsync(string cardId)
        {
            return await _repository.GetByUIDAsync(cardId);

        }

        public async Task<User> GetByJobAsync(string jobNub, string password)
        {
            return await _repository.GetByJobAsync(jobNub, password);


        }

        public async Task<User> GetByRoleIdAsync(string username, string password)
        {
            return await _repository.GetByRoleIdAsync(username, password);


        }

        public async Task UpdateAsync(User user)
        {
            await _repository.UpdateAsync(user);


        }


        public Task DeleteAsync(string jobNub)
        {
            return _repository.DeleteAsync(jobNub);
        }

        public async Task RecordLoginAsync(User user, string loginIp = null)
        {
             await _repository.RecordLoginAsync(user, loginIp);
        }


        public async Task<List<User>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}

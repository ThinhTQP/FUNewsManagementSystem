using BusinessObjects.Entities;
using FUNews.DAL.Repositories;
using FUNews.BLL.UnitOfWorks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FUNews.BLL.Services
{
    public class SystemAccountService : ISystemAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<SystemAccount, short> _systemAccountRepository;

        public SystemAccountService(IGenericRepository<SystemAccount, short> systemAccountRepository, IUnitOfWork unitOfWork)
        {
            _systemAccountRepository = systemAccountRepository;
            _unitOfWork = unitOfWork;
        }

        // Get all system accounts
        public async Task<IEnumerable<SystemAccount>> GetAllSystemAccountsAsync()
        {
            return await _systemAccountRepository.FindAll().ToListAsync();
        }

        // Get system account by ID
        public async Task<SystemAccount?> GetSystemAccountByIdAsync(short id)
        {
            return await _systemAccountRepository.FindById(id, "AccountId");
        }

        // Add a new system account
        public async Task AddSystemAccountAsync(SystemAccount systemAccount)
        {
            if (systemAccount == null)
                throw new ArgumentNullException(nameof(systemAccount));

            _systemAccountRepository.Create(systemAccount);
            await _unitOfWork.SaveChange(); // Save changes using UnitOfWork
        }

        // Update an existing system account
        public async Task UpdateSystemAccountAsync(SystemAccount systemAccount)
        {
            if (systemAccount == null)
                throw new ArgumentNullException(nameof(systemAccount));

            _systemAccountRepository.Update(systemAccount);
            await _unitOfWork.SaveChange(); // Save changes using UnitOfWork
        }

        // Delete a system account by ID
        public async Task DeleteSystemAccountAsync(short id)
        {
            var systemAccount = await _systemAccountRepository.FindById(id, "AccountId");
            if (systemAccount == null)
                throw new ArgumentException("System account not found");

            _systemAccountRepository.Delete(systemAccount);
            await _unitOfWork.SaveChange(); // Save changes using UnitOfWork
        }
    }
}

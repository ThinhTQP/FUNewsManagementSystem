using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Entities;
using System.Collections.Generic;

public interface ISystemAccountService
{
    Task<IEnumerable<SystemAccount>> GetAllSystemAccountsAsync();
    Task<SystemAccount?> GetSystemAccountByIdAsync(short id);
    Task AddSystemAccountAsync(SystemAccount systemAccount);
    Task UpdateSystemAccountAsync(SystemAccount systemAccount);
    Task DeleteSystemAccountAsync(short id);
}

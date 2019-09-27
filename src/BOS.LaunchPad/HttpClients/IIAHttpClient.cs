using BOS.LaunchPad.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BOS.LaunchPad.HttpClients
{
    public interface IIAHttpClient
    {
        Task<List<Module>> GetModulesAsync(bool filterDeleted = true);
        Task<List<PermissionsSet>> GetPermissionsForOwner(Guid ownerId, bool filterDeleted = true);
    }
}

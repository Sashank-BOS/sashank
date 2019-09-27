using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BOS.LaunchPad.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BOS.LaunchPad.HttpClients
{
    public class IAHttpClient : IIAHttpClient
    {
        private readonly HttpClient _client;

        public IAHttpClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<List<Module>> GetModulesAsync(bool filterDeleted = true)
        {
            string queryString = filterDeleted ? "Modules?$expand=Operations&filter=Deleted eq false&api-version=1.0" : "Modules?$expand=Operations&api-version=1.0";

            var response = await _client.GetAsync($"{_client.BaseAddress}{queryString}").ConfigureAwait(false);
            var json = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
            var modules = json["value"] == null ? new List<Module>() : JsonConvert.DeserializeObject<List<Module>>(json["value"].ToString());

            return modules;
        }

        public async Task<List<PermissionsSet>> GetPermissionsForOwner(Guid ownerId, bool filterDeleted = true)
        {
            string queryString = filterDeleted ? $"Permissions/GetOwnerPermissionsSets(ownerId={ownerId})?$expand=Permissions&filter=deleted eq false&api-version=1.0"
                : $"Permissions/GetOwnerPermissionsSets(ownerId={ownerId})?$expand=Permissions&api-version=1.0";
            
            var response = await _client.GetAsync($"{_client.BaseAddress}{queryString}");
            var json = JsonConvert.DeserializeObject<JObject>(response.Content.ReadAsStringAsync().Result);
            var permissionsSets = json["value"] == null ? new List<PermissionsSet>() : JsonConvert.DeserializeObject<List<PermissionsSet>>(json["value"].ToString());

            return permissionsSets;
        }
    }
}

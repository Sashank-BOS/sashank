using BOS.Auth.Client.ClientModels;
using System;
namespace BOS.LaunchPad.Models
{
    public class Role : IRole
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Deleted { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset LastModifiedOn { get; set; }
    }
}

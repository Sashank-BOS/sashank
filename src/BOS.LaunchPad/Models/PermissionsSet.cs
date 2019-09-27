using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BOS.LaunchPad.Models
{
    public class PermissionsSet : IPermissionsSet
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public Guid ReferenceId { get; set; }
        public string ReferenceName { get; set; }
        public string Code { get; set; }
        public bool Deleted { get; set; }
        public SetType Type { get; set; }
        [JsonProperty("Permissions", ItemConverterType = typeof(ConcreteConverter<IOperation, Operation>))]
        public List<IOperation> Permissions { get; set; } = new List<IOperation>();
    }

}

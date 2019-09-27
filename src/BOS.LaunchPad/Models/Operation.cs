using BOS.Base.Client;
using BOS.IA.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BOS.LaunchPad.Models
{
    public class Operation : IOperation
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Deleted { get; set; }
        public bool IsAssigned { get; set; }
        public Guid? ParentOperationId { get; set; }
        [JsonProperty("ChildOperations", ItemConverterType = typeof(ConcreteConverter<IOperation, Operation>))]
        public List<IOperation> ChildOperations { get; set; }
        public IOperation ParentOperation { get; set; }
    }
}

using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Activities;

namespace WorkflowConfigurator.Models.Workflow
{
    public class WorkflowDefinitionBackup
    {
        public string? WorkflowDefinitionId { get; set; }
        public string? Name { get; set; }
        public object? Context { get; set; }
        public int Version { get; set; }
        public List<ActivityDefinition> Activities { get; set; }
        public List<ActivityConnection> ActivityConnections { get; set; }
        public int StartingConnection { get; set; }
        public ActivityDefinition StartingDefinition { get; set; }

        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Creator { get; set; }
        public string? Reviewer { get; set; }

        public string? CompanyId { get; set; }

    }

}

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Services.Helper;

namespace WorkflowConfigurator.Models.Workflow
{
    [Table("WorkflowInstance")]
    public class WorkflowInstance
    {
        [BsonId]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string? InstanceName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string WorkflowDefinitionId { get; set; }

        [BsonRepresentation(BsonType.String)]
        public WorkflowStatus Status { get; set; }

        public Dictionary<string, string> GlobalVariables { get; set; } = new Dictionary<string, string>();
        public int CurrentActivityId { get; set; }
        public int ProjectId { get; set; }

        /// <summary>
        /// JSON value of the latest executed activity output object
        /// </summary>
        public string LastOutput { get; private set; }

        public ActivityStateResult CurrentState { get; set; }

        public List<ActivityResult> ActivityResults { get; set; } = new List<ActivityResult>();

        public List<ActivityConnection> ActivityConnections { get; set; }

        public string Owner { get; set; }

        public string CompanyId { get; set; }

        public DateTime? CreatedOn { get; set; }

        public int MiniWFStep {get; set;} = -1;

        public string LastMiniWFOutcome {get; set;}

        public int DefinitionVer { get; set; }

        public void SetLastOutput(string val)
        {
            LastOutput = val;
        }

        /// <summary>
        /// Return global variable or null if it was not set
        /// </summary>
        /// <returns></returns>
        public string GetVariable(string key)
        {
            if (GlobalVariables == null || !GlobalVariables.ContainsKey(key))
            {
                return null;
            }

            return GlobalVariables[key];
        }
    }
}

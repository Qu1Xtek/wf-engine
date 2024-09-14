using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkflowConfiguration.Infrastructure
{
    public class WorkflowDefinitionStoreSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string WorkflowDefinitionCollectionName { get; set; } = null!;
        public string WorkflowDefinitionBackupCollectionName { get; set; } = null!;

        public string UsersCollectionName { get; set; } = null!;

        public string UserSessionsCollectionName { get; set; } = null!;

        public string WorkflowInstanceCollectionName { get; set; } = null!;
        public string ActivityTimerCollectionName { get; set; } = null!;
        public string ArchivedActivityTimerCollectionName { get; set; } = null!;
        public string MaterialCollectionName { get; set; } = null!;
    }
}

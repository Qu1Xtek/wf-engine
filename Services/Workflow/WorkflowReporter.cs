using Newtonsoft.Json;
using WorkflowConfigurator.Interface.Workflow;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Models.Workflow.Dto;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Workflow
{
    public class WorkflowReporter : IWorkflowReporter
    {
        private WFInstanceRepo _wfInstanceRepo;
        private WorkflowInstanceRepository _wfInstanceService;
        private readonly WorkflowDefinitionRepository _wfDefinitionService;

        public WorkflowReporter(WFInstanceRepo wfIRepo, WorkflowInstanceRepository wfInstanceService, WorkflowDefinitionRepository wfDefinitionService)
        {
            _wfInstanceRepo = wfIRepo;
            _wfInstanceService = wfInstanceService;
            _wfDefinitionService = wfDefinitionService;

        }

        private List<WorkflowReportDTO> PrepareData(List<WorkflowInstance> instances)
        {
            List<WorkflowReportDTO> data = new List<WorkflowReportDTO>();

            foreach (var instance in instances)
            {
                data.Add(new WorkflowReportDTO
                {
                    Id = instance.Id,
                    DefinitionId = instance.WorkflowDefinitionId,
                    DefinitionVersion = instance.DefinitionVer.ToString(),
                    Status = instance.Status.ToString(),
                    WorkflowInstanceName = instance.InstanceName
                });
            }

            return data;
        }

        public async Task<List<WorkflowReportDTO>> GetRecentReport(string companyId)
        {
            var instances = await _wfInstanceRepo.GetRecent(companyId);

            return PrepareData(instances);
        }

        public async Task<WorkflowReportDTO> GetReportById(string wfId)
        {
            var instance = await _wfInstanceService.GetAsync(wfId);
            var wfReport = new WorkflowReportDTO
            {
                Steps = new List<StepsResultDTO>(),
                DefinitionId = instance.WorkflowDefinitionId,
                Id = instance.Id,
                DefinitionVersion = "1", // add version in wf instace
                Status = instance.Status.ToString(),
                WorkflowInstanceName = instance.InstanceName
            };

            var wfDefinition = await _wfDefinitionService.GetAsync(instance.WorkflowDefinitionId);

            foreach (var item in instance.ActivityResults)
            {
                var activityDefinition = wfDefinition
                    .WorkflowDefinitionVersionData[instance.DefinitionVer]
                    .Activities
                    .Single(a => a.ActivityId == item.ActivitySource);

                wfReport.Steps.Add(new StepsResultDTO(activityDefinition, item));
            }

            return wfReport;
        }

        public async Task<List<WorkflowReportDTO>> GetReportsBetween(DateTime from, DateTime to, string companyId)
        {
            var instances = await _wfInstanceRepo.GetFromTo(from, to, companyId);

            return PrepareData(instances);
        }
    }
}

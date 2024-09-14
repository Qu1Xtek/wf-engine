using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Models.Workflow.Dto;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.DIP;

namespace WorkflowConfigurator.Services.Service
{
    public class WorkflowDefinitionService
    {
        private readonly WorkflowDefinitionRepository _workflowDefinitionRepository;
        private readonly WorkflowDefinitionBackupService _workflowDefinitionBackupService;
        private readonly UserSessionService _userSessionService;
        private readonly DIPService _dipService;
        public WorkflowDefinitionService(WorkflowDefinitionRepository workflowDefinitionRepository,
            WorkflowDefinitionBackupService workflowDefinitionBackupService,
            DIPService dipService,
            UserSessionService userSessionService)
        {
            _workflowDefinitionRepository = workflowDefinitionRepository;
            _workflowDefinitionBackupService = workflowDefinitionBackupService;
            _dipService = dipService;
            _userSessionService = userSessionService;
        }
        public async Task<List<WorkflowDefinition>> GetAsync() =>
            await _workflowDefinitionRepository.GetAsync();

        public async Task<WorkflowDefinition> GetAsync(string id) =>
            await _workflowDefinitionRepository.GetAsync(id);

        public WorkflowDefinition Get(string id)
        {
            return _workflowDefinitionRepository.Get(id);
        }

        public async Task<WorkflowDefinition?> GetByNameAsync(string name) =>
             await _workflowDefinitionRepository.GetByNameAsync(name);

        public async Task<WorkflowDefinitionDto> CreateAsync(WorkflowDefinitionDto workflowDefinitionDto, UserLogin user)
        {
            WorkflowDefinition workflowDefinition = new WorkflowDefinition();
            workflowDefinition.Name = workflowDefinitionDto.Name;
            workflowDefinition.Version = 1;
            workflowDefinition.CreationDate = DateTime.UtcNow;
            workflowDefinition.Creator = user.Email;
            workflowDefinition.CompanyId = user.CompanyId;
            workflowDefinition.IsMiniWorkflow = workflowDefinitionDto.IsMiniWf;
            workflowDefinition.Draft = null;

            WorkflowDefinitionData data = new WorkflowDefinitionData();
            data.Activities = workflowDefinitionDto.Activities;
            data.ActivityConnections = workflowDefinitionDto.ActivityConnections;
            data.StartingActivity = workflowDefinitionDto.StartingActivity;
            data.StartingDefinition = workflowDefinitionDto.StartingDefinition;

            workflowDefinition.WorkflowDefinitionVersionData = new Dictionary<int, WorkflowDefinitionData>()
            {
                { workflowDefinition.Version, data }
            };

            var exits = await GetByNameAsync(workflowDefinition.Name);
            if (exits != null && exits.Name == workflowDefinition.Name && exits.Version == 0)
            {
                workflowDefinition.Id = exits.Id;
                await _workflowDefinitionRepository.UpdateAsync(workflowDefinition.Id, workflowDefinition);
            }
            else
            {
                await _workflowDefinitionRepository.CreateAsync(workflowDefinition);
            }

            WorkflowDefinitionBackup workflowDefinitionBackup = new WorkflowDefinitionBackup();

            workflowDefinitionBackup.WorkflowDefinitionId = workflowDefinition.Id;
            workflowDefinitionBackup.Name = workflowDefinition.Name;
            workflowDefinitionBackup.Version = workflowDefinition.Version;
            workflowDefinitionBackup.Activities = data.Activities;
            workflowDefinitionBackup.ActivityConnections = data.ActivityConnections;
            workflowDefinitionBackup.StartingConnection = data.StartingActivity;
            workflowDefinitionBackup.StartingDefinition = data.StartingDefinition;
            workflowDefinitionBackup.CreationDate = DateTime.UtcNow;
            workflowDefinitionBackup.Creator = workflowDefinition.Creator;
            workflowDefinitionBackup.CompanyId = workflowDefinition.CompanyId;

            await _workflowDefinitionBackupService.CreateAsync(workflowDefinitionBackup);

            await _dipService.CreateWorkflowDefinitionRecord(workflowDefinition.Id);

            return ConvertEntityToDto(workflowDefinition);
        }


        public async Task<WorkflowDefinitionDto> UpdateAsync(WorkflowDefinitionDto workflowDefinitionDto)
        {
            WorkflowDefinition workflowDefinition = await GetAsync(workflowDefinitionDto.Id);

            WorkflowDefinitionData data = new WorkflowDefinitionData();
            data.Activities = workflowDefinitionDto.Activities;
            data.ActivityConnections = workflowDefinitionDto.ActivityConnections;
            data.StartingActivity = workflowDefinitionDto.StartingActivity;
            data.StartingDefinition = workflowDefinitionDto.StartingDefinition;

            workflowDefinition.Version = workflowDefinition.Version + 1;
            workflowDefinition.WorkflowDefinitionVersionData.Add(workflowDefinition.Version, data);
            workflowDefinition.UpdateDate = DateTime.UtcNow;
            workflowDefinition.Name = workflowDefinitionDto.Name;

            // Allow only to turn workflows to mini, reverse action should not be allowed
            if (workflowDefinitionDto.IsMiniWf)
            {
                workflowDefinition.IsMiniWorkflow = true;
            }

            await _workflowDefinitionRepository.UpdateAsync(workflowDefinition.Id, workflowDefinition);

            WorkflowDefinitionBackup workflowDefinitionBackup = new WorkflowDefinitionBackup();

            workflowDefinitionBackup.WorkflowDefinitionId = workflowDefinition.Id;
            workflowDefinitionBackup.Name = workflowDefinition.Name;
            workflowDefinitionBackup.Version = workflowDefinition.Version;
            workflowDefinitionBackup.Activities = data.Activities;
            workflowDefinitionBackup.ActivityConnections = data.ActivityConnections;
            workflowDefinitionBackup.StartingConnection = data.StartingActivity;
            workflowDefinitionBackup.CreationDate = workflowDefinition.CreationDate;
            workflowDefinitionBackup.Creator = workflowDefinition.Creator;
            workflowDefinitionBackup.Reviewer = workflowDefinition.Reviewer;
            workflowDefinitionBackup.UpdateDate = workflowDefinition.UpdateDate;
            workflowDefinitionBackup.StartingDefinition = workflowDefinitionDto.StartingDefinition;

            await _workflowDefinitionBackupService.CreateAsync(workflowDefinitionBackup);

            await _dipService.UpdateWorkflowDefinitionRecord(workflowDefinition);

            return ConvertEntityToDto(workflowDefinition);
        }

        public async Task RemoveAsync(string id)
        {
            await _workflowDefinitionRepository.RemoveAsync(id);

            await _userSessionService.RemoveByWorkflowDefinitionId(id);
        }
           


        public async Task<List<WorkflowDefinitionDto>> GetWorkflowDefition(UserLogin user, bool? onlyMinies)
        {
            List<WorkflowDefinition> workflowDefinitions = await GetAsync();
            List<WorkflowDefinitionDto> result = new List<WorkflowDefinitionDto>();

            foreach (WorkflowDefinition workflowDefinition in workflowDefinitions)
            {
                if (workflowDefinition.CompanyId == user.CompanyId
                    && (!onlyMinies.HasValue ||
                    workflowDefinition.IsMiniWorkflow == onlyMinies.Value))
                {
                    result.Add(ConvertEntityToDto(workflowDefinition));
                }
            }


            return result;
        }

        public async Task<WorkflowDefinitionDto> SaveDraftAsync(WorkflowDefinitionDto workflowDefinitionDto, UserLogin user)
        {
            // Step 1: Create or get WorkflowDefinition
            WorkflowDefinition workflowDefinition = await CreateOrGetWorkflowDefinition(workflowDefinitionDto, user);

            // Allow only to turn workflows to mini, reverse action should not be allowed
            if (workflowDefinitionDto.IsMiniWf)
            {
                workflowDefinition.IsMiniWorkflow = true;
            }

            // Step 3: Handle repository interaction and backup creation
            await HandleRepositoryInteractionAndBackup(workflowDefinition);

            // Convert and return the result
            return ConvertEntityToDto(workflowDefinition);
        }

        private WorkflowDefinitionDto ConvertEntityToDto(WorkflowDefinition workflowDefinition)
        {
            WorkflowDefinitionDto dto = new WorkflowDefinitionDto();
            dto.Id = workflowDefinition.Id;
            dto.Name = workflowDefinition.Name;
            dto.Version = workflowDefinition.Version;
            dto.CreationDate = workflowDefinition.CreationDate;
            dto.Creator = workflowDefinition.Creator;
            dto.Reviewer = workflowDefinition.Reviewer;
            dto.UpdateDate = workflowDefinition.UpdateDate;
            dto.CompanyId = workflowDefinition.CompanyId;
            dto.IsMiniWf = workflowDefinition.IsMiniWorkflow;
            dto.Draft = workflowDefinition.Draft;
            WorkflowDefinitionData data = workflowDefinition.WorkflowDefinitionVersionData[workflowDefinition.Version];

            dto.Activities = data.Activities;
            dto.ActivityConnections = data.ActivityConnections;
            dto.StartingActivity = data.StartingActivity;
            dto.StartingDefinition = data.StartingDefinition;
            return dto;
        }

        private async Task HandleRepositoryInteractionAndBackup(WorkflowDefinition workflowDefinition)
        {
            var definitionData = new WorkflowDefinitionData()
            {
                Activities = workflowDefinition.Draft.Activities,
                ActivityConnections = workflowDefinition.Draft.ActivityConnections,
                StartingActivity = workflowDefinition.Draft.StartingActivity,
                StartingDefinition = workflowDefinition.Draft.StartingDefinition
            };

            if (workflowDefinition.Id is null)
            {
                workflowDefinition.WorkflowDefinitionVersionData = new Dictionary<int, WorkflowDefinitionData>()
                 {{ workflowDefinition.Version, definitionData }};

                var exits = await GetByNameAsync(workflowDefinition.Name);
                if (exits is null)
                {
                    await _workflowDefinitionRepository.CreateAsync(workflowDefinition);
                }
            }
            else
            {
                await _workflowDefinitionRepository.UpdateAsync(workflowDefinition.Id.ToString(), workflowDefinition);
                await CreateBackup(workflowDefinition, definitionData);
                await _dipService.UpdateWorkflowDefinitionRecord(workflowDefinition);
            }
        }
        private async Task CreateBackup(WorkflowDefinition workflowDefinition, WorkflowDefinitionData data)
        {
            var backup = new WorkflowDefinitionBackup
            {
                WorkflowDefinitionId = workflowDefinition.Id.ToString(),
                Name = workflowDefinition.Name,
                Version = workflowDefinition.Version,
                Activities = data.Activities,
                ActivityConnections = data.ActivityConnections,
                StartingConnection = data.StartingActivity,
                CreationDate = workflowDefinition.CreationDate,
                Creator = workflowDefinition.Creator,
                Reviewer = workflowDefinition.Reviewer,
                UpdateDate = workflowDefinition.UpdateDate,
                StartingDefinition = data.StartingDefinition
            };
            await _workflowDefinitionBackupService.CreateAsync(backup);
        }

        private async Task<WorkflowDefinition> CreateOrGetWorkflowDefinition(WorkflowDefinitionDto workflowDefinitionDto, UserLogin user)
        {
            if (workflowDefinitionDto.Id != null)
            {
                var result = await GetAsync(workflowDefinitionDto.Id);
                result.Draft = workflowDefinitionDto;
                return result;

            }

            return new WorkflowDefinition
            {
                Name = workflowDefinitionDto.Name,
                Version = 0,
                CreationDate = DateTime.UtcNow,
                Creator = user.Email,
                CompanyId = user.CompanyId,
                IsMiniWorkflow = workflowDefinitionDto.IsMiniWf,
                Reviewer = user.Email,
                DraftLastUpdateDate = DateTime.UtcNow,
                Draft = workflowDefinitionDto,

            };
        }
    }
}

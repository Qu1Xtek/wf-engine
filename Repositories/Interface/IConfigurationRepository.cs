using WorkflowConfigurator.Models;

namespace WorkflowConfigurator.Repositories.Interface
{
    public interface IConfigurationRepository
    {
        Task<ProjectSettings> GetFirstAsync();
    }
}

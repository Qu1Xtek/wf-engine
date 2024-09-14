using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.DTO.UserManagement;

namespace WorkflowConfigurator.Repositories
{
    public interface IJWTManagerRepository
    {
        Task<TokenDTO> Authenticate(string username, string password);

        Task<bool> IsInvalid(string token);
    }

}
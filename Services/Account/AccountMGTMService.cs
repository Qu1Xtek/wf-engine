using Newtonsoft.Json;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.DTO.UserManagement;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Services.Account
{
    public class AccountMGTMService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly UserRepository _userRepo;

        public AccountMGTMService(HttpClient httpClient, IConfiguration config, UserRepository uRep)
        {
            _config = config;
            _httpClient = httpClient;
            _userRepo = uRep;

            _httpClient.BaseAddress = new Uri(
                config
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.BaseURL)
                );
        }


        /// <summary>
        /// This is a leftover method of the old usermanagement
        /// should be reworked to work with the new keycloak UM
        /// </summary>
        /// <param name="account"></param>
        /// <param name="secretauth"></param>
        /// <returns></returns>
        public async Task<string> CreateUser(AccountManagementDTO account, string secretauth)
        {
            var localUser = new User
            {
                //Email = account.Email,
                //CompanyId = account.CompanyName,
            };


            var content = new StringContent(JsonConvert.SerializeObject(account), null, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, _config
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.CreateURL)
                );

            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Authorization", secretauth);
            request.Content = content;

            var result = await _httpClient.SendAsync(request);

            if (!result.IsSuccessStatusCode)
            {
                return "Couldnt create account on user management";
            }

            var response = await result.Content.ReadAsStringAsync();

            // await _userRepo.Create(localUser);
            Console.WriteLine(response);

            return "Account sucessfully created";

        }
    }
}

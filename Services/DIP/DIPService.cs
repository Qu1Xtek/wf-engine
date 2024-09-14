using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using WorkflowConfigurator.Models.DIP;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.DIP
{
    public class DIPService
    {
        private HttpClient _client { get; set; }
        private WorkflowDefinitionRepository _wfDefRepo { get; set; }

        private string _token { get; set; }

        private string _dipUrl { get; set; }

        public DIPService(WorkflowDefinitionRepository repo)
        {
            _client = new HttpClient(); //TODO fix this
            _wfDefRepo = repo;
            _dipUrl = "https://dip.dev.arxum.app";
        }

        private async Task<bool> Login()
        {
            DIPLogin login = new DIPLogin();
            login.email = "dip.dev.org@arxum.app";
            login.password = "dip.dev.org@arxum.app20230220-moh";

            StringContent jsonContent = new(
                JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");


            using HttpResponseMessage response = await _client.PostAsync(
                _dipUrl+"/suite/api/login",
                jsonContent);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            _token = JsonConvert.DeserializeObject<DIPLoginResponse>(jsonResponse).Token;


            if (_client.DefaultRequestHeaders.Contains("Authorization"))
            {
                _client.DefaultRequestHeaders.Remove("Authorization");
            }
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);


            return true;
        }

        public async Task<bool> CreateWorkflowDefinitionRecord(string wfDefId)
        {
            return true;
            await Login();
            DIPRequestDto request = new DIPRequestDto();

            var workflowDefinition = await _wfDefRepo.GetAsync(wfDefId);

            request.IntegrityRecordId = workflowDefinition.Id;

            string hashedValue = QuickHash(JsonConvert.SerializeObject(workflowDefinition));

            request.Index1s = new string[] { hashedValue };
            request.Index2s = new string[] { };
            request.Index3s = new string[] { };
            request.Meta = new WorkflowDefinitionMeta(hashedValue);

            using MultipartFormDataContent content = PrepareMultiPart(
            System.Text.Json.JsonSerializer.Serialize(request));

            using HttpResponseMessage response = await _client.PostAsync(
                _dipUrl +"/suite/api/v2/integrity-records",
                content);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return true;

        }

        public async Task<bool> UpdateWorkflowDefinitionRecord(WorkflowDefinition workflowDefinition)
        {
            return true;

            await Login();
            DIPRequestDto request = new DIPRequestDto();

            request.IntegrityRecordId = workflowDefinition.Id;

            string hashedValue = QuickHash(JsonConvert.SerializeObject(workflowDefinition));

            request.Index1s = new string[] { hashedValue };
            request.Index2s = new string[] { };
            request.Index3s = new string[] { };
            request.Meta = new { Hash = hashedValue };

            using MultipartFormDataContent content = PrepareMultiPart(
            System.Text.Json.JsonSerializer.Serialize(request));

            using HttpResponseMessage response = await _client.PatchAsync(
                _dipUrl +"/suite/api/v2/integrity-records/" + request.IntegrityRecordId,
                content);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return true;
        }

        public async Task<DIPResponseRecord<WorkflowDefinitionMeta>> GetIntegrityRecord(string integrityRecordId)
        {
            await Login();
            using HttpResponseMessage response = await _client.GetAsync(_dipUrl +"/suite/api/v2/integrity-records/" + integrityRecordId);

            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<DIPResponseRecord<WorkflowDefinitionMeta>>(jsonResponse);
        }


        public async Task<bool> CreateWorkflowInstanceRecord(WorkflowInstance workflowInstance)
        {
            return true;

            await Login();

            DIPRequestDto request = new DIPRequestDto();

            request.IntegrityRecordId = workflowInstance.Id + "_" + workflowInstance.ProjectId;

            string hashedValue = QuickHash(JsonConvert.SerializeObject(workflowInstance));

            request.Index1s = new string[] { hashedValue };
            request.Index2s = new string[] { };
            request.Index3s = new string[] { };
            request.Meta = new WorkflowDefinitionMeta(hashedValue);


            using MultipartFormDataContent content = PrepareMultiPart(
            System.Text.Json.JsonSerializer.Serialize(request));

            using HttpResponseMessage response = await _client.PostAsync(
               _dipUrl + "/suite/api/v2/integrity-records",
                content);

            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            return true;
        }

        public async Task<bool> UpdateWorkflowInstanceRecord(WorkflowInstance workflowInstance)
        {
            return true;

            await Login();
            DIPRequestDto request = new DIPRequestDto();

            request.IntegrityRecordId = workflowInstance.Id + "_" + workflowInstance.ProjectId;

            string hashedValue = QuickHash(JsonConvert.SerializeObject(workflowInstance));

            request.Index1s = new string[] { hashedValue };
            request.Index2s = new string[] { };
            request.Index3s = new string[] { };
            request.Meta = new { Hash = hashedValue };

            using MultipartFormDataContent content = PrepareMultiPart(
            System.Text.Json.JsonSerializer.Serialize(request));

            using HttpResponseMessage response = await _client.PatchAsync(
                _dipUrl + "/suite/api/v1/integrity-records/" + request.IntegrityRecordId,
                content);

            try
            {
                response.EnsureSuccessStatusCode();
            } catch (Exception ex)
            {
                Console.WriteLine(ex);

            }


            var jsonResponse = await response.Content.ReadAsStringAsync();

            return true;
        }


        private MultipartFormDataContent PrepareMultiPart(string jsonData, string filePath = null)
        {
            MultipartFormDataContent content = new MultipartFormDataContent();
            // Add JSON data as a string content
            StringContent jsonContent = new StringContent(jsonData);
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Add(jsonContent, "integrityRecord");

            if (filePath != null)
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    StreamContent fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", Path.GetFileName(filePath));
                }
            }

            return content;
        }

        public async Task<bool> CheckDefinitionHash(WorkflowDefinition definition)
        {
            var responseRecord = await GetIntegrityRecord(definition.Id);

            string hash = responseRecord.Meta.Hash;
            string hashToCheckAgainst = QuickHash(JsonConvert.SerializeObject(definition));

            if (!string.Equals(hash, hashToCheckAgainst))
            {
                throw new Exception("WorkflowDefinition wrongfully edited!");
            }

            return true;
 
        }

        private string QuickHash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var inputHash = SHA256.HashData(inputBytes);
            return Convert.ToHexString(inputHash);
        }

    }
}

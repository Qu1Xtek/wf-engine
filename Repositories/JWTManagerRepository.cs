using Amazon.S3;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.DTO.UserManagement;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class JWTManagerRepository : IJWTManagerRepository
	{
		private readonly IConfiguration _configuration;
        private readonly UserService _userService;

        public JWTManagerRepository(IConfiguration configuration, UserService userService)
        {
            _configuration = configuration;
            _userService = userService;
        }

        /// <summary>
        /// Authenticates the user in the user management
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
		public async Task<TokenDTO> Authenticate(string username, string password)
		{
            //var authToken = await GetAuthToken(username, password);

            //var baseaddr = _configuration
            //     .GetSection(Configs.UserMGMTSection)
            //     .GetValue<string>(Configs.ExternalEndpoints.BaseURL);

            // var baseUri = new Uri(baseaddr);

            // var requestUri = _configuration
            //     .GetSection(Configs.UserMGMTSection)
            //     .GetValue<string>(Configs.ExternalEndpoints.ValidateURL);

            // var client = new HttpClient();
            // client.BaseAddress = baseUri;


            // var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            // var collection = new List<KeyValuePair<string, string>>();
            // collection.Add(new("client_id", _configuration.GetValue<string>(Configs.KCBLASTDEVID)));
            // collection.Add(new("client_secret", _configuration.GetValue<string>(Configs.BLASTDEVKCSECRET)));
            // collection.Add(new("token", authToken.AccessToken));

            // request.Content = new FormUrlEncodedContent(collection);

            // var response = await client.SendAsync(request);

            User user = await _userService.GetOneAsync(username);

            if (user != null && user.password == password)
            {
                string token = GenerateJwtToken(username);
                TokenDTO result = new TokenDTO { AccessToken = token, Email = user.Email, Company = user.CompanyId, Active = true };
                return result;
            }

            return null;
            //var result = JsonConvert.DeserializeObject<TokenDTO>(await response.Content.ReadAsStringAsync());

            //result.AccessToken = authToken.AccessToken;
            
        }

        static string GenerateJwtToken(string username)
        {
            // Define security key and signing credentials
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Your_Secret_Key_Here"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Define the claims
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Create the token
            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials);

            // Return the serialized token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Validates the token by sending it to the usermanagement and returns false if it's not valid
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> IsInvalid(string? token)
        {
            if (token == null) return true;

            var baseaddr = _configuration
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.BaseURL);

            var baseUri = new Uri(baseaddr);

            var requestUri = _configuration
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.ValidateURL);

            var client = new HttpClient();
            client.BaseAddress = baseUri;

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var collection = new List<KeyValuePair<string, string>>
            {
            new("client_id", _configuration.GetValue<string>(Configs.KCBLASTDEVID)),
            new("client_secret", _configuration.GetValue<string>(Configs.BLASTDEVKCSECRET)),
            new("token", token)
            };

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);
            var result = JsonConvert.DeserializeObject<TokenDTO>(await response.Content.ReadAsStringAsync());


            result.AccessToken = token;
            return result.IsNotActive;
        }

        public async Task<TokenDTO> GetAuthToken(string username, string pass)
        {
            var baseaddr = _configuration
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.BaseURL);

            var baseUri = new Uri(baseaddr);

            var requestUri = _configuration
                .GetSection(Configs.UserMGMTSection)
                .GetValue<string>(Configs.ExternalEndpoints.LoginURL);

            var client = new HttpClient();
            client.BaseAddress = baseUri;

            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8
                .GetBytes($"{_configuration.GetValue<string>(Configs.KCBLASTDEVID)}:{_configuration.GetValue<string>(Configs.BLASTDEVKCSECRET)}"));           

            request.Headers.Add(
                "Authorization", 
                $"Basic {auth}");

            var collection = new List<KeyValuePair<string, string>>();
            collection.Add(new("username", username));
            collection.Add(new("password", pass));
            collection.Add(new("grant_type", "password")); 

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);
            var result = JsonConvert.DeserializeObject<TokenDTO>(await response.Content.ReadAsStringAsync());


            result.AccessToken = result.AccessToken;
            return result;
        }
    }
}

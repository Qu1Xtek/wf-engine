using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Text;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Interface.Workflow;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Services.Helper;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Services.Workflow;
using WorkflowConfigurator.Services.PicaviTranslator;
using WorkflowConfigurator.Services.Materials;
using System.IdentityModel.Tokens.Jwt;
using WorkflowConfigurator.Services.Account;
using WorkflowConfigurator.Services.Workflow.Execution;
using WorkflowConfigurator.Services.Printer;
using WorkflowConfigurator.Services.Service;
using WorkflowConfigurator.Services.ActivityDefinitions;
using WorkflowConfigurator.Services.Caching;
using WorkflowConfigurator.Repositories.Configurations;
using WorkflowConfigurator.Services.Caching.ActivityDefinitions;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var objectSerializer = new ObjectSerializer(
     type => ObjectSerializer.DefaultAllowedTypes(type) || type == typeof(List<TextValueValue>) ||
     type == typeof(TextValue) ||
     type == typeof(TextValueValue));

// Register the custom serializer for List<TextValueValue>
//BsonSerializer.RegisterSerializer(typeof(List<TextValueValue>), new ListSerializer<TextValueValue>());


// Register the object serializer
BsonSerializer.RegisterSerializer(objectSerializer);
//BsonSerializer.RegisterSerializer(typeof(List<TextValueValue>), new ListSerializer<TextValueValue>());
// Register the WorkflowDefinitionStoreSettings instance
builder.Services.AddSingleton(new WorkflowDefinitionStoreSettings
{
    WorkflowDefinitionCollectionName = "WorkflowDefinition",
    WorkflowDefinitionBackupCollectionName = "WorkflowDefinitionBackup",
    WorkflowInstanceCollectionName = "WorkflowInstance",
    UsersCollectionName = "Users",
    UserSessionsCollectionName = "UserSessions",
    DatabaseName = "WorkflowConfiguration",
    ActivityTimerCollectionName = "ActivityTimers",
    ArchivedActivityTimerCollectionName = "ArchivedActivityTimers",
    MaterialCollectionName = "Materials",
    ConnectionString = "mongodb+srv://dedicateddevcluster.0yddt.mongodb.net/WorkflowConfiguration?authSource=%24external&authMechanism=MONGODB-AWS&retryWrites=true&w=majority"
});

// Register the WorkflowInstanceService with the correct lifetime scope

builder.Services.AddScoped<WorkflowInstanceRepository>();
builder.Services.AddScoped<WorkflowDefinitionRepository>();
builder.Services.AddScoped<WorkflowDefinitionBackupRepository>();
builder.Services.AddScoped<UserSessionService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MaterialRepository>();
builder.Services.AddScoped<PrinterService>();
builder.Services.AddScoped<MatCategoryRepository>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ActivityTimerRepository>();
builder.Services.AddScoped<ArchivedActivityTimerService>();
builder.Services.AddScoped<WFInstanceRepo>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IWorkflowReporter, WorkflowReporter>();
builder.Services.AddScoped<ActivityService>();
builder.Services.AddScoped<DIPService>();
builder.Services.AddScoped<ScreenTranslateService>();
builder.Services.AddScoped<IfScreenMessageTranslator>();
builder.Services.AddScoped<InputScreenTranslator>();
builder.Services.AddScoped<InputSplitScreenTranslator>();
builder.Services.AddScoped<InputWithAddonsScreenTranslator>();
builder.Services.AddScoped<MessageScreenTranslator>();
builder.Services.AddScoped<ScanScreenTranslator>();
builder.Services.AddScoped<ScanSplitScreenTranslator>();
builder.Services.AddScoped<UserConfigurableTimerTranslator>();
builder.Services.AddScoped<TimerScreenActivityTranslator>();
builder.Services.AddScoped<WebService>();
builder.Services.AddScoped<PrintAllLabelsTranslator>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<InstanceMaintainer>();
builder.Services.AddScoped<WorkflowInstanceService>();
builder.Services.AddScoped<WorkflowInvokerService>();
builder.Services.AddScoped<WorkflowDefinitionService>();
builder.Services.AddScoped<WorkflowDefinitionBackupService>();
builder.Services.AddScoped<MiniWorkflowService>();
builder.Services.AddScoped<PathTaker>();
builder.Services.AddScoped<WorkflowMaster>();
builder.Services.AddScoped<HotkeyHandler>();
builder.Services.AddScoped<ConfigurationsRepository>();
builder.Services.AddHostedService<GlobalCache>();
builder.Services.AddScoped<ActivityDefinitionMaintainer>();
builder.Services.AddScoped<LocalCache>();
builder.Services.AddScoped<ActivityTemplateLoader>();
builder.Services.AddScoped<ScreenBuilderService>();
builder.Services.AddHttpClient<AccountMGTMService>();
builder.Services.AddHttpClient<MaterialService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IJWTManagerRepository, JWTManagerRepository>();
builder.Services.AddHostedService<TimedHostedService>();


builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidateIssuerSigningKey = false,
        ClockSkew = TimeSpan.Zero,
        RequireExpirationTime = true,
        RequireSignedTokens = false,

        SignatureValidator = delegate (string token, TokenValidationParameters parames)
        {
            var jwt = new JwtSecurityToken(token);

            return jwt;
        },
    };
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = async (context) =>
        {
            var webService = context.HttpContext.RequestServices.GetService<WebService>();
            if (!await webService.ValidateSession(context))
            {
                context.Fail(new UnauthorizedAccessException());
            }
        }
    };
});

builder.Services.AddAuthorization(opt =>
{
    opt.FallbackPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});
string corsPolicyUrl = Environment.GetEnvironmentVariable("CorsPolicyUrl") ?? builder.Configuration["CorsPolicyUrl"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("",
            "https://designer.dev.arxum.app",
            "https://localhost:3333",
            "http://localhost:4200",
            "https://demo.dev.arxum.app", 
            "https://workflow-designer.dev.arxum.app").SetIsOriginAllowedToAllowWildcardSubdomains()
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName.ToLowerInvariant() == "debug")
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

MongoDBHelper.InitialSetup(app.Environment.EnvironmentName, builder.Configuration);

//app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints
    .MapControllers()
    .RequireCors("AllowAll"); // Apply the CORS policy to the controllers
});



Activity.Provider = app.Services;
app.Run();

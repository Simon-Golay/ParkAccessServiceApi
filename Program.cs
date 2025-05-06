using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using ParkAccessServiceApi.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(
    builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<ApiSettings>>().Value);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ParkAccess Graph API", Version = "v1" });

    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "ApiKey",
        Type = SecuritySchemeType.ApiKey,
        Description = "API key required to access this API"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireParkingManagerRole", policy =>
        policy.RequireRole("ParkingManager")); // nom du rôle défini dans Azure AD
});

builder.Services.AddSingleton<GraphService>();
builder.Services.AddSingleton<EventStoreService>();
builder.Services.AddSingleton<ParkingStoreService>();
builder.Services.AddSingleton<HistoryStoreService>();
builder.Services.AddHostedService<CalendarService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<EventTriggerService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkAccess Graph API v1"));
}

app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

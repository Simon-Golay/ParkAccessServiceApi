using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ParkAccessServiceApi.Settings;
using System;
using System.Threading.Tasks;

public class ApiKeyMiddleware
{
    private readonly ApiSettings _apiSettings;
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(ApiSettings apiSettings, RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _apiSettings = apiSettings;
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey("ApiKey"))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("API key is missing.");
            return;
        }

        var apiKey = context.Request.Headers["ApiKey"].ToString();

        if (apiKey != _apiSettings.ApiKey)
        {
            _logger.LogWarning("Invalid API key.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized access - Invalid API key.");
            return;
        }

        await _next(context);
    }
}

using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ParkAccess Graph API", Version = "v1" });
});

builder.Services.AddSingleton<GraphService>();
builder.Services.AddSingleton<EventStoreService>();
builder.Services.AddSingleton<ParkingStoreService>();
builder.Services.AddHostedService<CalendarService>();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<EventTriggerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkAccess Graph API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

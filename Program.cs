using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ParkAccess Graph API", Version = "v1" });
});

// Enregistrement du service Graph personnalisé
builder.Services.AddSingleton<GraphService>();

// Enregistrement du dépôt en mémoire pour les événements
builder.Services.AddSingleton<EventStoreService>();

// Enregistrement de la tâche d’arrière-plan pour interroger le calendrier périodiquement
builder.Services.AddHostedService<CalendarService>();

builder.Services.AddHttpClient();

// Enregistrement de la tâche d'arrière-plan pour vérifier et déclencher l'action
builder.Services.AddHostedService<EventTriggerService>();

var app = builder.Build();

// Configurer le pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ParkAccess Graph API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

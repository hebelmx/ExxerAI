using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure dependency injection
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAgentRepository, InMemoryAgentRepository>();
builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();

// Add CORS for development
builder.Services.AddCors(options =>
{
	options.AddPolicy("Development", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseCors("Development");
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();

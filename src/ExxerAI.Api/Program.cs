using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new() { 
		Title = "ExxerAI API", 
		Version = "v1",
		Description = "API for managing agents and tasks in the ExxerAI system"
	});
	
	// Include XML comments
	var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
	var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
	if (File.Exists(xmlPath))
	{
		c.IncludeXmlComments(xmlPath);
	}
});

// Configure dependency injection
builder.Services.AddScoped<IAgentService, AgentService>();
builder.Services.AddScoped<IAgentRepository, InMemoryAgentRepository>();

// Note: Task repository implementation needed for full functionality
// builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();

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
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExxerAI API V1");
		c.RoutePrefix = string.Empty; // Makes Swagger UI available at root
	});
	app.UseCors("Development");
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

app.Run();

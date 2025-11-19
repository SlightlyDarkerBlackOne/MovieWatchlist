using MovieWatchlist.Api.Extensions;
using MovieWatchlist.Application.Extensions;
using MovieWatchlist.Infrastructure.Extensions;
using MovieWatchlist.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddSwagger();
builder.Services.ConfigureSettings(builder.Configuration);
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure();
builder.Services.AddApplication();
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddApiCors(builder.Environment);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();

public partial class Program { }

using Amazon.DynamoDBv2;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// DynamoDB クライアントの設定
builder.Services.AddSingleton<IAmazonDynamoDB>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var config = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "ap-northeast-1")
    };

    // Development環境ではLocalStackを使用
    if (builder.Environment.IsDevelopment())
    {
        var serviceURL = configuration["DynamoDB:ServiceURL"];
        if (!string.IsNullOrEmpty(serviceURL))
        {
            config.ServiceURL = serviceURL;
            config.UseHttp = true;
        }
    }

    return new AmazonDynamoDBClient(config);
});

builder.Services.AddControllers();

builder.Services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
builder.Services.AddScoped<IStartGameUseCase, StartGameUseCase>();
builder.Services.AddScoped<IGameRepository, DynamoDBGameRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "お絵描き当てゲーム API",
        Version = "v1",
        Description = "お絵描き当てゲームのAPI仕様"
    });
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Development環境ではHTTPSリダイレクトを無効化
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }

using Amazon.DynamoDBv2;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Infrastructure.Repositories;

namespace EsiritoriApi.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // DynamoDB クライアントの設定
        services.AddSingleton<IAmazonDynamoDB>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var config = new AmazonDynamoDBConfig
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"] ?? "ap-northeast-1")
            };

            // Lambda環境でもLocalStackを使用する場合
            var serviceURL = configuration["DynamoDB:ServiceURL"] ?? Environment.GetEnvironmentVariable("DYNAMODB_ENDPOINT");
            if (!string.IsNullOrEmpty(serviceURL))
            {
                config.ServiceURL = serviceURL;
                config.UseHttp = true;
            }

            return new AmazonDynamoDBClient(config);
        });

        services.AddControllers();

        services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
        services.AddScoped<IStartGameUseCase, StartGameUseCase>();
        services.AddScoped<IGameRepository, DynamoDBGameRepository>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "お絵描き当てゲーム API",
                Version = "v1",
                Description = "お絵描き当てゲームのAPI仕様"
            });
        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors();

        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
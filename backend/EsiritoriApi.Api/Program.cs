using Amazon.DynamoDBv2;
using EsiritoriApi.Domain.Game;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Lambda hosting support
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// DynamoDB クライアントの設定  
builder.Services.AddSingleton<IAmazonDynamoDB>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var logger = provider.GetRequiredService<ILogger<Program>>();
    
    var region = configuration["AWS:Region"] ?? "ap-northeast-1";
    var serviceURL = configuration["DynamoDB:ServiceURL"];
    
    logger.LogInformation("DynamoDB設定: Region={Region}, ServiceURL={ServiceURL}, Environment={Environment}", 
        region, serviceURL, builder.Environment.EnvironmentName);
    
    // LocalStack使用時は環境変数でAWS設定を上書き
    if (builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(serviceURL))
    {
        Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_DYNAMODB", serviceURL);
        Environment.SetEnvironmentVariable("AWS_REGION", region);
        logger.LogInformation("LocalStack環境変数設定: AWS_ENDPOINT_URL_DYNAMODB={ServiceURL}, AWS_REGION={Region}", 
            serviceURL, region);
    }
    
    var config = new AmazonDynamoDBConfig
    {
        RegionEndpoint = Amazon.RegionEndpoint.APNortheast1
    };
    
    // Development環境でServiceURLが設定されている場合のみ上書き
    if (builder.Environment.IsDevelopment() && !string.IsNullOrEmpty(serviceURL))
    {
        config.ServiceURL = serviceURL;
        config.UseHttp = true;
        logger.LogInformation("LocalStack DynamoDB使用: {ServiceURL}", serviceURL);
    }
    else if (builder.Environment.IsDevelopment())
    {
        logger.LogWarning("DynamoDB:ServiceURL が設定されていません");
    }
    
    logger.LogInformation("最終的なDynamoDB設定: Region={Region}, ServiceURL={ServiceURL}, UseHttp={UseHttp}", 
        region, config.ServiceURL, config.UseHttp);

    var client = new AmazonDynamoDBClient(config);
    
    // クライアント作成後の設定確認
    logger.LogInformation("作成されたクライアントの設定: ServiceURL={ServiceURL}, RegionEndpoint={RegionEndpoint}", 
        client.Config.ServiceURL, client.Config.RegionEndpoint?.DisplayName);
    
    return client;
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

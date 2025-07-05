using EsiritoriApi.Application.Interfaces;
using EsiritoriApi.Application.UseCases;
using EsiritoriApi.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddControllers();

builder.Services.AddScoped<ICreateGameUseCase, CreateGameUseCase>();
builder.Services.AddScoped<IStartGameUseCase, StartGameUseCase>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }

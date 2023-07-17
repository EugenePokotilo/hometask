using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Common;
using Common.Models;
using Common.Networking;
using GameServer;
using GameServer.Configurations;
using GameServer.ConnectionManagement;
using GameServer.Handlers;
using GameServer.Middleware;
using GameServer.Repositories;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };
});
builder.Services.AddAuthorization();

builder.Services.AddHandlers();
builder.Services.AddSingleton<IPlayerConnectionManager, PlayerConnectionManager>();
builder.Services.AddScoped<IPlayerNotificator, PlayerNotificator>();
builder.Services.AddScoped<CurrentUserProvider>(); 
builder.Services.AddScoped<IPlayerRepository, InMemoryPlayerRepository>(); //as it's for test reasons it might be even a singleton
builder.Services.AddScoped<IResourceRepository, InMemoryResourceRepository>();

builder.Host.UseSerilog(
    
    (hostingContext, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
    
    );
//builder.Host.UseSerilog();

var app = builder.Build();

    
//HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//move access token from query to header to utilize out of the box token validation as ws connection will not add token in headers
app.Use(async (context, next) =>
{
    if (context.Request.Query.TryGetValue("access_token", out var accessToken) && accessToken is {})
    {
        context.Request.Headers["Authorization"] = $"Bearer {accessToken}";
    }

    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseRouting();

var wsOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(5) //todo: handle reconnection & close
};
app.UseWebSockets(wsOptions);
app.UseMiddleware<WebSocketConnectionMiddleware>();

app.Run();
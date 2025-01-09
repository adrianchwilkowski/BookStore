using BookStore;
using BookStore.Helper;
using BookStore.Services;
using Infrastructure;
using Infrastructure.Enums;
using Infrastructure.Migrations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.IncludeErrorDetails = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        RequireExpirationTime = true
    };
    x.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            Console.WriteLine(context.ToString());
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            Console.WriteLine(context.Exception);
            return Task.CompletedTask;
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            Console.WriteLine(context.ToString());
            return context.Response.CompleteAsync();
        }
    };
});
builder.Services.Configure<JwtConfiguration>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtInfo>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("UserPolicy", policy =>
    {
        policy.RequireRole(Roles.User.ToString());
    });
    options.AddPolicy("ManagerPolicy", policy =>
    {
        policy.RequireRole(Roles.Manager.ToString());
    });
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireRole(Roles.Admin.ToString());
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
 {
     {
           new OpenApiSecurityScheme
             {
                 Reference = new OpenApiReference
                 {
                     Type = ReferenceType.SecurityScheme,
                     Id = "Bearer"
                 }
             },
             new string[] {}
     }

});
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "BookStoreApi", Version = "v1" }); ;
});

builder.Services.RegisterDbContext(builder.Configuration);
builder.Services.RegisterRepositories();

builder.Services.AddTransient<IIdentityService, IdentityService>();
builder.Services.AddTransient<IBookSearchService, BookSearchService>();
builder.Services.AddTransient<IOrdersService, OrdersService>();

var app = builder.Build();

app.UseMiddleware<ExceptionsHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookStoreApi V1");
    });
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

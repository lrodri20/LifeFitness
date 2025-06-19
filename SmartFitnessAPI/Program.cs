using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SmartFitnessApi.Data;
using SmartFitnessApi.Data.Seeding;
using SmartFitnessApi.Models;
using SmartFitnessApi.Services;

namespace SmartFitnessApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            // Add services to the container.
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddCors(opts =>
            {
                opts.AddPolicy("AllowAll", policy =>
                {
                    policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SmartFitness API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT like:\n\nBearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            });

            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IMatchingService, MatchingService>();
            builder.Services.AddControllers();
            builder.Services.AddDbContext<SmartFitnessDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var jwtSection = builder.Configuration.GetSection("JwtSettings");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSection["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = jwtSection["Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async ctx =>
                    {
                        var db = ctx.HttpContext.RequestServices
                                    .GetRequiredService<SmartFitnessDbContext>();
                        var jti = ctx.Principal!.FindFirst(JwtRegisteredClaimNames.Jti)!.Value;

                        bool revoked = await db.RevokedTokens
                                            .AnyAsync(x => x.JwtId == jti);
                        if (revoked)
                        {
                            // immediately reject
                            ctx.Fail("Token has been revoked.");
                        }
                    }
                };
            });

            var app = builder.Build();



            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartFitness API V1");
                });
                await app.Services.InitializeDatabaseAsync();
            }

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            // Map controllers
            app.MapControllers();
            app.Run();
        }
    }
    public static class DbInitializerExtensions
    {
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SmartFitnessDbContext>();
            var authenticationService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
            await DbInitializer.SeedAsync(context, authenticationService);
        }
    }
}

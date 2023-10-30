using Application.Activities;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddDbContext<DataContext>(o => o.UseNpgsql(config.GetConnectionString("postgresql")));

        services.AddCors(o =>
        {
            o.AddPolicy("CorsPolicy", policy =>
            {
                policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:3000");
            });
        });

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(List.Handler).Assembly));
        return services;
    }
}
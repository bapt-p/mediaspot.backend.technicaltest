using Mediaspot.Application.Assets.Commands.Create.CreateVideoAsset;
using Mediaspot.Application.Common;
using Mediaspot.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mediaspot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databaseName)
    {
        services.AddDbContext<MediaspotDbContext>(o => o.UseInMemoryDatabase(databaseName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAssetRepository, AssetRepository>();
        services.AddScoped<ITranscodeJobRepository, TranscodeJobRepository>();
        services.AddScoped<ITitleRepository, TitleRepository>();

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateVideoAssetCommand).Assembly));

        return services;
    }
}
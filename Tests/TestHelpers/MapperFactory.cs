using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.TestHelpers;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfiles>());

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMapper>();
    }
}

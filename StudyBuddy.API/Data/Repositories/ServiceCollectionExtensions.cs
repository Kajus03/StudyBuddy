using StudyBuddy.API.Data.Repositories.BlockingRepository;
using StudyBuddy.API.Data.Repositories.ChatRepository;
using StudyBuddy.API.Data.Repositories.MatchRepository;
using StudyBuddy.API.Data.Repositories.SchedulingRepository;
using StudyBuddy.API.Data.Repositories.SystemBlockRepository;
using StudyBuddy.API.Data.Repositories.UserRepository;

namespace StudyBuddy.API.Data.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IBlockingRepository, BlockingRepository.BlockingRepository>();
        services.AddScoped<IUserRepository, UserRepository.UserRepository>();
        services.AddScoped<IMatchRepository, MatchRepository.MatchRepository>();
        services.AddScoped<IMatchRequestRepository, MatchRequestRepository>();
        services.AddScoped<IChatRepository, ChatRepository.ChatRepository>();
        services.AddScoped<ISchedulingRepository, SchedulingRepository.SchedulingRepository>();
        services.AddScoped<ISystemBlockRepository, SystemBlockRepository.SystemBlockRepository>();

        return services;
    }
}

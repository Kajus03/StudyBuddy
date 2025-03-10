using Microsoft.EntityFrameworkCore;
using StudyBuddy.API.Data;
using StudyBuddy.API.Data.Repositories;
using StudyBuddy.API.Models;
using StudyBuddy.API.Services;
using StudyBuddy.API.Services.UserService;
using Castle.DynamicProxy;
using StudyBuddy.API.Data.Repositories.SchedulingRepository;
using StudyBuddy.API.Interceptors;
using StudyBuddy.API.Services.SchedulingService;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        corsBuilder => corsBuilder.WithOrigins("http://localhost:8000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Register repositories
builder.Services.AddRepositories();

// Register services
builder.Services.AddServices();

var proxyGenerator = new ProxyGenerator();
var logFilePath = "/var/log/study_buddy.log";

builder.Services.AddScoped<ISchedulingService>(provider =>
{
    var schedulingRepository = provider.GetRequiredService<ISchedulingRepository>();
    var schedulingService = new SchedulingService(schedulingRepository);
    var interceptor = new LoggingInterceptor(logFilePath);
    return proxyGenerator.CreateInterfaceProxyWithTarget<ISchedulingService>(schedulingService, interceptor);
});

// Registering implementations for DI
builder.Services.AddDbContext<StudyBuddyDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigin");

app.UseAuthorization();

app.MapControllers();

using IServiceScope scope = app.Services.CreateScope();
IUserService userService = scope.ServiceProvider.GetRequiredService<IUserService>();
// await UserCounter.InitializeAsync(userService);

await app.RunAsync();

public partial class Program { }

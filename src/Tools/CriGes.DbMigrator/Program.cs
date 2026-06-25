using CriGes.Modules.Platform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Development";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddJsonFile(Path.Combine(FindRepositoryRoot(), "src", "Apps", "CriGes.Api", "appsettings.json"), optional: true)
    .AddJsonFile(Path.Combine(FindRepositoryRoot(), "src", "Apps", "CriGes.Api", $"appsettings.{environment}.json"), optional: true)
    .AddEnvironmentVariables()
    .AddCommandLine(args)
    .Build();

var connectionString = configuration.GetConnectionString("CriGes");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Connection string 'CriGes' is required.");
    return 2;
}

try
{
    var options = new DbContextOptionsBuilder<PlatformDbContext>()
        .UseSqlServer(connectionString)
        .Options;

    await using var dbContext = new PlatformDbContext(options);
    await dbContext.Database.MigrateAsync();

    Console.WriteLine("Database migrations applied successfully.");
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine("Database migration failed.");
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static string FindRepositoryRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);

    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "CriGes.sln")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return AppContext.BaseDirectory;
}

using System.Xml.Linq;

namespace CriGes.ArchitectureTests;

public sealed class ArchitectureRulesTests
{
    [Fact]
    public void ProjectReferencesMatchTheInitialArchitecturePlan()
    {
        var projects = LoadProjects();

        AssertReferences(
            projects["Platform.Domain"],
            required:
            [
                @"src\BuildingBlocks\CriGes.SharedKernel\CriGes.SharedKernel.csproj"
            ],
            forbidden:
            [
                "EntityFrameworkCore",
                "Infrastructure",
                "Api"
            ]);

        AssertReferences(
            projects["Platform.Application"],
            required:
            [
                @"src\Modules\Platform\CriGes.Modules.Platform.Domain\CriGes.Modules.Platform.Domain.csproj",
                @"src\Modules\Platform\CriGes.Modules.Platform.Contracts\CriGes.Modules.Platform.Contracts.csproj",
                @"src\BuildingBlocks\CriGes.Application.Abstractions\CriGes.Application.Abstractions.csproj",
                @"src\BuildingBlocks\CriGes.SharedKernel\CriGes.SharedKernel.csproj"
            ],
            forbidden:
            [
                "Platform.Infrastructure",
                "Platform.Api"
            ]);

        AssertReferences(
            projects["Platform.Infrastructure"],
            required:
            [
                @"src\Modules\Platform\CriGes.Modules.Platform.Application\CriGes.Modules.Platform.Application.csproj",
                @"src\Modules\Platform\CriGes.Modules.Platform.Domain\CriGes.Modules.Platform.Domain.csproj",
                @"src\Modules\Platform\CriGes.Modules.Platform.Contracts\CriGes.Modules.Platform.Contracts.csproj",
                @"src\BuildingBlocks\CriGes.Infrastructure\CriGes.Infrastructure.csproj"
            ],
            forbidden: []);

        AssertReferences(
            projects["Platform.Contracts"],
            required: [],
            forbidden:
            [
                "Platform.Domain",
                "Platform.Application",
                "EntityFrameworkCore"
            ]);

        AssertReferences(
            projects["Platform.Api"],
            required:
            [
                @"src\Modules\Platform\CriGes.Modules.Platform.Application\CriGes.Modules.Platform.Application.csproj",
                @"src\Modules\Platform\CriGes.Modules.Platform.Contracts\CriGes.Modules.Platform.Contracts.csproj",
                @"src\BuildingBlocks\CriGes.Contracts\CriGes.Contracts.csproj"
            ],
            forbidden:
            [
                "Platform.Infrastructure"
            ]);

        AssertReferences(
            projects["Desktop"],
            required:
            [
                @"src\Modules\Platform\CriGes.Modules.Platform.Contracts\CriGes.Modules.Platform.Contracts.csproj",
                @"src\BuildingBlocks\CriGes.Contracts\CriGes.Contracts.csproj"
            ],
            forbidden:
            [
                "Platform.Application",
                "Platform.Infrastructure",
                "Platform.Domain"
            ]);
    }

    private static Dictionary<string, ProjectInfo> LoadProjects()
    {
        var root = FindRepositoryRoot();

        return new(StringComparer.Ordinal)
        {
            ["Platform.Domain"] = LoadProject(root, @"src\Modules\Platform\CriGes.Modules.Platform.Domain\CriGes.Modules.Platform.Domain.csproj"),
            ["Platform.Application"] = LoadProject(root, @"src\Modules\Platform\CriGes.Modules.Platform.Application\CriGes.Modules.Platform.Application.csproj"),
            ["Platform.Infrastructure"] = LoadProject(root, @"src\Modules\Platform\CriGes.Modules.Platform.Infrastructure\CriGes.Modules.Platform.Infrastructure.csproj"),
            ["Platform.Contracts"] = LoadProject(root, @"src\Modules\Platform\CriGes.Modules.Platform.Contracts\CriGes.Modules.Platform.Contracts.csproj"),
            ["Platform.Api"] = LoadProject(root, @"src\Modules\Platform\CriGes.Modules.Platform.Api\CriGes.Modules.Platform.Api.csproj"),
            ["Desktop"] = LoadProject(root, @"src\Apps\CriGes.Desktop\CriGes.Desktop.csproj"),
        };
    }

    private static ProjectInfo LoadProject(string root, string relativePath)
    {
        var fullPath = Path.Combine(root, relativePath);
        var document = XDocument.Load(fullPath);
        var projectDirectory = Path.GetDirectoryName(fullPath) ?? root;

        var projectReferences = document
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeRelativeToRoot(root, Path.Combine(projectDirectory, value!)))
            .ToArray();

        var packageReferences = document
            .Descendants("PackageReference")
            .Select(element => element.Attribute("Include")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();

        return new ProjectInfo(projectReferences, packageReferences);
    }

    private static void AssertReferences(ProjectInfo project, string[] required, string[] forbidden)
    {
        foreach (var requiredReference in required)
        {
            Assert.Contains(NormalizePath(requiredReference), project.ProjectReferences);
        }

        var allReferences = project.ProjectReferences.Concat(project.PackageReferences).ToArray();

        foreach (var forbiddenFragment in forbidden)
        {
            Assert.DoesNotContain(allReferences, reference => reference.Contains(forbiddenFragment, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static string FindRepositoryRoot()
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

        throw new DirectoryNotFoundException("Could not find CriGes.sln from the test output directory.");
    }

    private static string NormalizeRelativeToRoot(string root, string path)
    {
        return NormalizePath(Path.GetRelativePath(root, Path.GetFullPath(path)));
    }

    private static string NormalizePath(string path)
    {
        return path.Replace('/', '\\');
    }

    private sealed record ProjectInfo(string[] ProjectReferences, string[] PackageReferences);
}

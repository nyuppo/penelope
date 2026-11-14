using System;
using System.IO;
using System.Linq;
using System.Text.Json;

public class ProjectDefinition
{
    public const string Extension = ".penelope";
    public const int CurrentFormatVersion = 1;

    public required string Name { get; set; }
    public int FormatVersion {get; set; } = CurrentFormatVersion;
}

public class Project
{
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public ProjectDefinition Definition { get; }

    public string FilePath { get; }
    public string RootPath => Path.GetDirectoryName(FilePath)!;
    public string Name => Definition.Name;

    Project(ProjectDefinition definition, string filePath)
    {
        Definition = definition;
        FilePath = Path.GetFullPath(filePath);
    }

    public static Project Load(string filePath)
    {
        var definition = JsonSerializer.Deserialize<ProjectDefinition>(File.ReadAllText(filePath)) ?? throw new InvalidDataException("The project file is empty.");

        if (definition.FormatVersion > ProjectDefinition.CurrentFormatVersion)
        {
            throw new InvalidDataException($"This project was created with a newer version of Penelope: attempted to load {definition.FormatVersion}, this version of Penelope supports up to {ProjectDefinition.CurrentFormatVersion}.");
        }

        // TODO: future improvement: migrate project if FormatVersion < CurrentFormatVersion

        return new Project(definition, filePath);
    }

    public static Project Create(string parentDir, string name)
    {
        var root = Path.Combine(parentDir, name);
        if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any())
        {
            throw new IOException($"The folder \"{root}\" already exists and isn't empty.");
        }

        Directory.CreateDirectory(root);

        var project = new Project(
            new ProjectDefinition { Name = name },
            Path.Combine(root, name + ProjectDefinition.Extension)
        );
        project.Save();
        return project;
    }

    public void Save() => File.WriteAllText(FilePath, JsonSerializer.Serialize(Definition, JsonOptions));

    public static bool IsLoadError(Exception e) => e is IOException or UnauthorizedAccessException or JsonException or InvalidDataException;
}
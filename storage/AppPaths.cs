#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class AppPaths
{
    const string AppName = "Penelope";

    public static string GlobalConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    public static string LocalPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppName);
    public static string DefaultAppRootPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);
}

public class GlobalConfig
{
    static string FilePath => Path.Combine(AppPaths.GlobalConfigPath, "config.json");
    static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string? ProjectsRoot { get; set; } // null = use default
    public List<string> RecentProjects { get; set; } = [];

    [JsonIgnore]
    public string EffectiveProjectsRoot => string.IsNullOrWhiteSpace(ProjectsRoot) ? AppPaths.DefaultAppRootPath : ProjectsRoot;

    public event Action? Changed;

    public static GlobalConfig Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                return JsonSerializer.Deserialize<GlobalConfig>(File.ReadAllText(FilePath)) ?? new();
            }
        } catch (JsonException)
        {
            // TODO: fall back to defaults
        }

        return new();
    }

    public void Save()
    {
        Directory.CreateDirectory(AppPaths.GlobalConfigPath);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
        Changed?.Invoke();
    }
}
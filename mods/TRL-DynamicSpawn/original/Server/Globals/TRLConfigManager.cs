using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using TRLDynamicSpawnServer.Models;

namespace TRLDynamicSpawnServer.Globals;

public enum ConfigOperationResult
{
    Success,
    Failure,
    ActiveProcess
}

public static class TRLConfigManager
{
    private static readonly string ConfigPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory, "config", "config.json");
    private static readonly string DefaultConfigPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? AppDomain.CurrentDomain.BaseDirectory, "config", "config.default.json");
    private static bool _isProcessing = false;

    private static readonly JsonSerializerOptions JsonOpts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<ConfigOperationResult> LoadConfig()
    {
        if (_isProcessing) return ConfigOperationResult.ActiveProcess;
        _isProcessing = true;

        // Load custom spawns coordinates
        TRLDynamicSpawnServer.Helpers.SpawnPointsManager.LoadSpawns();

        try
        {
            if (!File.Exists(DefaultConfigPath) && File.Exists(ConfigPath))
            {
                File.Copy(ConfigPath, DefaultConfigPath, overwrite: false);
            }

            if (File.Exists(ConfigPath))
            {
                var json = await File.ReadAllTextAsync(ConfigPath);
                var loaded = JsonSerializer.Deserialize<TRLConfig>(json, JsonOpts);
                if (loaded != null)
                {
                    TRLDynamicSpawnServer.Routers.TRLRouters.CurrentConfig = loaded;
                }
            }
            else
            {
                // Create default config if missing
                await SaveConfigInner();
            }
            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TRL-DynamicSpawn] Error loading config: {ex.Message}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public static async Task<ConfigOperationResult> ResetConfig()
    {
        if (_isProcessing) return ConfigOperationResult.ActiveProcess;
        _isProcessing = true;

        try
        {
            if (File.Exists(DefaultConfigPath))
            {
                var json = await File.ReadAllTextAsync(DefaultConfigPath);
                var loaded = JsonSerializer.Deserialize<TRLConfig>(json, JsonOpts);
                if (loaded != null)
                {
                    TRLDynamicSpawnServer.Routers.TRLRouters.CurrentConfig = loaded;
                }
                else
                {
                    TRLDynamicSpawnServer.Routers.TRLRouters.CurrentConfig = new TRLConfig();
                }
            }
            else
            {
                TRLDynamicSpawnServer.Routers.TRLRouters.CurrentConfig = new TRLConfig();
            }

            await SaveConfigInner();
            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TRL-DynamicSpawn] Error resetting config: {ex.Message}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    public static async Task<ConfigOperationResult> SaveConfig()
    {
        if (_isProcessing) return ConfigOperationResult.ActiveProcess;
        _isProcessing = true;

        try
        {
            await SaveConfigInner();
            return ConfigOperationResult.Success;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TRL-DynamicSpawn] Error saving config: {ex.Message}");
            return ConfigOperationResult.Failure;
        }
        finally
        {
            _isProcessing = false;
        }
    }

    private static async Task SaveConfigInner()
    {
        var directory = Path.GetDirectoryName(ConfigPath);
        if (!Directory.Exists(directory) && directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(TRLDynamicSpawnServer.Routers.TRLRouters.CurrentConfig, JsonOpts);
        await File.WriteAllTextAsync(ConfigPath, json);
    }
}

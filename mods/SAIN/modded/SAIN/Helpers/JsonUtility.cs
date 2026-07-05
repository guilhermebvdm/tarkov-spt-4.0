using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SAIN.Preset;
using SAIN.Preset.GearStealthValues;

namespace SAIN.Helpers;

public enum JsonEnum
{
    Presets,
    GlobalSettings,
}

public static class JsonUtility
{
    public static readonly Dictionary<JsonEnum, string> FileAndFolderNames = new()
    {
        { JsonEnum.Presets, "Presets" },
        { JsonEnum.GlobalSettings, "GlobalSettings" },
    };

    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        Converters = { new StringEnumConverter() },
        Formatting = Formatting.Indented,
    };

    public const string PresetsFolder = "Presets";
    public const string JsonExtension = ".json";
    public const string Info = "Info";

    public static void SaveObjectToJson(object objectToSave, string fileName, params string[] folders)
    {
        if (objectToSave == null)
        {
            return;
        }

        try
        {
            if (!GetFoldersPath(out string foldersPath, folders))
            {
                Directory.CreateDirectory(foldersPath);
            }

            var fullPath = Path.Combine(foldersPath, fileName);
            fullPath = Path.ChangeExtension(fullPath, JsonExtension);

            File.WriteAllText(fullPath, JsonConvert.SerializeObject(objectToSave, JsonSerializerSettings));
        }
        catch (Exception e)
        {
            Logger.LogError(e);
        }
    }

    public static bool DoesFileExist(string fileName, params string[] folders)
    {
        if (!GetFoldersPath(out string foldersPath, folders))
        {
            return false;
        }
        string filePath = Path.Combine(foldersPath, fileName);
        filePath = Path.ChangeExtension(filePath, JsonExtension);
        return File.Exists(filePath);
    }

    public static class Load
    {
        public static void LoadCustomPresetOptions(List<SAINPresetDefinition> list)
        {
            list.Clear();
            if (!GetFoldersPath(out string foldersPath, PresetsFolder))
            {
                Directory.CreateDirectory(foldersPath);
            }
            var array = Directory.GetDirectories(foldersPath);
            foreach (var item in array)
            {
                string path = Path.Combine(item, Info + JsonExtension);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    var obj = DeserializeObject<SAINPresetDefinition>(json);
                    if (obj.IsCustom)
                    {
                        list.Add(obj);
                    }
                }
                else
                {
                    Logger.LogError($"Could not Import Info.json at path [{path}]. Is the file missing?");
                }
            }
        }

        public static void LoadStealthValues(List<ItemStealthValue> list, params string[] folders)
        {
            if (!GetFoldersPath(out string foldersPath, folders))
            {
                return;
            }
            foreach (var file in Directory.GetFiles(foldersPath, "*.json"))
            {
                string jsonContent = File.ReadAllText(file);
                list.Add(JsonConvert.DeserializeObject<ItemStealthValue>(jsonContent));
            }
        }

        public static T DeserializeObject<T>(string file)
        {
            return JsonConvert.DeserializeObject<T>(file);
        }

        public static string LoadTextFile(string fileExtension, string fileName, params string[] folders)
        {
            if (GetFoldersPath(out string foldersPath, folders))
            {
                string filePath = Path.Combine(foldersPath, fileName);

                filePath += fileExtension;

                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath);
                }
            }
            return null;
        }

        public static bool LoadJsonFile(out string json, string fileName, params string[] folders)
        {
            json = LoadTextFile(JsonExtension, fileName, folders);
            return json != null;
        }

        public static bool LoadObject<T>(out T obj, string fileName, params string[] folders)
        {
            try
            {
                string json = LoadTextFile(JsonExtension, fileName, folders);
                if (json != null)
                {
                    obj = DeserializeObject<T>(json);
                    return true;
                }
            }
            catch (JsonSerializationException) { }
            obj = default;
            return false;
        }
    }

    public static void DeletePreset(SAINPresetDefinition preset)
    {
        var path = GetPath("Presets", preset.Name);
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    private static void CheckCreateFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void CreateFolder(params string[] subFolders)
    {
        string path = GetPath(subFolders);
        CheckCreateFolder(path);
    }

    public static bool DoesFolderExist(params string[] subFolders)
    {
        string path = GetPath(subFolders);
        return Directory.Exists(path);
    }

    public static bool GetFoldersPath(out string path, params string[] folders)
    {
        path = GetPath(folders);
        return Directory.Exists(path);
    }

    private static string GetPath(params string[] folders)
    {
        string path = GetSAINPluginPath();
        for (int i = 0; i < folders.Length; i++)
        {
            path = Path.Combine(path, folders[i]);
        }
        return path;
    }

    public static string GetSAINPluginPath()
    {
        string pluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //var path = Path.Combine(pluginFolder, nameof(SAIN));
        CheckCreateFolder(pluginFolder);
        return pluginFolder;
    }
}

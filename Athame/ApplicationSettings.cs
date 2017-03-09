﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Athame.PluginAPI.Service;
using Newtonsoft.Json;

namespace Athame
{
    public enum AlbumArtworkSaveFormat
    {
        DontSave,
        AsCover,
        AsArtistAlbum
    }

    public class ApplicationSettings
    {
        private const string SettingsFilename = "settings.json";

        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Athame");

        public static readonly string SettingsPath = Path.Combine(SettingsDirectory, SettingsFilename);

        // Need this so service settings can be deserialised across services and assemblies
        private static readonly JsonSerializerSettings SerializerSettings;
        static ApplicationSettings()
        {
            SerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Formatting.Indented
            };
        }

        private bool ignoreSave;
        private string settingsPath;

        private static ApplicationSettings _default;

        public static ApplicationSettings Default
        {
            get
            {
                if (_default == null)
                {
                    Directory.CreateDirectory(SettingsDirectory);
                    if (!File.Exists(SettingsPath))
                    {
                        _default = new ApplicationSettings(SettingsPath);
                        _default.Save();
                    }
                    else
                    {
                        try
                        {
                            // Assign settings path to deserialised settings instance
                            _default = JsonConvert.DeserializeObject<ApplicationSettings>(
                                File.ReadAllText(SettingsPath), SerializerSettings);
                            _default.settingsPath = SettingsPath;
                        }
                        catch (JsonSerializationException)
                        {
                            MessageBox.Show("");
                            _default = new ApplicationSettings(SettingsPath);
                            _default.Save();
                        }
                    }
                }
                return _default;
            }
        }

        public void Save()
        {
            if (ignoreSave) return;
            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(this, SerializerSettings));
        }

        public void Clear()
        {
            ignoreSave = true;
            File.WriteAllText(settingsPath, JsonConvert.SerializeObject(new ApplicationSettings(null), SerializerSettings));
        }

        public ApplicationSettings(string settingsPath)
        {
            this.settingsPath = settingsPath;
            ServiceSettings = new Dictionary<string, StoredSettings>();
            SaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            TrackFilenameFormat = "{AlbumArtistOrArtist} - {Album.Title}/{TrackNumber} {Title}";
            AlbumArtworkSaveFormat = AlbumArtworkSaveFormat.AsCover;
        }

        public Dictionary<string, StoredSettings> ServiceSettings { get; set; }
        public string SaveLocation { get; set; }
        public string TrackFilenameFormat { get; set; }
        public AlbumArtworkSaveFormat AlbumArtworkSaveFormat { get; set; }
        
    }
}

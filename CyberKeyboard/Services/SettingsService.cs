using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using CyberKeyboard.Graphics;
using CyberKeyboard.Models;

namespace CyberKeyboard.Services
{
    public class SettingsService
    {
        private static SettingsService? _instance;
        public static SettingsService Instance => _instance ??= new SettingsService();

        private readonly string _storageDir;
        private readonly string _activeSettingsPath;
        private readonly string _profilesPath;

        public ObservableCollection<KeyboardProfile> Profiles { get; } = new();
        public KeyboardProfile ActiveSettings { get; private set; } = new();

        public event Action<KeyboardProfile>? ProfileApplied;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public SettingsService()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _storageDir = Path.Combine(appData, "CyberKeyboard");
            try
            {
                if (!Directory.Exists(_storageDir))
                {
                    Directory.CreateDirectory(_storageDir);
                }
            }
            catch
            {
                _storageDir = AppDomain.CurrentDomain.BaseDirectory;
            }

            _activeSettingsPath = Path.Combine(_storageDir, "active_settings.json");
            _profilesPath = Path.Combine(_storageDir, "profiles.json");

            LoadProfiles();
            LoadActiveSettings();
        }

        public void LoadActiveSettings()
        {
            try
            {
                if (File.Exists(_activeSettingsPath))
                {
                    string json = File.ReadAllText(_activeSettingsPath);
                    var loaded = JsonSerializer.Deserialize<KeyboardProfile>(json, JsonOptions);
                    if (loaded != null)
                    {
                        ActiveSettings = loaded;
                        return;
                    }
                }
            }
            catch
            {
                // Fallback to default
            }

            ActiveSettings = CreateDefaultProfile("Aktif Profil");
        }

        public void SaveActiveSettings(KeyboardProfile profile)
        {
            try
            {
                profile.LastModified = DateTime.Now;
                ActiveSettings = profile;
                string json = JsonSerializer.Serialize(profile, JsonOptions);
                File.WriteAllText(_activeSettingsPath, json);
            }
            catch
            {
                // Silent fail
            }
        }

        public void LoadProfiles()
        {
            Profiles.Clear();
            bool loaded = false;

            try
            {
                if (File.Exists(_profilesPath))
                {
                    string json = File.ReadAllText(_profilesPath);
                    var list = JsonSerializer.Deserialize<List<KeyboardProfile>>(json, JsonOptions);
                    if (list != null && list.Count > 0)
                    {
                        foreach (var p in list)
                        {
                            Profiles.Add(p);
                        }
                        loaded = true;
                    }
                }
            }
            catch
            {
                // Fallback to defaults
            }

            if (!loaded || Profiles.Count == 0)
            {
                CreateDefaultPresets();
                SaveProfiles();
            }
        }

        public void SaveProfiles()
        {
            try
            {
                string json = JsonSerializer.Serialize(Profiles.ToList(), JsonOptions);
                File.WriteAllText(_profilesPath, json);
            }
            catch
            {
                // Silent fail
            }
        }

        public void SaveProfile(string name, KeyboardProfile currentValues)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "Özel Profil";
            name = name.Trim();

            var existing = Profiles.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.LastModified = DateTime.Now;
                existing.ScreenIndex = currentValues.ScreenIndex;
                existing.ScreenDeviceName = currentValues.ScreenDeviceName;
                existing.IsFullscreen = currentValues.IsFullscreen;
                existing.RowHeight = currentValues.RowHeight;
                existing.IsDockBottom = currentValues.IsDockBottom;
                existing.IsNumpadVisible = currentValues.IsNumpadVisible;
                existing.IsPlayerVisible = currentValues.IsPlayerVisible;
                existing.WindowWidth = currentValues.WindowWidth;
                existing.WindowHeight = currentValues.WindowHeight;
                existing.WindowLeft = currentValues.WindowLeft;
                existing.WindowTop = currentValues.WindowTop;
                existing.Theme = currentValues.Theme;
                existing.UiLanguage = currentValues.UiLanguage;
                existing.TypingLanguage = currentValues.TypingLanguage;
                existing.IsSoundEnabled = currentValues.IsSoundEnabled;
                existing.Volume = currentValues.Volume;
                existing.IsShuffle = currentValues.IsShuffle;
                existing.IsRepeat = currentValues.IsRepeat;
                existing.PlaylistFiles = new List<string>(currentValues.PlaylistFiles);
            }
            else
            {
                var newProfile = currentValues.Clone(name);
                newProfile.IsBuiltIn = false;
                Profiles.Add(newProfile);
            }

            SaveProfiles();
        }

        public void DeleteProfile(KeyboardProfile profile)
        {
            if (profile != null)
            {
                Profiles.Remove(profile);
                SaveProfiles();
            }
        }

        public void ApplyProfile(KeyboardProfile profile)
        {
            if (profile == null) return;
            SaveActiveSettings(profile);
            ProfileApplied?.Invoke(profile);
        }

        private void CreateDefaultPresets()
        {
            Profiles.Add(new KeyboardProfile
            {
                Name = "📱 Dokunmatik Monitör (Tam Ekran)",
                IsBuiltIn = true,
                RowHeight = 94.0,
                IsFullscreen = true,
                IsNumpadVisible = true,
                IsPlayerVisible = true,
                IsDockBottom = true,
                Theme = CyberTheme.CyberCyan,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.8
            });

            Profiles.Add(new KeyboardProfile
            {
                Name = "🌆 Synthwave Gece Modu",
                IsBuiltIn = true,
                RowHeight = 82.0,
                IsFullscreen = false,
                IsNumpadVisible = true,
                IsPlayerVisible = true,
                Theme = CyberTheme.SynthwavePink,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.75
            });

            Profiles.Add(new KeyboardProfile
            {
                Name = "💻 Matrix Terminal",
                IsBuiltIn = true,
                RowHeight = 68.0,
                IsFullscreen = false,
                IsNumpadVisible = false,
                IsPlayerVisible = false,
                Theme = CyberTheme.MatrixGreen,
                UiLanguage = UiLanguage.English,
                TypingLanguage = SupportedLanguage.English,
                IsSoundEnabled = true,
                Volume = 0.5
            });

            Profiles.Add(new KeyboardProfile
            {
                Name = "📻 Retro Mekanik",
                IsBuiltIn = true,
                RowHeight = 68.0,
                IsFullscreen = false,
                IsNumpadVisible = true,
                IsPlayerVisible = false,
                Theme = CyberTheme.Retro,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.7
            });

            Profiles.Add(new KeyboardProfile
            {
                Name = "⚡ Retro-Futurist Glitch",
                IsBuiltIn = true,
                RowHeight = 82.0,
                IsFullscreen = false,
                IsNumpadVisible = true,
                IsPlayerVisible = true,
                IsDockBottom = true,
                Theme = CyberTheme.RetroFuturist,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.8
            });

            Profiles.Add(new KeyboardProfile
            {
                Name = "🌴 80's Chill Neon",
                IsBuiltIn = true,
                RowHeight = 82.0,
                IsFullscreen = false,
                IsNumpadVisible = true,
                IsPlayerVisible = true,
                IsDockBottom = true,
                Theme = CyberTheme.RetroFuturist80s,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.75
            });
        }

        public static KeyboardProfile CreateDefaultProfile(string name)
        {
            return new KeyboardProfile
            {
                Name = name,
                RowHeight = 68.0,
                IsFullscreen = false,
                IsNumpadVisible = true,
                IsPlayerVisible = false,
                IsDockBottom = true,
                Theme = CyberTheme.CyberCyan,
                UiLanguage = UiLanguage.Turkish,
                TypingLanguage = SupportedLanguage.Turkish,
                IsSoundEnabled = true,
                Volume = 0.75
            };
        }
    }
}

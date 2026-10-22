using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyberKeyboard.Services
{
    public enum UiLanguage
    {
        Turkish,
        English,
        French,
        German
    }

    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        public static LocalizationService Instance => _instance ??= new LocalizationService();

        private UiLanguage _currentLanguage = UiLanguage.Turkish;
        public UiLanguage CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LanguageCode));
                    LanguageChanged?.Invoke(value);
                }
            }
        }

        public string LanguageCode => CurrentLanguage switch
        {
            UiLanguage.Turkish => "TR",
            UiLanguage.English => "EN",
            UiLanguage.French => "FR",
            UiLanguage.German => "DE",
            _ => "TR"
        };

        public event Action<UiLanguage>? LanguageChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public void CycleLanguage()
        {
            CurrentLanguage = CurrentLanguage switch
            {
                UiLanguage.Turkish => UiLanguage.English,
                UiLanguage.English => UiLanguage.French,
                UiLanguage.French => UiLanguage.German,
                UiLanguage.German => UiLanguage.Turkish,
                _ => UiLanguage.Turkish
            };
        }

        public string Get(string key)
        {
            if (Translations.TryGetValue(key, out var dict))
            {
                if (dict.TryGetValue(CurrentLanguage, out var val))
                {
                    return val;
                }
            }
            return key;
        }

        private static readonly Dictionary<string, Dictionary<UiLanguage, string>> Translations = new()
        {
            // HUD Toolbar Quick Actions
            ["Cut"] = new()
            {
                [UiLanguage.Turkish] = "✂ KES",
                [UiLanguage.English] = "✂ CUT",
                [UiLanguage.French] = "✂ COUPER",
                [UiLanguage.German] = "✂ AUSSCHN."
            },
            ["Copy"] = new()
            {
                [UiLanguage.Turkish] = "📋 KOPYALA",
                [UiLanguage.English] = "📋 COPY",
                [UiLanguage.French] = "📋 COPIER",
                [UiLanguage.German] = "📋 KOPIEREN"
            },
            ["Paste"] = new()
            {
                [UiLanguage.Turkish] = "📄 YAPIŞTIR",
                [UiLanguage.English] = "📄 PASTE",
                [UiLanguage.French] = "📄 COLLER",
                [UiLanguage.German] = "📄 EINFÜGEN"
            },
            ["SelectAll"] = new()
            {
                [UiLanguage.Turkish] = "🔍 TÜMÜ",
                [UiLanguage.English] = "🔍 ALL",
                [UiLanguage.French] = "🔍 TOUT",
                [UiLanguage.German] = "🔍 ALLES"
            },
            ["Undo"] = new()
            {
                [UiLanguage.Turkish] = "↺ GERİ AL",
                [UiLanguage.English] = "↺ UNDO",
                [UiLanguage.French] = "↺ ANNULER",
                [UiLanguage.German] = "↺ RÜCKG."
            },

            // HUD Toolbar Right Actions
            ["Screen"] = new()
            {
                [UiLanguage.Turkish] = "🖥️ EKRAN",
                [UiLanguage.English] = "🖥️ SCREEN",
                [UiLanguage.French] = "🖥️ ÉCRAN",
                [UiLanguage.German] = "🖥️ BILDSCHIRM"
            },
            ["Player"] = new()
            {
                [UiLanguage.Turkish] = "🎵 ÇALAR",
                [UiLanguage.English] = "🎵 PLAYER",
                [UiLanguage.French] = "🎵 LECTEUR",
                [UiLanguage.German] = "🎵 SPIELER"
            },
            ["Size"] = new()
            {
                [UiLanguage.Turkish] = "↕ BOYUT:",
                [UiLanguage.English] = "↕ SIZE:",
                [UiLanguage.French] = "↕ TAILLE:",
                [UiLanguage.German] = "↕ GRÖSSE:"
            },
            ["TypingLang"] = new()
            {
                [UiLanguage.Turkish] = "⌨️ DİL:",
                [UiLanguage.English] = "⌨️ LANG:",
                [UiLanguage.French] = "⌨️ CLAVIER:",
                [UiLanguage.German] = "⌨️ TAST:"
            },
            ["Fullscreen"] = new()
            {
                [UiLanguage.Turkish] = "⛶ TAM EKRAN",
                [UiLanguage.English] = "⛶ FULLSCREEN",
                [UiLanguage.French] = "⛶ PLEIN ÉCRAN",
                [UiLanguage.German] = "⛶ VOLLBILD"
            },
            ["Restore"] = new()
            {
                [UiLanguage.Turkish] = "❐ PENCERE",
                [UiLanguage.English] = "❐ RESTORE",
                [UiLanguage.French] = "❐ RESTAURER",
                [UiLanguage.German] = "❐ FENSTER"
            },
            ["Numpad"] = new()
            {
                [UiLanguage.Turkish] = "🔢 NUMPAD",
                [UiLanguage.English] = "🔢 NUMPAD",
                [UiLanguage.French] = "🔢 PAVÉ NUM",
                [UiLanguage.German] = "🔢 ZIFFERN"
            },
            ["Sound"] = new()
            {
                [UiLanguage.Turkish] = "SES",
                [UiLanguage.English] = "SFX",
                [UiLanguage.French] = "SON",
                [UiLanguage.German] = "TON"
            },
            ["TargetDetecting"] = new()
            {
                [UiLanguage.Turkish] = "HEDEF: ALGILANIYOR...",
                [UiLanguage.English] = "TARGET: DETECTING...",
                [UiLanguage.French] = "CIBLE: DÉTECTION...",
                [UiLanguage.German] = "ZIEL: ERKENNUNG..."
            },

            // Audio Player Deck
            ["NoTrackLoaded"] = new()
            {
                [UiLanguage.Turkish] = "PARÇA YÜKLENMEDİ — SES DOSYASI SÜRÜKLEYİN",
                [UiLanguage.English] = "NO TRACK LOADED — DRAG & DROP AUDIO",
                [UiLanguage.French] = "AUCUNE PISTE — GLISSER-DÉPOSER AUDIO",
                [UiLanguage.German] = "KEIN TITEL GELADEN — AUDIO HIERHER ZIEHEN"
            },
            ["LiveStreamReady"] = new()
            {
                [UiLanguage.Turkish] = "CYBER_DECK HAZIR — İSTEDİĞİNİZ YERE YAZIN",
                [UiLanguage.English] = "CYBER_DECK READY — TYPE ANYWHERE",
                [UiLanguage.French] = "CYBER_DECK PRÊT — ÉCRIVEZ PARTOUT",
                [UiLanguage.German] = "CYBER_DECK BEREIT — ÜBERALL TIPPEN"
            },
            ["Keys"] = new()
            {
                [UiLanguage.Turkish] = "TUŞ:",
                [UiLanguage.English] = "KEYS:",
                [UiLanguage.French] = "TOUCHES:",
                [UiLanguage.German] = "TASTEN:"
            },

            // Playlist / File Tree Tabs & Action Bar
            ["PlaylistTab"] = new()
            {
                [UiLanguage.Turkish] = "📑 LİSTE",
                [UiLanguage.English] = "📑 PLAYLIST",
                [UiLanguage.French] = "📑 LISTE",
                [UiLanguage.German] = "📑 LISTE"
            },
            ["TreeTab"] = new()
            {
                [UiLanguage.Turkish] = "📁 DOSYALAR",
                [UiLanguage.English] = "📁 FILES",
                [UiLanguage.French] = "📁 FICHIERS",
                [UiLanguage.German] = "📁 DATEIEN"
            },
            ["DragHint"] = new()
            {
                [UiLanguage.Turkish] = "⬇️ Sürükleyin",
                [UiLanguage.English] = "⬇️ Drag & Drop",
                [UiLanguage.French] = "⬇️ Glisser-Déposer",
                [UiLanguage.German] = "⬇️ Ablegen"
            },
            ["EmptyPlaylistCue"] = new()
            {
                [UiLanguage.Turkish] = "Ses dosyalarını (.mp3, .wav, .flac) buraya sürükleyin",
                [UiLanguage.English] = "Drag audio files (.mp3, .wav, .flac) here",
                [UiLanguage.French] = "Glissez des fichiers audio (.mp3, .wav, .flac) ici",
                [UiLanguage.German] = "Audiodateien (.mp3, .wav, .flac) hierher ziehen"
            },
            ["AddFile"] = new()
            {
                [UiLanguage.Turkish] = "➕ DOSYA",
                [UiLanguage.English] = "➕ FILE",
                [UiLanguage.French] = "➕ FICHIER",
                [UiLanguage.German] = "➕ DATEI"
            },
            ["AddFolder"] = new()
            {
                [UiLanguage.Turkish] = "📂 KLASÖR",
                [UiLanguage.English] = "📂 FOLDER",
                [UiLanguage.French] = "📂 DOSSIER",
                [UiLanguage.German] = "📂 ORDNER"
            },
            ["ClearPlaylist"] = new()
            {
                [UiLanguage.Turkish] = "🗑 TEMİZLE",
                [UiLanguage.English] = "🗑 CLEAR",
                [UiLanguage.French] = "🗑 EFFACER",
                [UiLanguage.German] = "🗑 LEEREN"
            },
            ["Play"] = new()
            {
                [UiLanguage.Turkish] = "▶ OYNAT",
                [UiLanguage.English] = "▶ PLAY",
                [UiLanguage.French] = "▶ JOUER",
                [UiLanguage.German] = "▶ ABSPIELEN"
            },
            ["Add"] = new()
            {
                [UiLanguage.Turkish] = "➕ EKLE",
                [UiLanguage.English] = "➕ ADD",
                [UiLanguage.French] = "➕ AJOUTER",
                [UiLanguage.German] = "➕ HINZUFÜGEN"
            },
            ["ClockSizeSmall"] = new()
            {
                [UiLanguage.Turkish] = "Küçük",
                [UiLanguage.English] = "Small",
                [UiLanguage.French] = "Petit",
                [UiLanguage.German] = "Klein"
            },
            ["ClockSizeMedium"] = new()
            {
                [UiLanguage.Turkish] = "Orta",
                [UiLanguage.English] = "Medium",
                [UiLanguage.French] = "Moyen",
                [UiLanguage.German] = "Mittel"
            },
            ["ClockSizeLarge"] = new()
            {
                [UiLanguage.Turkish] = "Büyük",
                [UiLanguage.English] = "Large",
                [UiLanguage.French] = "Grand",
                [UiLanguage.German] = "Groß"
            },
            ["ClockSizeExtraLarge"] = new()
            {
                [UiLanguage.Turkish] = "Ekstra Büyük",
                [UiLanguage.English] = "Extra Large",
                [UiLanguage.French] = "Très Grand",
                [UiLanguage.German] = "Sehr Groß"
            },
            ["CalendarPrevMonth"] = new()
            {
                [UiLanguage.Turkish] = "Önceki Ay",
                [UiLanguage.English] = "Previous Month",
                [UiLanguage.French] = "Mois Précédent",
                [UiLanguage.German] = "Vorheriger Monat"
            },
            ["CalendarNextMonth"] = new()
            {
                [UiLanguage.Turkish] = "Sonraki Ay",
                [UiLanguage.English] = "Next Month",
                [UiLanguage.French] = "Mois Suivant",
                [UiLanguage.German] = "Nächster Monat"
            },
            ["CalendarToday"] = new()
            {
                [UiLanguage.Turkish] = "Bugün",
                [UiLanguage.English] = "Today",
                [UiLanguage.French] = "Aujourd'hui",
                [UiLanguage.German] = "Heute"
            },

            // Tree View Quick Roots
            ["MyMusic"] = new()
            {
                [UiLanguage.Turkish] = "🎵 Müziğim (Müzik Klasörü)",
                [UiLanguage.English] = "🎵 My Music (Audio Folder)",
                [UiLanguage.French] = "🎵 Ma Musique (Dossier Audio)",
                [UiLanguage.German] = "🎵 Eigene Musik (Audio Ordner)"
            },
            ["Downloads"] = new()
            {
                [UiLanguage.Turkish] = "📥 İndirilenler (Downloads)",
                [UiLanguage.English] = "📥 Downloads",
                [UiLanguage.French] = "📥 Téléchargements",
                [UiLanguage.German] = "📥 Downloads"
            },
            ["Desktop"] = new()
            {
                [UiLanguage.Turkish] = "🖥️ Masaüstü (Desktop)",
                [UiLanguage.English] = "🖥️ Desktop",
                [UiLanguage.French] = "🖥️ Bureau",
                [UiLanguage.German] = "🖥️ Desktop"
            },
            ["LocalDisk"] = new()
            {
                [UiLanguage.Turkish] = "Yerel Disk",
                [UiLanguage.English] = "Local Disk",
                [UiLanguage.French] = "Disque Local",
                [UiLanguage.German] = "Lokaler Datenträger"
            },

            // Digital Clock & Calendar HUD
            ["Week"] = new()
            {
                [UiLanguage.Turkish] = "HAFTA",
                [UiLanguage.English] = "WEEK",
                [UiLanguage.French] = "SEMAINE",
                [UiLanguage.German] = "WOCHE"
            },
            ["Day"] = new()
            {
                [UiLanguage.Turkish] = "GÜN",
                [UiLanguage.English] = "DAY",
                [UiLanguage.French] = "JOUR",
                [UiLanguage.German] = "TAG"
            },
            ["Uptime"] = new()
            {
                [UiLanguage.Turkish] = "ÇALIŞMA",
                [UiLanguage.English] = "UPTIME",
                [UiLanguage.French] = "SESSION",
                [UiLanguage.German] = "LAUFZEIT"
            },
            ["LocalTime"] = new()
            {
                [UiLanguage.Turkish] = "YEREL SAAT",
                [UiLanguage.English] = "LOCAL TIME",
                [UiLanguage.French] = "HEURE LOCALE",
                [UiLanguage.German] = "LOKALE ZEIT"
            },
            ["Target"] = new()
            {
                [UiLanguage.Turkish] = "HEDEF",
                [UiLanguage.English] = "TARGET",
                [UiLanguage.French] = "CIBLE",
                [UiLanguage.German] = "ZIEL"
            },
            ["DockBottom"] = new()
            {
                [UiLanguage.Turkish] = "⇕ HİZALA: ALT",
                [UiLanguage.English] = "⇕ DOCK: BOTTOM",
                [UiLanguage.French] = "⇕ ANCRER: BAS",
                [UiLanguage.German] = "⇕ DOCK: UNTEN"
            },
            ["DockCenter"] = new()
            {
                [UiLanguage.Turkish] = "⇕ HİZALA: ORTA",
                [UiLanguage.English] = "⇕ DOCK: CENTER",
                [UiLanguage.French] = "⇕ ANCRER: CENTRE",
                [UiLanguage.German] = "⇕ DOCK: MITTE"
            },

            // Monitor Selector Dialog
            ["MonitorSelectorTitle"] = new()
            {
                [UiLanguage.Turkish] = "🖥️ MONİTÖR SEÇİMİ // EKRAN DEĞİŞTİR",
                [UiLanguage.English] = "🖥️ MONITOR SELECTOR // DISPLAY MATRIX",
                [UiLanguage.French] = "🖥️ SÉLECTION DU MONITEUR // MATRICE",
                [UiLanguage.German] = "🖥️ BILDSCHIRMAUSWAHL // DISPLAY-MATRIX"
            },
            ["MonitorSelectorSubtitle"] = new()
            {
                [UiLanguage.Turkish] = "Klavyeyi taşımak istediğiniz monitöre dokunun:",
                [UiLanguage.English] = "Tap the monitor you want to move the keyboard to:",
                [UiLanguage.French] = "Touchez l'écran vers lequel déplacer le clavier :",
                [UiLanguage.German] = "Tippen Sie auf den Bildschirm, zu dem verschoben werden soll:"
            },
            ["PrimaryBadge"] = new()
            {
                [UiLanguage.Turkish] = "BİRİNCİL",
                [UiLanguage.English] = "PRIMARY",
                [UiLanguage.French] = "PRINCIPAL",
                [UiLanguage.German] = "PRIMÄR"
            },
            ["ExternalTouchBadge"] = new()
            {
                [UiLanguage.Turkish] = "HARİCİ / DOKUNMATİK",
                [UiLanguage.English] = "EXTERNAL / TOUCH",
                [UiLanguage.French] = "EXTERNE / TACTILE",
                [UiLanguage.German] = "EXTERN / TOUCH"
            },
            ["ActiveBadge"] = new()
            {
                [UiLanguage.Turkish] = "AKTİF",
                [UiLanguage.English] = "ACTIVE",
                [UiLanguage.French] = "ACTIF",
                [UiLanguage.German] = "AKTIV"
            },
            ["RefreshDisplays"] = new()
            {
                [UiLanguage.Turkish] = "🔄 YENİLE",
                [UiLanguage.English] = "🔄 REFRESH",
                [UiLanguage.French] = "🔄 ACTUALISER",
                [UiLanguage.German] = "🔄 AKTUALISIEREN"
            },

            // Profile & Settings Manager
            ["Profiles"] = new()
            {
                [UiLanguage.Turkish] = "⚙️ PROFİL",
                [UiLanguage.English] = "⚙️ PROFILES",
                [UiLanguage.French] = "⚙️ PROFILS",
                [UiLanguage.German] = "⚙️ PROFILE"
            },
            ["ProfileManagerTitle"] = new()
            {
                [UiLanguage.Turkish] = "⚙️ AYARLAR & PROFİL YÖNETİCİSİ // CONFIG MATRIX",
                [UiLanguage.English] = "⚙️ SETTINGS & PROFILE MATRIX // CONFIG MATRIX",
                [UiLanguage.French] = "⚙️ GESTION DES PROFILS // MATRICE CONFIG",
                [UiLanguage.German] = "⚙️ EINSTELLUNGEN & PROFILE // CONFIG MATRIX"
            },
            ["ActiveProfile"] = new()
            {
                [UiLanguage.Turkish] = "Aktif Profil:",
                [UiLanguage.English] = "Active Profile:",
                [UiLanguage.French] = "Profil Actif :",
                [UiLanguage.German] = "Aktives Profil:"
            },
            ["AutoSavedHint"] = new()
            {
                [UiLanguage.Turkish] = "● Tüm ayarlar anlık otomatik kaydedilir",
                [UiLanguage.English] = "● All settings are saved automatically",
                [UiLanguage.French] = "● Paramètres enregistrés automatiquement",
                [UiLanguage.German] = "● Einstellungen werden automatisch gespeichert"
            },
            ["NewProfileNamePlaceholder"] = new()
            {
                [UiLanguage.Turkish] = "Yeni profil adı girin...",
                [UiLanguage.English] = "Enter new profile name...",
                [UiLanguage.French] = "Nom du nouveau profil...",
                [UiLanguage.German] = "Neuen Profilnamen eingeben..."
            },
            ["SaveNewProfile"] = new()
            {
                [UiLanguage.Turkish] = "💾 YENİ KAYDET",
                [UiLanguage.English] = "💾 SAVE NEW",
                [UiLanguage.French] = "💾 ENREGISTRER",
                [UiLanguage.German] = "💾 NEU SPEICHERN"
            },
            ["UpdateCurrentProfile"] = new()
            {
                [UiLanguage.Turkish] = "🔄 GÜNCELLE",
                [UiLanguage.English] = "🔄 UPDATE",
                [UiLanguage.French] = "🔄 METTRE À JOUR",
                [UiLanguage.German] = "🔄 AKTUALISIEREN"
            },
            ["LoadProfile"] = new()
            {
                [UiLanguage.Turkish] = "✓ YÜKLE",
                [UiLanguage.English] = "✓ LOAD",
                [UiLanguage.French] = "✓ CHARGER",
                [UiLanguage.German] = "✓ LADEN"
            },
            ["OverwriteProfile"] = new()
            {
                [UiLanguage.Turkish] = "💾 KAYDET",
                [UiLanguage.English] = "💾 SAVE",
                [UiLanguage.French] = "💾 ENR.",
                [UiLanguage.German] = "💾 SP."
            },
            ["DeleteProfile"] = new()
            {
                [UiLanguage.Turkish] = "🗑 SİL",
                [UiLanguage.English] = "🗑 DELETE",
                [UiLanguage.French] = "🗑 SUPPR.",
                [UiLanguage.German] = "🗑 LÖSCHEN"
            },
            ["SavedProfiles"] = new()
            {
                [UiLanguage.Turkish] = "Kayıtlı Profiller:",
                [UiLanguage.English] = "Saved Profiles:",
                [UiLanguage.French] = "Profils Enregistrés :",
                [UiLanguage.German] = "Gespeicherte Profile:"
            },
            ["QuickPresets"] = new()
            {
                [UiLanguage.Turkish] = "Hızlı İsimler:",
                [UiLanguage.English] = "Quick Names:",
                [UiLanguage.French] = "Noms Rapides :",
                [UiLanguage.German] = "Schnellnamen:"
            },
            ["ProfileLoadedFeedback"] = new()
            {
                [UiLanguage.Turkish] = "✓ Profil başarıyla yüklendi ve uygulandı!",
                [UiLanguage.English] = "✓ Profile loaded and applied successfully!",
                [UiLanguage.French] = "✓ Profil chargé et appliqué avec succès !",
                [UiLanguage.German] = "✓ Profil erfolgreich geladen und angewendet!"
            },
            ["ProfileSavedFeedback"] = new()
            {
                [UiLanguage.Turkish] = "✓ Profil başarıyla kaydedildi!",
                [UiLanguage.English] = "✓ Profile saved successfully!",
                [UiLanguage.French] = "✓ Profil enregistré avec succès !",
                [UiLanguage.German] = "✓ Profil erfolgreich gespeichert!"
            },
            ["ProfileDeletedFeedback"] = new()
            {
                [UiLanguage.Turkish] = "✓ Profil silindi.",
                [UiLanguage.English] = "✓ Profile deleted.",
                [UiLanguage.French] = "✓ Profil supprimé.",
                [UiLanguage.German] = "✓ Profil gelöscht."
            }
        };

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

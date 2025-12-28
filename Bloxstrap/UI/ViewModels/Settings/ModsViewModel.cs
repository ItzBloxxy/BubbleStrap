using Bloxstrap.Enums;
using Bloxstrap.AppData;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Shell;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class ModsViewModel : NotifyPropertyChangedViewModel
    {
        private void OpenModsFolder() => Process.Start("explorer.exe", Paths.Modifications);

        private readonly Dictionary<string, byte[]> FontHeaders = new()
        {
            { "ttf", new byte[4] { 0x00, 0x01, 0x00, 0x00 } },
            { "otf", new byte[4] { 0x4F, 0x54, 0x54, 0x4F } },
            { "ttc", new byte[4] { 0x74, 0x74, 0x63, 0x66 } }
        };

        private void ManageCustomFont()
        {
            if (!String.IsNullOrEmpty(TextFontTask.NewState))
            {
                TextFontTask.NewState = "";
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = $"{Strings.Menu_FontFiles}|*.ttf;*.otf;*.ttc"
                };

                if (dialog.ShowDialog() != true)
                    return;

                string type = dialog.FileName.Substring(dialog.FileName.Length - 3, 3).ToLowerInvariant();

                if (!FontHeaders.ContainsKey(type)
                    || !FontHeaders.Any(x => File.ReadAllBytes(dialog.FileName).Take(4).SequenceEqual(x.Value)))
                {
                    Frontend.ShowMessageBox(Strings.Menu_Mods_Misc_CustomFont_Invalid, MessageBoxImage.Error);
                    return;
                }

                TextFontTask.NewState = dialog.FileName;
            }

            OnPropertyChanged(nameof(ChooseCustomFontVisibility));
            OnPropertyChanged(nameof(DeleteCustomFontVisibility));
        }

        public ICommand OpenModsFolderCommand => new RelayCommand(OpenModsFolder);

        public Visibility ChooseCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Collapsed : Visibility.Visible;

        public Visibility DeleteCustomFontVisibility => !String.IsNullOrEmpty(TextFontTask.NewState) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand ManageCustomFontCommand => new RelayCommand(ManageCustomFont);

        public ICommand AddCustomDeathSoundCommand => new RelayCommand(AddCustomDeathSound);

        public ICommand RemoveCustomDeathSoundCommand => new RelayCommand(RemoveCustomDeathSound);

        public ICommand OpenCompatSettingsCommand => new RelayCommand(OpenCompatSettings);

        public ModPresetTask OldAvatarBackgroundTask { get; } = new("OldAvatarBackground", @"ExtraContent\places\Mobile.rbxl", "OldAvatarBackground.rbxl");

        public ModPresetTask OldCharacterSoundsTask { get; } = new("OldCharacterSounds", new()
        {
            { @"content\sounds\action_footsteps_plastic.mp3", "Sounds.OldWalk.mp3"  },
            { @"content\sounds\action_jump.mp3",              "Sounds.OldJump.mp3"  },
            { @"content\sounds\action_get_up.mp3",            "Sounds.OldGetUp.mp3" },
            { @"content\sounds\action_falling.mp3",           "Sounds.Empty.mp3"    },
            { @"content\sounds\action_jump_land.mp3",         "Sounds.Empty.mp3"    },
            { @"content\sounds\action_swim.mp3",              "Sounds.Empty.mp3"    },
            { @"content\sounds\impact_water.mp3",             "Sounds.Empty.mp3"    }
        });

        public EmojiModPresetTask EmojiFontTask { get; } = new();

        public EnumModPresetTask<Enums.CursorType> CursorTypeTask { get; } = new("CursorType", new()
        {
            {
                Enums.CursorType.From2006, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2006.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2006.ArrowFarCursor.png" }
                }
            },
            {
                Enums.CursorType.From2013, new()
                {
                    { @"content\textures\Cursors\KeyboardMouse\ArrowCursor.png",    "Cursor.From2013.ArrowCursor.png"    },
                    { @"content\textures\Cursors\KeyboardMouse\ArrowFarCursor.png", "Cursor.From2013.ArrowFarCursor.png" }
                }
            }
        });

        private Visibility GetVisibility(string directory, string[] filenames, bool checkExist)
        {
            bool anyExist = filenames.Any(name => File.Exists(Path.Combine(directory, name)));
            return (checkExist ? anyExist : !anyExist) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility ChooseCustomDeathSoundVisibility =>
            GetVisibility(Path.Combine(Paths.Modifications, "Content", "sounds"),
                  new[] { "oof.ogg" }, checkExist: false);

        public Visibility DeleteCustomDeathSoundVisibility =>
            GetVisibility(Path.Combine(Paths.Modifications, "Content", "sounds"),
                          new[] { "oof.ogg" }, checkExist: true);

        public FontModPresetTask TextFontTask { get; } = new();

        private void AddCustomFile(string[] targetFiles, string targetDir, string dialogTitle, string filter, string failureText, Action postAction = null!)
        {
            var dialog = new OpenFileDialog
            {
                Filter = filter,
                Title = dialogTitle
            };

            if (dialog.ShowDialog() != true)
                return;

            string sourcePath = dialog.FileName;
            Directory.CreateDirectory(targetDir);

            try
            {
                foreach (var name in targetFiles)
                {
                    string destPath = Path.Combine(targetDir, name);
                    File.Copy(sourcePath, destPath, overwrite: true);
                }
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Failed to add {failureText}:\n{ex.Message}", MessageBoxImage.Error);
                return;
            }

            postAction?.Invoke();
        }

        private void RemoveCustomFile(string[] targetFiles, string targetDir, string notFoundMessage, Action postAction = null!)
        {
            bool anyDeleted = false;

            foreach (var name in targetFiles)
            {
                string filePath = Path.Combine(targetDir, name);
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                        anyDeleted = true;
                    }
                    catch (Exception ex)
                    {
                        Frontend.ShowMessageBox($"Failed to remove {name}:\n{ex.Message}", MessageBoxImage.Error);
                    }
                }
            }

            if (!anyDeleted)
            {
                Frontend.ShowMessageBox(notFoundMessage, MessageBoxImage.Information);
            }

            postAction?.Invoke();
        }

        private void OpenCompatSettings()
        {
            string path = new RobloxPlayerData().ExecutablePath;

            if (File.Exists(path))
                PInvoke.SHObjectProperties(HWND.Null, SHOP_TYPE.SHOP_FILEPATH, path, "Compatibility");
            else
                Frontend.ShowMessageBox(Strings.Common_RobloxNotInstalled, MessageBoxImage.Error);

        }

        public void AddCustomDeathSound()
        {
            AddCustomFile(
                new[] { "oof.ogg" },
                Path.Combine(Paths.Modifications, "Content", "sounds"),
                "Select a Custom Death Sound",
                "OGG Audio (*.ogg)|*.ogg",
                "death sound",
                () =>
                {
                    OnPropertyChanged(nameof(ChooseCustomDeathSoundVisibility));
                    OnPropertyChanged(nameof(DeleteCustomDeathSoundVisibility));
                });
        }

        public void RemoveCustomDeathSound()
        {
            RemoveCustomFile(
                new[] { "oof.ogg" },
                Path.Combine(Paths.Modifications, "Content", "sounds"),
                "No custom death sound found to remove.",
                () =>
                {
                    OnPropertyChanged(nameof(ChooseCustomDeathSoundVisibility));
                    OnPropertyChanged(nameof(DeleteCustomDeathSoundVisibility));
                });
        }
    }
}
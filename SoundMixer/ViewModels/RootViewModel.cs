﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using SoundMixer.Models;
using SoundMixer.Miscellanea;
using SoundMixer.Extensions;

using Newtonsoft.Json;

using Stylet;
using Unosquare.FFME.Common;
using GongSolutions.Wpf.DragDrop;
using SoundMixer.UserControls;
using Serilog;
using System.Threading.Channels;
using Microsoft.WindowsAPICodePack.Shell;
using System.Threading;
using NYoutubeDL;
using NYoutubeDL.Models;

namespace SoundMixer.ViewModels
{
    public class RootViewModel : Screen, IHandle<AddedSoundFromStream>, IHandle<AddingSoundFromStream>, IDragSource, IDropTarget
    {
        readonly private IWindowManager windowManager;
        readonly private IEventAggregator eventAggregator;

        readonly private static double defaultVolume = 0.5;
        private bool isDirty = false;
        readonly private ResourceDictionary styles;
        readonly private Style defaultSceneButtonStyle;
        readonly private Style selectedSceneButtonStyle;
        readonly private Style defaultMoodButtonStyle;
        readonly private Style selectedMoodButtonStyle;

        private string activeFilePath;

        private WorkspaceModel workspace;
        public WorkspaceModel Workspace
        {
            get { return this.workspace; }
            set { this.SetAndNotify(ref this.workspace, value); }
        }

        private SceneModel selectedScene;
        public SceneModel SelectedScene
        {
            get { return this.selectedScene; }
            set { this.SetAndNotify(ref this.selectedScene, value); }
        }

        private MoodModel selectedMood;
        public MoodModel SelectedMood
        {
            get { return this.selectedMood; }
            set { this.SetAndNotify(ref this.selectedMood, value); }
        }

        private DirectSoundDeviceInfo selectedOutputDevice;
        public DirectSoundDeviceInfo SelectedOutputDevice
        {
            get { return this.selectedOutputDevice; }
            set { this.SetAndNotify(ref this.selectedOutputDevice, value); }
        }

        private BindableCollection<DirectSoundDeviceInfo> outputDevices;
        public BindableCollection<DirectSoundDeviceInfo> OutputDevices
        {
            get { return this.outputDevices; }
            set { this.SetAndNotify(ref this.outputDevices, value); }
        }

        private bool isPlayingAll;
        public bool IsPlayingAll
        {
            get { return this.isPlayingAll; }
            set { this.SetAndNotify(ref this.isPlayingAll, value); }
        }

        private Enums.ProgramStatus status;
        public Enums.ProgramStatus Status
        {
            get { return this.status; }
            set
            {
                this.SetAndNotify(ref this.status, value);
            }
        }

        public RootViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
        {
            // Configure logging (keep the last 7 days)
            Log.Logger = new LoggerConfiguration().WriteTo.File("log-.txt", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7).CreateLogger();

            // Configure global exception handling
            if (Application.Current != null)
            {
                Application.Current.DispatcherUnhandledException += (s, e) =>
                {
                    Log.Fatal(e.Exception.Message);

#if (!DEBUG)
                    e.Handled = true;
#endif
                };
            }

            // Configure FFME
            Unosquare.FFME.Library.FFmpegDirectory = @"Resources\ffmpeg";
            Unosquare.FFME.Library.FFmpegLoadModeFlags = FFmpeg.AutoGen.FFmpegLoadMode.AudioOnly;

            Log.Information("Loading FFME from {0}.", Path.GetFullPath(Unosquare.FFME.Library.FFmpegDirectory));
            Log.Information("FFME flags: {0}.", Unosquare.FFME.Library.FFmpegLoadModeFlags.ToString());

            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;

            eventAggregator.Subscribe(this);

            Workspace = new WorkspaceModel();

            styles = new ResourceDictionary()
            {
                Source = new Uri("/SoundMixer;component/Styles.xaml", UriKind.RelativeOrAbsolute)
            };

            defaultSceneButtonStyle = styles["SceneButton"] as Style;
            selectedSceneButtonStyle = styles["SceneButtonSelected"] as Style;
            defaultMoodButtonStyle = styles["MoodButton"] as Style;
            selectedMoodButtonStyle = styles["MoodButtonSelected"] as Style;

            Status = Enums.ProgramStatus.Ready;

            outputDevices = new BindableCollection<DirectSoundDeviceInfo>(Unosquare.FFME.Library.EnumerateDirectSoundDevices());
            var lastOutputDeviceId = Settings.GetSetting(Enums.Setting.OutputDeviceId);

            SelectedOutputDevice = string.IsNullOrWhiteSpace(lastOutputDeviceId) || !outputDevices.Any(d => d.DeviceId.ToString() == lastOutputDeviceId) ? outputDevices.ElementAt(0) : outputDevices.Single(d => d.DeviceId.ToString() == lastOutputDeviceId);
        }

        /// <summary>
        /// Generate a name based on a string that is not present on a specified list of strings
        /// </summary>
        /// <param name="baseName"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public string GetUniqueNameFromString(string baseName, IList<string> list)
        {
            // Generate unique name for scene
            var potentialName = baseName;
            var i = 1;

            while (list.Contains(potentialName))
            {
                potentialName = baseName + " " + i;
                ++i;
            }

            return potentialName;
        }

        /// <summary>
        /// Add a new scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void AddScene(string sceneName)
        {
            Workspace.Scenes.Add(new SceneModel(GetUniqueNameFromString(sceneName, Workspace.Scenes.Select(o => o.Name).ToList())));

            isDirty = true;

            Log.Information("Adding scene {0}.", sceneName);
        }

        /// <summary>
        /// Remove a scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void RemoveScene(string sceneName)
        {
            var length = Workspace.Scenes.Count;
            for (var i = length - 1; i >= 0; --i)
            {
                if (Workspace.Scenes[i].Name == sceneName)
                {
                    var selectedSceneName = SelectedScene.Name; // Just in case we are removing the selected scene

                    // Select a different scene and mood if we deleted the selected one
                    if (selectedSceneName == sceneName)
                    {
                        StopAllSounds();
                        SelectScene(0);
                    }

                    Workspace.Scenes.RemoveAt(i);

                    isDirty = true;

                    Log.Information("Removing scene {0}.", sceneName);
                }
            }
        }

        /// <summary>
        /// Add a new mood to the selected scene
        /// </summary>
        /// <param name="moodName"></param>
        /// <param name="sceneName"></param>
        public void AddMood(string moodName)
        {
            var newMood = new MoodModel(GetUniqueNameFromString(moodName, SelectedScene.Moods.Select(o => o.Name).ToList()));
            SelectedScene.Moods.Add(newMood);

            // Then add all the sounds from the scene
            foreach (var sound in SelectedScene.Sounds)
            {
                newMood.SoundProperties.Add(new SoundPropertyModel(sound.Name, defaultVolume, sound.GUID));
                newMood.SoundProperties.LastOrDefault().Sound = sound;
            }

            isDirty = true;

            Log.Information("Adding mood {0} to scene {1}.", moodName, SelectedScene.Name);
        }

        /// <summary>
        /// Remove a mood from the selected scene
        /// </summary>
        /// <param name="moodName"></param>
        /// <param name="sceneName"></param>
        public void RemoveMood(string moodName)
        {
            var length = SelectedScene.Moods.Count;
            for (var i = length - 1; i >= 0; --i)
            {
                if (SelectedScene.Moods[i].Name == moodName)
                {

                    // Select a different mood if we deleted the selected one
                    if (SelectedMood.Name == moodName)
                    {
                        StopAllSounds();
                        SelectedMood = SelectedScene.Moods.Count > 1 ? SelectedScene.Moods[0] : null;
                    }

                    SelectedScene.Moods.RemoveAt(i);

                    isDirty = true;

                    Log.Information("Removing mood {0} from scene {1}.", moodName, SelectedScene.Name);
                }
            }
        }

        /// <summary>
        /// Add a new sound to the selected scene
        /// </summary>
        /// <param name="soundPath"></param>
        public void AddSound(string soundPath, SoundPropertyModel soundPropertyModel = null, bool isURL = false)
        {
            string tempName;

            if (isURL)
            {
                // TODO: Get ID field from URL
                tempName = "URL";
            }
            else
            {
                tempName = Path.GetFileNameWithoutExtension(soundPath);
            }

            // TODO: Move this to another thread
            var soundDuration = GetMediaDuration(soundPath, isURL);

            var newSound = new SoundModel(GetUniqueNameFromString(tempName, SelectedScene.Sounds.Select(o => o.Name).ToList()), soundPath, soundDuration, soundPropertyModel?.GUID ?? Guid.Empty, isURL);
            SelectedScene.Sounds.Add(newSound);

            // Then add to every mood
            foreach (var mood in SelectedScene.Moods)
            {
                if (soundPropertyModel == null)
                {
                    var newName = GetUniqueNameFromString(newSound.Name, SelectedMood.SoundProperties.Select(o => o.Name).ToList());
                    mood.SoundProperties.Add(new SoundPropertyModel(newName, defaultVolume, newSound.GUID));
                }
                else
                {
                    // Clone the sound
                    var newName = GetUniqueNameFromString(soundPropertyModel.Name, SelectedMood.SoundProperties.Select(o => o.Name).ToList());
                    var newSoundPropertyModel = new SoundPropertyModel(newName, soundPropertyModel.Volume, soundPropertyModel.GUID)
                    {
                        IsDelayed = soundPropertyModel.IsDelayed,
                        IsLoop = soundPropertyModel.IsLoop,
                        UseRandomDelay = soundPropertyModel.UseRandomDelay,
                        DelayTime = soundPropertyModel.DelayTime,
                        MinDelay = soundPropertyModel.MinDelay,
                        MaxDelay = soundPropertyModel.MaxDelay
                    };
                    mood.SoundProperties.Add(newSoundPropertyModel);
                }

                var sound = FindSoundFromGuid(newSound.GUID);
                mood.SoundProperties.LastOrDefault().Sound = sound;
            }

            isDirty = true;

            Log.Information("Adding sound {0} to scene {1}.", newSound.Name, SelectedScene.Name);
        }

        /// <summary>
        /// Remove a sound from the selected scene
        /// </summary>
        /// <param name="soundName"></param>
        public void RemoveSound(string soundName)
        {
            var length = SelectedScene.Sounds.Count;
            var soundIndex = -1;
            for (var i = length - 1; i >= 0; --i)
            {
                if (SelectedScene.Sounds[i].Name == soundName)
                {
                    SelectedScene.Sounds.RemoveAt(i);
                    soundIndex = i;
                    break;
                }
            }

            // Then remove from every mood
            foreach (var mood in SelectedScene.Moods)
            {
                mood.SoundProperties.RemoveAt(soundIndex);
            }

            isDirty = true;

            Log.Information("Removing sound {0} from scene {1}.", soundName, SelectedScene.Name);
        }

        /// <summary>
        /// Remove a sound from the selected mood in the selected scene
        /// </summary>
        /// <param name="soundGuid"></param>
        public void RemoveSound(Guid soundGuid)
        {
            var length = SelectedScene.Sounds.Count;
            var soundIndex = -1;
            for (var i = length - 1; i >= 0; --i)
            {
                if (SelectedScene.Sounds[i].GUID == soundGuid)
                {
                    SelectedScene.Sounds.RemoveAt(i);
                    soundIndex = i;
                    break;
                }
            }

            // Then remove from every mood
            foreach (var mood in SelectedScene.Moods)
            {
                mood.SoundProperties.RemoveAt(soundIndex);
            }

            isDirty = true;

            Log.Information("Removing sound {0} from scene {1}.", FindSoundFromGuid(soundGuid).Name, SelectedScene.Name);
        }

        public void SelectScene(string name)
        {
            StopAllSounds();

            for (var i = 0; i < Workspace.Scenes.Count; ++i)
            {
                if (Workspace.Scenes[i].Name == name)
                {
                    SelectedScene = Workspace.Scenes[i];

                    // It is possible that the UI has not updated yet when we go to
                    // call PlayAllSounds, so force an update
                    View?.UpdateLayout();

                    var sceneStack = (View as Views.RootView).SceneStack;
                    var sceneButtons = sceneStack.GetChildrenOfType<Button>();

                    // Apply the default style to every scene button and the selected style to the newly selected one
                    foreach (var sceneButton in sceneButtons)
                    {
                        if ((sceneButton.Content as UserControls.EditableTextBlock).Text == name)
                        {
                            sceneButton.Style = selectedSceneButtonStyle;
                        }
                        else
                        {
                            sceneButton.Style = defaultSceneButtonStyle;
                        }
                    }

                    break;
                }
            }

            //SelectMood(0);
            Log.Information("Selecting scene {0} by name.", name);
        }

        public void SelectScene(int index)
        {
            StopAllSounds();

            SelectedScene = Workspace.Scenes.Count > index ? Workspace.Scenes[index] : null;

            // It is possible that the UI has not updated yet when we go to
            // call PlayAllSounds, so force an update
            View?.UpdateLayout();

            var sceneStack = (View as Views.RootView).SceneStack;
            var sceneButtons = sceneStack.GetChildrenOfType<Button>();

            // Apply the default style to every scene button and the selected style to the newly selected one
            foreach (var sceneButton in sceneButtons)
            {
                if ((sceneButton.Content as UserControls.EditableTextBlock).Text == SelectedScene.Name)
                {
                    sceneButton.Style = selectedSceneButtonStyle;
                }
                else
                {
                    sceneButton.Style = defaultSceneButtonStyle;
                }
            }

            //SelectMood(0);
            Log.Information("Selecting scene {0} by index.", Workspace.Scenes[index].Name);
        }

        public void SelectMood(string name)
        {
            StopAllSounds();

            for (var i = 0; i < SelectedScene.Moods.Count; ++i)
            {
                if (SelectedScene.Moods[i].Name == name)
                {
                    SelectedMood = SelectedScene.Moods[i];

                    // It is possible that the UI has not updated yet when we go to
                    // call PlayAllSounds, so force an update
                    View?.UpdateLayout();

                    var moodStack = (View as Views.RootView).MoodStack;
                    var moodButtons = moodStack.GetChildrenOfType<Button>();

                    // Apply the default style to every scene button and the selected style to the newly selected one
                    foreach (var moodButton in moodButtons)
                    {
                        if ((moodButton.Content as UserControls.EditableTextBlock).Text == name)
                        {
                            moodButton.Style = selectedMoodButtonStyle;
                        }
                        else
                        {
                            moodButton.Style = defaultMoodButtonStyle;
                        }
                    }

                    PlayAllSounds();
                    break;
                }
            }
            Log.Information("Selecting mood {0} in {1} by name.", name, SelectedScene.Name);
        }

        public void SelectMood(int index)
        {
            StopAllSounds();

            SelectedMood = SelectedScene.Moods.Count > index ? SelectedScene.Moods[index] : null;

            // It is possible that the UI has not updated yet when we go to
            // call PlayAllSounds, so force an update
            View?.UpdateLayout();

            var moodStack = (View as Views.RootView).MoodStack;
            var moodButtons = moodStack.GetChildrenOfType<Button>();

            // Apply the default style to every scene button and the selected style to the newly selected one
            foreach (var moodButton in moodButtons)
            {
                if ((moodButton.Content as UserControls.EditableTextBlock).Text == SelectedMood.Name)
                {
                    moodButton.Style = selectedMoodButtonStyle;
                }
                else
                {
                    moodButton.Style = defaultMoodButtonStyle;
                }
            }

            PlayAllSounds();
            Log.Information("Selecting mood {0} in {1} by index.", SelectedScene.Moods[index].Name, SelectedScene.Name);
        }

        public void SaveWorkspace(string path)
        {
            string json = JsonConvert.SerializeObject(Workspace, Formatting.Indented);
            File.WriteAllText(path, json);

            Log.Information("Saving workspace to {0}.", path);
        }

        public void LoadWorkspace(string path)
        {
            string json = File.ReadAllText(path);
            Workspace = JsonConvert.DeserializeObject<WorkspaceModel>(json);

            // Make sure all SounPropertyModels have the correct sound according
            // to the GUID
            foreach (var scene in Workspace.Scenes)
            {
                if (scene.Moods.Count > 0)
                {
                    for (var i = 0; i < scene.Moods[0]?.SoundProperties.Count; ++i)
                    {
                        SoundModel sound = FindSoundFromGuid(scene.Moods[0].SoundProperties[i].GUID);

                        // Since all moods contain the same sounds, we only need to find the matching sound once
                        foreach (var mood in scene.Moods)
                        {
                            mood.SoundProperties[i].Sound = sound;
                            mood.SoundProperties[i].Name = sound.Name;
                        }
                    }
                }
            }

            // After loading everything, set a default selection
            SelectScene(0);

            Log.Information("Loading workspace from {0}", path);
        }

        public SoundModel FindSoundFromGuid(Guid guid)
        {
            foreach (var scene in Workspace.Scenes)
            {
                var potentialResult = scene.Sounds.Where(o => o.GUID == guid).FirstOrDefault();

                if (potentialResult != null) return potentialResult;
            }

            // None matches
            return null;
        }

        public void PlayOrStopAllSoundsButton()
        {
            if (IsPlayingAll)
            {
                StopAllSounds();
            }
            else
            {
                PlayAllSounds();
            }
        }

        public void PlayAllSounds()
        {
            if (View != null)
            {
                var foundOne = false;
                var soundStack = (View as Views.RootView).SoundStack;
                var soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    if (!soundControl.IsPlaying && soundControl.SoundPropertyModel.IsLoop)
                    {
                        // Don't want to accidentally restart a sound
                        _ = soundControl.PlayOrStop();
                        foundOne = true;
                    }
                }

                // Also update the icon
                if (foundOne) IsPlayingAll = true;
            }
        }

        public void StopAllSounds()
        {
            if (View != null)
            {
                var soundStack = (View as Views.RootView).SoundStack;
                var soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    if (soundControl.IsPlaying)
                    {
                        // Don't want to accidentally restart a sound
                        _ = soundControl.PlayOrStop();
                    }
                }

                // And update the icon
                IsPlayingAll = false;
            }
        }

        public bool ContinueAfterOfferSave()
        {
            var fileName = Path.GetFileNameWithoutExtension(activeFilePath);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Untitled";
            }
            var message = $"Do you want to save changes to {fileName}?";
            var result = this.windowManager.ShowMessageBox(message, "SoundMixer", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes)
            {
                SaveWorkspaceFile();
                return true;
            }
            else if (result == MessageBoxResult.No)
            {
                isDirty = false;
                return true;
            }

            return false;
        }

        public void NewWorkspaceFile()
        {
            StopAllSounds();
            if (isDirty)
            {
                if (!ContinueAfterOfferSave())
                {
                    return;
                }
            }
            Workspace = new WorkspaceModel();
            SelectedScene = null;
            SelectedMood = null;
            activeFilePath = "";

            Log.Information("Creating new workspace.");
        }

        public void OpenWorkspaceFile()
        {
            StopAllSounds();
            if (isDirty)
            {
                if (!ContinueAfterOfferSave())
                {
                    return;
                }
            }

            var openFileDialog = new OpenFileDialog()
            {
                Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*",
                InitialDirectory = Settings.GetSetting(Enums.Setting.WorkspaceDirectory)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                var path = openFileDialog.FileName;
                if (File.Exists(path))
                {
                    activeFilePath = path;
                    Settings.SetSetting(Enums.Setting.WorkspaceDirectory, Directory.GetParent(activeFilePath).FullName);
                    LoadWorkspace(activeFilePath);
                }
            }
        }

        public void SaveWorkspaceFile()
        {
            // We should save over the previously used file, if any
            if (string.IsNullOrEmpty(activeFilePath))
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*",
                    InitialDirectory = Settings.GetSetting(Enums.Setting.WorkspaceDirectory)
                };
                if ((bool)saveFileDialog.ShowDialog())
                {
                    activeFilePath = saveFileDialog.FileName;
                    Settings.SetSetting(Enums.Setting.WorkspaceDirectory, Directory.GetParent(activeFilePath).FullName);
                }
                else
                {
                    // Do nothing
                    return;
                }
            }
            SaveWorkspace(activeFilePath);
            isDirty = false;
        }

        public void SaveWorkspaceFileAs()
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*",
                InitialDirectory = Settings.GetSetting(Enums.Setting.WorkspaceDirectory)
            };
            if ((bool)saveFileDialog.ShowDialog())
            {
                activeFilePath = saveFileDialog.FileName;
                Settings.SetSetting(Enums.Setting.WorkspaceDirectory, Directory.GetParent(activeFilePath).FullName);
                SaveWorkspace(activeFilePath);
                isDirty = false;
            }
        }


        public void AddSceneButton()
        {
            AddScene("New Scene");

            // Select scene if none selected
            if (SelectedScene == null)
            {
                SelectScene(Workspace.Scenes[0].Name);
            }
        }

        public void AddMoodButton()
        {
            if (SelectedScene != null)
            {
                AddMood("New Mood");

                // Select mood if none selected
                if (SelectedMood == null)
                {
                    SelectMood(SelectedScene.Moods[0].Name);
                }
            }
        }

        public void AddSoundButton()
        {
            if (SelectedMood != null)
            {
                // First open a file dialog to select the sound
                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Settings.GetSetting(Enums.Setting.SoundDirectory)
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var filePath = openFileDialog.FileName;
                    Settings.SetSetting(Enums.Setting.SoundDirectory, Directory.GetParent(filePath).FullName);
                    AddSound(filePath);
                }
            }
        }

        public void AddSoundFromStreamButton()
        {
            if (SelectedMood != null)
            {
                // Adding from youtube, so need to ask for a link
                this.windowManager.ShowDialog(new StreamViewModel(eventAggregator));

                var downloadedFilePath = "";

                if (File.Exists(downloadedFilePath))
                {
                    AddSound(downloadedFilePath);
                }
            }
        }

        public void OpenPreferences()
        {
            var settingsModel = new SettingsModel()
            {
                SelectedOutputDevice = this.SelectedOutputDevice,
                AvailableOutputDevices = this.OutputDevices
            };
            this.windowManager.ShowDialog(new SettingsViewModel(settingsModel));
            SelectedOutputDevice = settingsModel.SelectedOutputDevice;
            Settings.SetSetting(Enums.Setting.OutputDeviceId, SelectedOutputDevice.DeviceId.ToString());


            // Also need to update all sounds to use the new SelectedOutputDevice
            UpdateOutputDeviceInSounds();
        }

        public void UpdateOutputDeviceInSounds()
        {
            var soundStack = (View as Views.RootView).SoundStack;
            var soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

            // First go through all sound controls and check if any is solo
            /*foreach (var soundControl in soundControls)
            {
                //soundControl.SetOutputDevice(SelectedOutputDevice);
            }*/
        }

        /// <summary>
        ///  Number of ticks in file or video
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isURL"></param>
        /// <returns></returns>
        public long GetMediaDuration(string path, bool isURL)
        {
            long duration;
            if (isURL)
            {
                var youtubeDl = new YoutubeDL
                {
                    VideoUrl = path,
                    RetrieveAllInfo = true
                };

                // This is taking way too long
                // TODO: Fix and re-include
                // youtubeDl.PrepareDownload();
                return 0;
                //TimeSpan durationSpan = TimeSpan.FromSeconds((long)(youtubeDl.Info as VideoDownloadInfo).Duration);
                //duration = durationSpan.Ticks;
            }
            else
            {
                ShellFile sf = ShellFile.FromFilePath(path);
                long.TryParse(sf.Properties.System.Media.Duration.Value.ToString(), out duration);
            }
            return duration;
        }

        #region EventHandlers

        public void New_Click(object sender, RoutedEventArgs e)
        {
            NewWorkspaceFile();
        }

        public void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenWorkspaceFile();
        }

        public void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveWorkspaceFile();
        }

        public void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveWorkspaceFileAs();
        }

        public void AddScene_Click(object sender, RoutedEventArgs e)
        {
            AddSceneButton();
        }

        public void AddMood_Click(object sender, RoutedEventArgs e)
        {
            AddMoodButton();
        }

        public void AddSound_Click(object sender, RoutedEventArgs e)
        {
            // Shift was pressed during the click
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                AddSoundFromStreamButton();
            }
            else
            {
                AddSoundButton();
            }
        }

        public void AddSoundFromYouTube_Click(object sender, RoutedEventArgs e)
        {
            AddSoundFromStreamButton();
        }

        public void Preferences_Click(object sender, RoutedEventArgs e)
        {
            OpenPreferences();
        }

        public void SelectScene_LeftMouseUp(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button)?.Content as UserControls.EditableTextBlock).Text;
            if (name != null)
            {
                SelectScene(name);
            }
        }

        public void SelectMood_LeftMouseUp(object sender, RoutedEventArgs e)
        {
            var name = ((sender as Button)?.Content as UserControls.EditableTextBlock).Text;
            if (name != null)
            {
                SelectMood(name);
            }
        }

        public void RenameSceneButton_Click(object sender, RoutedEventArgs e)
        {
            var editableTextBlock = (((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock;

            editableTextBlock.EnterEditMode();
        }

        public void RemoveSceneButton_Click(object sender, RoutedEventArgs e)
        {
            var name = ((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock)?.Text;
            if (name != null)
            {
                RemoveScene(name);
            }
        }

        public void RenameMoodButton_Click(object sender, RoutedEventArgs e)
        {
            var editableTextBlock = (((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock;

            editableTextBlock.EnterEditMode();
        }

        public void RemoveMoodButton_Click(object sender, RoutedEventArgs e)
        {
            var name = ((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock)?.Text;
            if (name != null)
            {
                RemoveMood(name);
            }
        }

        public void EditSoundControl_Click(object sender, RoutedEventArgs e)
        {
            var soundControl = ((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as UserControls.SoundControl;

            var soundEditViewModel = new SoundEditViewModel(soundControl.SoundPropertyModel);
            this.windowManager.ShowDialog(soundEditViewModel);
        }

        public void CloneSoundControl_Click(object sender, RoutedEventArgs e)
        {
            var soundPropertyModel = (((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as UserControls.SoundControl)?.SoundPropertyModel;

            if (soundPropertyModel != null)
            {
                AddSound(soundPropertyModel.Sound.FilePath, soundPropertyModel);
            }
        }

        public void RemoveSoundControl_Click(object sender, RoutedEventArgs e)
        {
            var soundControl = (((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as UserControls.SoundControl);
            var guid = soundControl.SoundPropertyModel.GUID;
            soundControl.Stop();
            RemoveSound(guid);
        }

        public void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            PlayOrStopAllSoundsButton();
        }

        public void ReportABug_Click(object sender, RoutedEventArgs e)
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(executingAssembly.Location);

            string user = "juanferrer";
            string repo = "sound-mixer";
            string body = System.Web.HttpUtility.UrlEncode($"Version: {fvi.ProductVersion}\n\n## Steps to reproduce:\n1. Step 1\n2. Step 2\n3. Step 3\n\n## Actual behaviour\n\n\n## Expected behaviour:\n\n\n## Other details:");
            var psi = new ProcessStartInfo()
            {
                FileName = $"https://github.com/{user}/{repo}/issues/new?body={body}",
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutViewModel = new AboutViewModel();
            this.windowManager.ShowDialog(aboutViewModel);
        }

        public void SoloMuteButton_Click(object sender, RoutedEventArgs e)
        {
            var soundStack = (View as Views.RootView).SoundStack;
            var soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

            var foundOne = false;

            // First go through all sound controls and check if any is solo
            foreach (var soundControl in soundControls)
            {
                if (soundControl.SoundPropertyModel.IsSolo)
                {
                    foundOne = true;
                    break;
                }
            }

            if (foundOne)
            {
                // There is at least one solo
                foreach (var soundControl in soundControls)
                {
                    var shouldPlay = soundControl.SoundPropertyModel.IsSolo && !soundControl.SoundPropertyModel.IsMuted;
                    soundControl.SetPlayerMute(!shouldPlay); // Opposite because the function sets the IsMuted value
                }

            }
            else
            {
                // No solo found, rely on mute only
                foreach (var soundControl in soundControls)
                {
                    soundControl.SetPlayerMute(soundControl.SoundPropertyModel.IsMuted);
                }
            }

            e.Handled = true;
        }

        public void Window_Closing(object sender, EventArgs e)
        {
            Log.CloseAndFlush();
        }

        public void Handle(AddedSoundFromStream e)
        {
            // Get sound path
            var path = e.Args.SoundPath;
            AddSound(path, null, true);

            Status = Enums.ProgramStatus.Ready;
        }

        public void Handle(AddingSoundFromStream e)
        {
            // Start a "loading dialog"
            Status = Enums.ProgramStatus.Loading;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            if (dragInfo.SourceItem is SceneModel scene)
            {
                dragInfo.Effects = DragDropEffects.Move;
                dragInfo.Data = scene;
            }
            else if (dragInfo.SourceItem is MoodModel mood)
            {
                dragInfo.Effects = DragDropEffects.Move;
                dragInfo.Data = mood;
            }
            else if (dragInfo.SourceItem is SoundControl sound)
            {
                dragInfo.Effects = DragDropEffects.Move;
                dragInfo.Data = sound;
            }
        }

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return true;
        }

        public void Dropped(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is SceneModel && dropInfo.TargetItem is not SceneModel) ||
                (dropInfo.Data is MoodModel && (dropInfo.TargetItem is not MoodModel)) ||
                (dropInfo.Data is SoundPropertyModel && (dropInfo.TargetItem is not SoundPropertyModel)))
            {
            }
        }

        public void DragDropOperationFinished(DragDropEffects operationResult, IDragInfo dragInfo)
        {
            return;
        }

        public void DragCancelled()
        {
            return;
        }

        public bool TryCatchOccurredException(Exception exception)
        {
            return false;
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if ((dropInfo.Data is SceneModel && (dropInfo.TargetItem is not SceneModel)) ||
                (dropInfo.Data is MoodModel && (dropInfo.TargetItem is not MoodModel)) ||
                (dropInfo.Data is SoundPropertyModel && (dropInfo.TargetItem is not SoundPropertyModel)))
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            return;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using SoundMixer.Models;
using SoundMixer.Extensions;

using Newtonsoft.Json;
using Stylet;

namespace SoundMixer.ViewModels
{
    public class RootViewModel : Screen
    {
        private IWindowManager windowManager;

        private static double defaultVolume = 0.5;
        private bool isDirty = false;
        private ResourceDictionary styles;
        private Style defaultSceneButtonStyle;
        private Style selectedSceneButtonStyle;
        private Style defaultMoodButtonStyle;
        private Style selectedMoodButtonStyle;

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

        private bool isPlayingAll;
        public bool IsPlayingAll
        {
            get { return this.isPlayingAll; }
            set { this.SetAndNotify(ref this.isPlayingAll, value); }
        }


        public RootViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;

            Workspace = new WorkspaceModel();

            styles = new ResourceDictionary()
            {
                Source = new Uri("/SoundMixer;component/Styles.xaml", UriKind.RelativeOrAbsolute)
            };

            defaultSceneButtonStyle = styles["SceneButton"] as Style;
            selectedSceneButtonStyle = styles["SceneButtonSelected"] as Style;
            defaultMoodButtonStyle = styles["MoodButton"] as Style;
            selectedMoodButtonStyle = styles["MoodButtonSelected"] as Style;
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
            string potentialName = baseName;
            int i = 1;

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
        }

        /// <summary>
        /// Remove a scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void RemoveScene(string sceneName)
        {
            var length = Workspace.Scenes.Count;
            for (int i = length - 1; i >= 0; --i)
            {
                if (Workspace.Scenes[i].Name == sceneName)
                {
                    string selectedSceneName = SelectedScene.Name; // Just in case we accidentally remove it
                    Workspace.Scenes.RemoveAt(i);

                    // Select a different scene and mood if we deleted the selected one
                    if (selectedSceneName == sceneName)
                    {
                        StopAllSounds();
                        SelectScene(0);
                    }

                    isDirty = true;
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
            MoodModel newMood = new MoodModel(GetUniqueNameFromString(moodName, SelectedScene.Moods.Select(o => o.Name).ToList()));
            SelectedScene.Moods.Add(newMood);

            // Then add all the sounds from the scene
            foreach (var sound in SelectedScene.Sounds)
            {
                newMood.SoundProperties.Add(new SoundPropertyModel(sound.Name, defaultVolume, sound.GUID));
                newMood.SoundProperties.LastOrDefault().Sound = sound;
            }

            isDirty = true;
        }

        /// <summary>
        /// Remove a mood from the selected scene
        /// </summary>
        /// <param name="moodName"></param>
        /// <param name="sceneName"></param>
        public void RemoveMood(string moodName)
        {
            var length = SelectedScene.Moods.Count;
            for (int i = length - 1; i >= 0; --i)
            {
                if (SelectedScene.Moods[i].Name == moodName)
                {
                    SelectedScene.Moods.RemoveAt(i);

                    // Select a different mood if we deleted the selected one
                    if (SelectedMood.Name == moodName)
                    {
                        StopAllSounds();
                        //SelectedMood = SelectedScene.Moods.Count > 0 ? SelectedScene.Moods[0] : null;
                    }

                    isDirty = true;
                }
            }
        }

        /// <summary>
        /// Add a new sound to the selected scene
        /// </summary>
        /// <param name="soundPath"></param>
        public void AddSound(string soundPath, SoundPropertyModel soundPropertyModel = null)
        {
            string tempName = Path.GetFileNameWithoutExtension(soundPath);

            SoundModel newSound = new SoundModel(GetUniqueNameFromString(tempName, SelectedScene.Sounds.Select(o => o.Name).ToList()), soundPath);
            SelectedScene.Sounds.Add(newSound);

            // Then add to every mood
            foreach (var mood in SelectedScene.Moods)
            {
                if (soundPropertyModel == null)
                {
                    string newName = GetUniqueNameFromString(newSound.Name, SelectedMood.SoundProperties.Select(o => o.Name).ToList());
                    mood.SoundProperties.Add(new SoundPropertyModel(newName, defaultVolume, newSound.GUID));
                }
                else
                {
                    // Clone the sound
                    string newName = GetUniqueNameFromString(soundPropertyModel.Name, SelectedMood.SoundProperties.Select(o => o.Name).ToList());
                    var newSoundPropertyModel = new SoundPropertyModel(newName, soundPropertyModel.Volume, soundPropertyModel.GUID);
                    newSoundPropertyModel.IsDelayed = soundPropertyModel.IsDelayed;
                    newSoundPropertyModel.IsLoop = soundPropertyModel.IsLoop;
                    newSoundPropertyModel.UseRandomDelay = soundPropertyModel.UseRandomDelay;
                    newSoundPropertyModel.DelayTime = soundPropertyModel.DelayTime;
                    newSoundPropertyModel.MinDelay = soundPropertyModel.MinDelay;
                    newSoundPropertyModel.MaxDelay = soundPropertyModel.MaxDelay;
                    mood.SoundProperties.Add(newSoundPropertyModel);
                }

                SoundModel sound = FindSoundFromGuid(newSound.GUID);
                mood.SoundProperties.LastOrDefault().Sound = sound;
            }

            isDirty = true;
        }

        /// <summary>
        /// Remove a sound from the selected scene
        /// </summary>
        /// <param name="soundName"></param>
        public void RemoveSound(string soundName)
        {
            var length = SelectedScene.Sounds.Count;
            int soundIndex = -1;
            for (int i = length - 1; i >= 0; --i)
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
        }

        /// <summary>
        /// Remove a sound from the selected mood in the selected scene
        /// </summary>
        /// <param name="soundGuid"></param>
        public void RemoveSound(Guid soundGuid)
        {
            var length = SelectedScene.Sounds.Count;
            int soundIndex = -1;
            for (int i = length - 1; i >= 0; --i)
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
        }

        public void SelectScene(string name)
        {
            StopAllSounds();

            for (int i = 0; i < Workspace.Scenes.Count; ++i)
            {
                if (Workspace.Scenes[i].Name == name)
                {
                    SelectedScene = Workspace.Scenes[i];

                    // It is possible that the UI has not updated yet when we go to
                    // call PlayAllSounds, so force an update
                    if (View != null)
                    {
                        View.UpdateLayout();
                    }

                    var sceneStack = (View as Views.RootView).sceneStack;
                    List<Button> sceneButtons = sceneStack.GetChildrenOfType<Button>();

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
        }

        public void SelectScene(int index)
        {
            StopAllSounds();

            SelectedScene = Workspace.Scenes.Count > index ? Workspace.Scenes[index] : null;

            // It is possible that the UI has not updated yet when we go to
            // call PlayAllSounds, so force an update
            if (View != null)
            {
                View.UpdateLayout();
            }

            var sceneStack = (View as Views.RootView).sceneStack;
            List<Button> sceneButtons = sceneStack.GetChildrenOfType<Button>();

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
        }

        public void SelectMood(string name)
        {
            StopAllSounds();

            for (int i = 0; i < SelectedScene.Moods.Count; ++i)
            {
                if (SelectedScene.Moods[i].Name == name)
                {
                    SelectedMood = SelectedScene.Moods[i];

                    // It is possible that the UI has not updated yet when we go to
                    // call PlayAllSounds, so force an update
                    if (View != null)
                    {
                        View.UpdateLayout();
                    }

                    var moodStack = (View as Views.RootView).moodStack;
                    List<Button> moodButtons = moodStack.GetChildrenOfType<Button>();

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
        }

        public void SelectMood(int index)
        {
            StopAllSounds();

            SelectedMood = SelectedScene.Moods.Count > index ? SelectedScene.Moods[index] : null;

            // It is possible that the UI has not updated yet when we go to
            // call PlayAllSounds, so force an update
            if (View != null)
            {
                View.UpdateLayout();
            }

            var moodStack = (View as Views.RootView).moodStack;
            List<Button> moodButtons = moodStack.GetChildrenOfType<Button>();

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
        }

        public void SaveWorkspace(string path)
        {
            string json = JsonConvert.SerializeObject(Workspace, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public void LoadWorkspace(string path)
        {
            string json = File.ReadAllText(path);
            Workspace = JsonConvert.DeserializeObject<WorkspaceModel>(json);

            // Make sure all SounPropertyModels have the correct sound according
            // to the GUID
            foreach (var scene in Workspace.Scenes)
            {
                for (int i = 0; i < scene.Moods[0].SoundProperties.Count; ++i)
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

            // After loading everything, set a default selection
            SelectScene(0);
        }

        public SoundModel FindSoundFromGuid(Guid guid)
        {
            foreach (var scene in Workspace.Scenes)
            {
                SoundModel potentialResult = scene.Sounds.Where(o => o.GUID == guid).FirstOrDefault();

                if (potentialResult != null) return potentialResult;
            }

            // None matches
            return null;
        }

        public void PlayAllSounds()
        {
            if (View != null)
            {
                bool foundOne = false;
                var soundStack = (View as Views.RootView).soundStack;
                List<UserControls.SoundControl> soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    if (!soundControl.IsPlaying && soundControl.SoundPropertyModel.IsLoop)
                    {
                        // Don't want to accidentally restart a sound
                        soundControl.PlayOrStop();
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
                var soundStack = (View as Views.RootView).soundStack;
                List<UserControls.SoundControl> soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    if (soundControl.IsPlaying)
                    {
                        // Don't want to accidentally restart a sound
                        soundControl.PlayOrStop();
                    }
                }

                // And update the icon
                IsPlayingAll = false;
            }
        }

        public bool ContinueAfterOfferSave()
        {
            string fileName = Path.GetFileNameWithoutExtension(activeFilePath);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = "Untitled";
            }
            string message = $"Do you want to save changes to {fileName}?";
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

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                if (File.Exists(path))
                {
                    activeFilePath = path;
                    LoadWorkspace(activeFilePath);
                }
            }
        }

        public void SaveWorkspaceFile()
        {
            // We should save over the previously used file, if any
            if (string.IsNullOrEmpty(activeFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*";
                if ((bool)saveFileDialog.ShowDialog())
                {
                    activeFilePath = saveFileDialog.FileName;
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Workspace files (*.wsp)|*.wsp|All files (*.*)|*.*";
            if ((bool)saveFileDialog.ShowDialog())
            {
                activeFilePath = saveFileDialog.FileName;
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
                OpenFileDialog openFileDialog = new OpenFileDialog();

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    AddSound(filePath);
                }
            }
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
            AddSoundButton();
        }

        public void SelectScene_LeftMouseUp(object sender, RoutedEventArgs e)
        {
            string name = ((sender as Button)?.Content as UserControls.EditableTextBlock).Text;
            if (name != null)
            {
                SelectScene(name);
            }
        }

        public void SelectMood_LeftMouseUp(object sender, RoutedEventArgs e)
        {
            string name = ((sender as Button)?.Content as UserControls.EditableTextBlock).Text;
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
            string name = ((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock)?.Text;
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
            string name = ((((sender as MenuItem)?.Parent as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock)?.Text;
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
            Guid guid = soundControl.SoundPropertyModel.GUID;
            if (guid != null)
            {
                soundControl.Stop();
                RemoveSound(guid);
            }
        }

        public void PlayAllButton_Click(object sender, RoutedEventArgs e)
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

        public void ReportABug_Click(object sender, RoutedEventArgs e)
        {
            string user = "juanferrer";
            string repo = "sound-mixer";
            string body = System.Web.HttpUtility.UrlEncode("## Steps to reproduce:\n1. Step 1\n2. Step 2\n3. Step 3\n\n## Actual behaviour\n\n\n## Expected behaviour:\n\n\n## Other details:");
            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = $"https://github.com/{user}/{repo}/issues/new?body={body}",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }

        public void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutViewModel = new AboutViewModel();
            this.windowManager.ShowDialog(aboutViewModel);
        }

        #endregion
    }
}

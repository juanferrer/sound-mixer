using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SoundMixer.Models;
using SoundMixer.Extensions;

using Newtonsoft.Json;
using Stylet;
using System.Linq;

namespace SoundMixer.ViewModels
{
    public class RootViewModel : Screen
    {
        private static double defaultVolume = 0.5;
        private string activeFilePath;
        private bool isDirty = false;

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

        public RootViewModel()
        {
            Workspace = new WorkspaceModel();

            // Debug data
            // Create scenes
            Workspace.Scenes.Add(new SceneModel("Scene 1"));
            Workspace.Scenes.Add(new SceneModel("Scene 2"));

            // Create moods
            SelectScene("Scene 1");
            AddMood("Mood 1");
            AddMood("Mood 2");
            SelectScene("Scene 2");
            AddMood("Mood 3");

            // Create sounds
            SelectScene("Scene 1");
            AddSound(@".\1.wav");
            AddSound(@".\1.wav");
            SelectScene("Scene 2");
            AddSound(@".\1.wav");

            SelectScene("Scene 1");
            SelectMood("Mood 1");
        }

        /// <summary>
        /// Add a new scene
        /// </summary>
        /// <param name="sceneName"></param>
        public void AddScene(string sceneName)
        {
            Workspace.Scenes.Add(new SceneModel(sceneName));
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
                    Workspace.Scenes.RemoveAt(i);

                    // Select a different scene and mood if we deleted the selected one
                    if (SelectedScene.Name == sceneName)
                    {
                        SelectedScene = Workspace.Scenes.Count > 0 ? Workspace.Scenes?[0] : null;
                        SelectedMood = SelectedScene?.Moods.Count > 0 ? SelectedScene.Moods[0] : null;
                    }
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
            MoodModel newMood = new MoodModel(moodName);
            SelectedScene.Moods.Add(newMood);

            // Then add all the sounds from the scene
            foreach (var sound in SelectedScene.Sounds)
            {
                newMood.SoundProperties.Add(new SoundPropertyModel(defaultVolume, sound.GUID));
                newMood.SoundProperties.LastOrDefault().Sound = sound;
;            }
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
                        SelectedMood = SelectedScene.Moods.Count > 0 ? SelectedScene.Moods[0] : null;
                    }
                }
            }
        }

        /// <summary>
        /// Add a new sound to the selected scene
        /// </summary>
        /// <param name="soundPath"></param>
        public void AddSound(string soundPath)
        {
            string tempName = Path.GetFileNameWithoutExtension(soundPath);

            SoundModel newSound = new SoundModel(tempName, soundPath);
            SelectedScene.Sounds.Add(newSound);

            // Then add to every mood
            foreach (var mood in SelectedScene.Moods)
            {
                mood.SoundProperties.Add(new SoundPropertyModel(defaultVolume, newSound.GUID));

                SoundModel sound = FindSoundFromGuid(newSound.GUID);
                mood.SoundProperties.LastOrDefault().Sound = sound;
            }
        }

        /// <summary>
        /// Remove a sound from the selected mood in the selected scene
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
                }
            }

            // Then remove from every mood
            foreach (var mood in SelectedScene.Moods)
            {
                mood.SoundProperties.RemoveAt(soundIndex);
            }
        }

        public void SelectScene(string name)
        {
            StopAllSounds();

            for (int i = 0; i < Workspace.Scenes.Count; ++i)
            {
                if (Workspace.Scenes[i].Name == name)
                {
                    SelectedScene = Workspace.Scenes[i];
                }
            }

            SelectedMood = SelectedScene.Moods.Count > 0 ? SelectedScene.Moods[0] : null;
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

                    PlayAllSounds();
                }
            }

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
                    }
                }
            }
        }

        public SoundModel FindSoundFromGuid(Guid guid)
        {
            foreach (var scene in Workspace.Scenes)
            {
                //foreach (var sound in scene.Sounds)
                //{
                //    if (sound.GUID == guid)
                //    {
                //        return sound;
                //    }
                //}
                return scene.Sounds.Where(x => x.GUID == guid).FirstOrDefault();
            }

            // None matches
            return null;
        }

        public void PlayAllSounds()
        {
            if (View != null)
            {
                var soundStack = (View as Views.RootView).soundsStack;
                List<UserControls.SoundControl> soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    soundControl.Play();
                }
            }
        }

        public void StopAllSounds()
        {
            if (View != null)
            {
                var soundStack = (View as Views.RootView).soundsStack;
                List<UserControls.SoundControl> soundControls = soundStack.GetChildrenOfType<UserControls.SoundControl>();

                foreach (var soundControl in soundControls)
                {
                    soundControl.Stop();
                }
            }
        }

        #region EventHandlers

        public void New_Click(object sender, RoutedEventArgs e)
        {
            if (isDirty)
            {
                // Want to save?
                // TODO
            }
            Workspace = new WorkspaceModel();
            SelectedScene = null;
            SelectedMood = null;
        }

        public void Open_Click(object sender, RoutedEventArgs e)
        {
            if (isDirty)
            {
                // Want to save?
                // TODO
            }

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string path = openFileDialog.FileName;
                if (File.Exists(path))
                {
                    LoadWorkspace(path);
                }
            }
        }

        public void Save_Click(object sender, RoutedEventArgs e)
        {
            // We should save over the previously used file, if any
            if (string.IsNullOrEmpty(activeFilePath))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
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

        public void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            if ((bool)saveFileDialog.ShowDialog())
            {
                SaveWorkspace(saveFileDialog.FileName);
                isDirty = false;
            }
        }

        public void AddScene_Click(object sender, RoutedEventArgs e)
        {
            AddScene("New Scene");

            // Select scene if none selected
            if (SelectedScene == null)
            {
                SelectedScene = Workspace.Scenes[0];
            }
        }

        public void AddMood_Click(object sender, RoutedEventArgs e)
        {
            AddMood("New Mood");

            // Select mood if none selected
            if (SelectedMood == null)
            {
                SelectedMood = SelectedScene.Moods[0];
            }
        }

        public void AddSound_Click(object sender, RoutedEventArgs e)
        {
            // First open a file dialog to select the sound
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                AddSound(filePath);
            }
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
            var editableTextBlock = (((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as Button).Content as UserControls.EditableTextBlock;

            editableTextBlock.EnterEditMode();
        }

        public void CloneSceneButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void RemoveSceneButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ((((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock).Text;
            if (name != null)
            {
                RemoveScene(name);
            }
        }

        public void RenameMoodButton_Click(object sender, RoutedEventArgs e)
        {
            var editableTextBlock = (((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock;

            editableTextBlock.EnterEditMode();
        }

        public void CloneMoodButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void RemoveMoodButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ((((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as Button)?.Content as UserControls.EditableTextBlock).Text;
            if (name != null)
            {
                RemoveMood(name);
            }
        }

        public void RenameSoundControl_Click(object sender, RoutedEventArgs e)
        {
            var soundControl = (((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as UserControls.SoundControl);

            soundControl.EnterEditMode();
        }

        public void CloneSoundControl_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void RemoveSoundControl_Click(object sender, RoutedEventArgs e)
        {
            string name = (((sender as MenuItem)?.CommandParameter as ContextMenu)?.PlacementTarget as UserControls.SoundControl).Text;
            if (name != null)
            {
                RemoveSound(name);
            }
        }

        #endregion
    }
}

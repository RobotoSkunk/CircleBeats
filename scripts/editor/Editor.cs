/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2023 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using ClockBombGames.CircleBeats.IO;
using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class Editor : Control
	{
		[Export] Playground.Playground playground;

		[ExportGroup("Editor elements")]
		[Export] EditorTimeline timeline;

		[ExportGroup("Temporal debug stuff")]
		[Export] Label filenameLabel;
		[Export] Button openProjectButton;
		[Export] Button saveProjectButton;
		[Export] Button loadAudioFileButton;
		[Export] FileDialog audioFileDialog;
		[Export] FileDialog openProjectFileDialog;
		[Export] FileDialog saveProjectFileDialog;

		Window window;
		string projectFilePath;
		string audioFilePath;


		// Playground
		public Playground.Playground Playground => playground; // Playground playground? Playground.

		float timelineBodySizeBuffer;


		public override void _EnterTree()
		{
			window = GetViewport().GetWindow();

			timeline.OnResizeBody += ResizePlayground;
			window.SizeChanged += ResizePlaygroundImpl;

			openProjectButton.Pressed += OpenProjectHandler;
			saveProjectButton.Pressed += SaveProjectHandler;
			loadAudioFileButton.Pressed += OpenAudioFileHandler;

			openProjectFileDialog.FileSelected += OnProjectFileSelected;
			saveProjectFileDialog.FileSelected += OnProjectFileSaved;
			audioFileDialog.FileSelected += OnAudioFileSelected;
		}

		public override void _ExitTree()
		{
			timeline.OnResizeBody -= ResizePlayground;
			window.SizeChanged -= ResizePlaygroundImpl;

			openProjectButton.Pressed -= OpenProjectHandler;
			saveProjectButton.Pressed -= SaveProjectHandler;
			loadAudioFileButton.Pressed -= OpenAudioFileHandler;

			openProjectFileDialog.FileSelected -= OnProjectFileSelected;
			saveProjectFileDialog.FileSelected -= OnProjectFileSaved;
			audioFileDialog.FileSelected -= OnAudioFileSelected;
		}

		public override void _Process(double delta)
		{
		}


		#region Temporal debug stuff until a proper file handling system is created

		void OpenProjectHandler()
		{
			openProjectFileDialog.Popup();
		}

		void SaveProjectHandler()
		{
			if (string.IsNullOrEmpty(projectFilePath)) {
				saveProjectFileDialog.Popup();
			} else {
				FileManager.WriteLocalLevel(projectFilePath, audioFilePath);
			}
		}

		void OpenAudioFileHandler()
		{
			audioFileDialog.Popup();
		}

		void OnProjectFileSelected(string filepath)
		{
			try {
				projectFilePath = filepath;
				filenameLabel.Text = filepath;

				saveProjectButton.Disabled = false;

				audioFilePath = FileManager.ReadLocalLevel(filepath);
				OnAudioFileSelected(audioFilePath);
			} catch (Exception e) {
				GD.PrintErr(e);
			}
		}

		void OnProjectFileSaved(string filepath)
		{
			try {
				projectFilePath = filepath;
				filenameLabel.Text = filepath;

				FileManager.WriteLocalLevel(projectFilePath, audioFilePath);
			} catch (Exception e) {
				GD.PrintErr(e);
			}
		}

		void OnAudioFileSelected(string filepath)
		{
			try {
				var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);

				var audioStream = new AudioStreamMP3 {
					Data = file.GetBuffer((long)file.GetLength()),
				};

				file.Close();

				audioFilePath = filepath;
				saveProjectButton.Disabled = false;

				timeline.SetAudioStream(audioStream);
			} catch (Exception e) {
				GD.PrintErr(e);
			}
		}

		#endregion


		void ResizePlayground(float newSize)
		{
			timelineBodySizeBuffer = newSize;

			ResizePlaygroundImpl();
		}

		void ResizePlaygroundImpl()
		{
			Vector2 multiplier = Vector2.One * Mathf.Clamp((timelineBodySizeBuffer - 250f) / 225f, 0f, 1f) * 0.25f;
			Vector2 pivotOffset = playground.Size / 2f;

			pivotOffset.Y -= playground.Size.Y * multiplier.Y;

			playground.PivotOffset = pivotOffset;
			playground.Scale = Vector2.One - multiplier;
		}
	}
}

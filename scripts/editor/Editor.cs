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
using Godot;


namespace ClockBombGames.CircleBeats.Editor
{
	public partial class Editor : Control
	{
		[Export] Playground.Playground playground;

		[ExportGroup("Editor elements")]
		[Export] EditorTimeline timeline;
		[Export] FileDialog fileDialog;


		Window window;


		// Playground
		public Playground.Playground Playground => playground; // Playground playground? Playground.

		float timelineBodySizeBuffer;


		public override void _Ready()
		{
			fileDialog.Popup();
		}

		public override void _EnterTree()
		{
			window = GetViewport().GetWindow();

			timeline.OnResizeBody += ResizePlayground;
			window.SizeChanged += ResizePlaygroundImpl;
			fileDialog.FileSelected += OnFileSelected;
			fileDialog.Canceled += OnDialogCancel;
		}

		public override void _ExitTree()
		{
			timeline.OnResizeBody -= ResizePlayground;
			window.SizeChanged -= ResizePlaygroundImpl;
		}

		
		void OnFileSelected(string filepath)
		{
			try {
				var file = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);

				var audioStream = new AudioStreamMP3 {
					Data = file.GetBuffer((long)file.GetLength()),
				};

				file.Close();
				timeline.SetAudioStream(audioStream);

			} catch (Exception e) {
				GD.PrintErr(e);
				fileDialog.Popup();
			}
		}

		void OnDialogCancel()
		{
			fileDialog.Popup();
		}


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

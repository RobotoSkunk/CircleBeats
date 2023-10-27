/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2023 Edgar Lima <contact@robotoskunk.com>

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


using Godot;
using Godot.Collections;


namespace ClockBombGames.CircleBeats.Analyzers
{
	public class AudioStreamReader
	{
		readonly AudioStream stream;
		byte[] data;

		float avgDecibels = -1f;


		public AudioStreamReader(AudioStream stream) {
			this.stream = stream;
		}


		public void ReadNonAlloc(ref byte[] buffer)
		{
			if (stream is AudioStreamMP3 mp3) {
				buffer = mp3.Data;

			} else if (stream is AudioStreamWav wav) {
				buffer = wav.Data;

			} else if (stream is AudioStreamOggVorbis ogg) {
				long[] data = ogg.PacketSequence.GranulePositions;
				// ?????????????????????????????????

				for (int i = 0; i < data.Length; i++) {
					GD.Print(data[i]);
				}

			} else {
				buffer = null;
			}
		}


		public float AverageDecibels
		{
			get {
				if (avgDecibels != -1f) {
					return avgDecibels;
				}

				


				return avgDecibels;
			}
		}
	}
}

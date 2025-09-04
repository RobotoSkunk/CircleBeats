/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2025 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

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
using System.IO;
using System.Xml;


namespace ClockBombGames.CircleBeats.IO
{
	public static class FileManager
	{
		public static string ReadLocalLevel(string filepath)
		{
			XmlDocument doc = new();
			doc.Load(filepath);

			XmlNode attribute = doc.DocumentElement.SelectSingleNode("/data/song");

			return attribute.InnerText;
		}

		public static void WriteLocalLevel(string filepath, string songPath)
		{
			XmlDocument doc = new();
			XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
			XmlElement root = doc.DocumentElement;

			doc.InsertBefore(xmlDeclaration, root);


			XmlElement dataNode = doc.CreateElement(string.Empty, "data", string.Empty);
			doc.AppendChild(dataNode);

			XmlElement attribute = doc.CreateElement(string.Empty, "song", string.Empty);
			attribute.InnerText = songPath;

			dataNode.AppendChild(attribute);

			doc.Save(filepath);
		}

		public static bool LocalLevelExists(string filepath) {
			return File.Exists(filepath);
		}



		public static class Paths
		{
			static string _dataDir = null;
			public static string DataDir
			{
				get
				{
					_dataDir ??= OS.GetUserDataDir();
					GD.Print(_dataDir);
					return _dataDir;
				}
			}
		}
	}
}

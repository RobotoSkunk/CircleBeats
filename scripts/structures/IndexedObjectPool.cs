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


using System.Collections.Generic;

using Godot;


namespace ClockBombGames.CircleBeats.Structures
{
	/// <summary>
	/// An useful class to do object pooling with different nodes that inherits from the same class.
	/// </summary>
	/// <typeparam name="T">
	/// The type of node that will be stored.
	/// It's suggested to point to the base class of all nodes.
	/// </typeparam>
	public class IndexedObjectPool<T> where T : Node, IForIndexedObjectPool
	{
		readonly Dictionary<int, Queue<T>> availableObjects = [];
		readonly PackedScene[] packedScenes;


		/// <summary>
		/// Creates a new IndexedObjectPool with a list of all available packedScenes to use.
		/// </summary>
		/// <param name="packedScenes">The packed scene references to instantiate when required.</param>
		public IndexedObjectPool(PackedScene[] packedScenes)
		{
			this.packedScenes = packedScenes;

			// To avoid a complexity of O(n²) when allocating the queues in the next line.
			availableObjects.EnsureCapacity(packedScenes.Length);

			for (int i = 0; i < packedScenes.Length; i++) {
				availableObjects.Add(i, new());
			}
		}


		/// <summary>
		/// Returns a new or reused requested node.
		/// </summary>
		/// <param name="index">The indexed group where the requested node is from.</param>
		/// <param name="parent">The parent node where it'll be added to.</param>
		/// <returns>The requested scene, either reused or instantiated.</returns>
		public T RequestScene(int index, Node parent)
		{
			T scene;

			if (availableObjects[index].Count == 0) {
				scene = packedScenes[index].Instantiate<T>();
				scene.PoolIndex = index;

			} else {
				scene = availableObjects[index].Dequeue();
			}

			parent.AddChild(scene);

			return scene;
		}


		/// <summary>
		/// Safely removes a scene from the nodes tree and returns it to the available objects queue.
		/// </summary>
		/// <param name="scene">The scene to remove.</param>
		public void RemoveScene(T scene)
		{
			Node parent = scene.GetParent();

			if (parent == null) {
				return;
			}

			parent.RemoveChild(scene);

			availableObjects[scene.PoolIndex].Enqueue(scene);
		}


		/// <summary>
		/// Deletes all nodes from the memory.
		/// </summary>
		public void FreeAll()
		{
			foreach (Queue<T> queue in availableObjects.Values) {
				foreach (T node in queue) {
					node.QueueFree();
				}

				queue.Clear();
			}
		}
	}


	/// <summary>
	/// Ensures that the object is suitable to be used in an indexed object pool.
	/// </summary>
	public interface IForIndexedObjectPool
	{
		/// <summary>
		/// Used by IndexedObjectPool, any pre-defined value will be ignored.
		/// <b>Do not edit this variable in runtime.</b>
		/// </summary>
		public int PoolIndex { get; set; }
	}
}

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
	/// Creates a new timeline that can use different types of nodes with the same base node.
	/// </summary>
	/// <typeparam name="TParameters">The base class for all parameters to be passed on each node.</typeparam>
	/// <typeparam name="TScene">The base class of the nodes that will be stored in the tree.</typeparam>
	/// <param name="packedScenes">The packed scene references to instantiate when required.</param>
	public class ObjectTimeline<TParameters, TScene>(PackedScene[] packedScenes) : IntervalTree<TParameters>
		where TScene : Node, IForIndexedObjectPool, IForTimeline<TParameters>
	{
		readonly IndexedObjectPool<TScene> objectPool = new(packedScenes);


		// Declared here to avoid GC
		readonly HashSet<NodeTimeline> nodesInUse = [];

		// Nodes to be deleted in next ticks
		readonly HashSet<NodeTimeline> toBeRemoved = [];


		public void GetTime(float time)
		{
			// Find all required nodes
			Root.FindIntersectInterval(time, (nodeFound) =>
			{
				NodeTimeline node = (NodeTimeline)nodeFound;

				if (node.Scene == null) {
					node.Scene = objectPool.RequestScene(node.PoolIndex, node.ParentTarget);

					node.Scene.SetParameters(node.Value);
				}

				node.Scene.ExecuteTime(time);


				// To add it later to the removed nodes
				nodesInUse.Add(node);

				// Ignore it because it's being in use
				toBeRemoved.Remove(node);
			});


			// Remove all unused nodes in this tick
			foreach (NodeTimeline node in toBeRemoved) {
				objectPool.RemoveScene(node.Scene);

				node.Scene = null;
			}


			// Clear and add the current nodes in use to remove them in the next tick
			toBeRemoved.Clear();
			toBeRemoved.UnionWith(nodesInUse);

			// Clear the nodesInUse list
			nodesInUse.Clear();
		}


		public void FreeAll()
		{
			objectPool.FreeAll();
		}



		public class NodeTimeline : TreeNode
		{
			public NodeTimeline(float start, float end, TParameters parameters) : base(start, end, parameters) { }
			public NodeTimeline(Interval<TParameters> interval) : base(interval) { }

			/// <summary>
			/// Tells which packed scene has to be used.
			/// </summary>
			public int PoolIndex { get; set; }

			/// <summary>
			/// Tells which node will be the scene added to.
			/// </summary>
			public Node ParentTarget { get; set; }

			/// <summary>
			/// Used by ObjectTimeline, <b>Do not edit this variable in runtime.</b>
			/// </summary>
			public TScene Scene { get; set; }
		}
	}

	/// <summary>
	/// Ensures that the object is suitable to be used in an object timeline.
	/// </summary>
	/// <typeparam name="TParameters">
	/// The custom class of parameters to execute when the timeline requests a given time
	/// between 0 and 1 (inclusive).
	/// </typeparam>
	public interface IForTimeline<TParameters>
	{
		/// <summary>
		/// The paramenters to do something with the current object with a time interval between 0 and 1 (inclusive).
		/// </summary>
		public virtual void SetParameters(TParameters parameters) { }

		/// <summary>
		/// Called by ObjectTimeline.
		/// Modifies the object transform with a given time interval based on the
		/// parameters received by SetParameters().
		/// </summary>
		public virtual void ExecuteTime(float time) { }
	}
}

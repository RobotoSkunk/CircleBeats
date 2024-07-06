/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2024 Edgar Lima (RobotoSkunk) <contact@robotoskunk.com>

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
	/// A structure of an interval tree. This structure is used to store intervals and retrieve them efficiently.
	/// </summary>
	public class IntervalTree<TValue>
	{
		public class Node
		{
			public Interval<TValue> Interval { get; private set; }

			public Node Left  { get; private set; }
			public Node Right  { get; private set; }
			public float Max  { get; private set; }

			public TValue Value => Interval.Value;

			float Start => Interval.Start;
			float End   => Interval.End;



			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(float start, float end, TValue value)
			{
				Interval = new Interval<TValue>(start, end, value);
			}

			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(Interval<TValue> interval)
			{
				Interval = interval;
			}

			/// <summary>
			/// Creates a new balanced branch with the specified intervals.
			/// </summary>
			public Node(Interval<TValue>[] intervals)
			{
				if (intervals.Length == 0) {
					return;
				}

				Interval = intervals[0];
				Max = Interval.End;

				if (intervals.Length == 1) {
					return;
				}


				int middleIndex = intervals.Length / 2;

				var left = intervals[..middleIndex];
				var right = intervals[middleIndex..];


				if (left.Length > 0) {
					Left = new Node(left);
				}

				if (right.Length > 0) {
					Right = new Node(right);
				}

				if (Left != null) {
					Max = Mathf.Max(Max, Left.Max);
				}

				if (Right != null) {
					Max = Mathf.Max(Max, Right.Max);
				}
			}


			public int CompareTo(Node other)
			{
				return Interval.CompareTo(other.Interval);
			}


			/// <summary>
			/// Adds a new node to the tree.
			/// </summary>
			public Node AddNode(Node newNode)
			{
				if (newNode.End > Max) {
					Max = newNode.End;
				}

				if (CompareTo(newNode) <= 0) {
					if (Right == null) {
						Right = newNode;
					} else {
						Right.AddNode(newNode);
					}
				} else {
					if (Left == null) {
						Left = newNode;
					} else {
						Left.AddNode(newNode);
					}
				}


				return this;
			}



			/// <summary>
			/// Searches for an interval that contains the given time.
			/// </summary>
			public Node FindInterval(float time)
			{
				if (Interval.HasTime(time)) {
					return this;
				}

				if (Left != null && Left.Max >= time) {
					Left.FindInterval(time);
				}

				if (Start <= time) {
					Right?.FindInterval(time);
				}

				return null;
			}

			/// <summary>
			/// Searches for an interval that contains the given time.
			/// </summary>
			public void FindIntersectInterval(float time, List<Node> result)
			{
				if (Interval.HasTime(time)) {
					result.Add(this);
				}

				if (Left != null && Left.Max >= time) {
					Left.FindIntersectInterval(time, result);
				}

				if (Start > time) {
					return;
				}

				Right?.FindIntersectInterval(time, result);
			}
		}

		public Node Root { get; private set; }
		Node _temporalNode;

		/// <summary>
		/// Creates a new empty interval tree.
		/// </summary>
		public IntervalTree() { }

		/// <summary>
		/// Creates a balanced interval tree from a given list.
		/// </summary>
		public IntervalTree(List<Interval<TValue>> intervals)
		{
			intervals.Sort((a, b) => a.CompareTo(b));
			Root = new Node(intervals.ToArray());
		}


		/// <summary>
		/// Searches for an interval that contains the given scalar. If there's a cached node and it contains
		/// the given scalar, it will be used instead.
		/// </summary>
		public Node Search(float time)
		{
			if (_temporalNode != null) {

				if (_temporalNode.Interval.HasTime(time)) {
					return _temporalNode;
				} else {
					_temporalNode = null;
				}
			}


			_temporalNode = ForceSearch(time);

			return _temporalNode;
		}

		/// <summary>
		/// Forces the tree to search for an interval that contains the given scalar, ignoring any cached node.
		/// </summary>
		public Node ForceSearch(float time)
		{
			return Root.FindInterval(time);
		}



		/// <summary>
		/// Adds an interval to the tree like a binary search tree.
		/// </summary>
		public void Add(Interval<TValue> interval)
		{
			Root?.AddNode(new Node(interval));
		}
	}
}

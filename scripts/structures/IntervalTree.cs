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


using System;
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
			public Node Right { get; private set; }

			public TValue Value => Interval.Value;

			float Start => Interval.Start;
			float End   => Interval.End;

			int Height { get; set; }
			float Max  { get; set; }


			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(float start, float end, TValue value)
			{
				Interval = new Interval<TValue>(start, end, value);
				Max = end;
			}

			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(Interval<TValue> interval)
			{
				Interval = interval;
				Max = interval.End;
			}

			private static int GetHeight(Node node)
			{
				if (node == null) {
					return 0;
				}

				return node.Height;
			}

			private static float GetMax(Node node)
			{
				if (node == null) {
					return int.MinValue;
				}

				return node.Max;
			}

			private int GetBalance()
			{
				return GetHeight(Left) - GetHeight(Right);
			}

			private void SetMax()
			{
				Max = Mathf.Max(End, Mathf.Max(GetMax(Left), GetMax(Right)));
			}


			private static Node RightRotate(Node y)
			{
				Node x = y?.Left;
				Node T2 = x?.Right;

				if (x == null && T2 == null) {
					return y;
				}

				x.Right = y;
				y.Left = T2;

				y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));
				x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));

				y.SetMax();
				x.SetMax();

				return x;
			}

			private static Node LeftRotate(Node x)
			{
				Node y = x?.Right;
				Node T2 = y?.Left;

				if (y == null && T2 == null) {
					return x;
				}

				y.Left = x;
				x.Right = T2;

				x.Height = 1 + Math.Max(GetHeight(x.Left), GetHeight(x.Right));
				y.Height = 1 + Math.Max(GetHeight(y.Left), GetHeight(y.Right));

				x.SetMax();
				y.SetMax();

				return y;
			}




			public float CompareTo(Node other)
			{
				return Interval.CompareTo(other.Interval);
			}


			/// <summary>
			/// Inserts a new node to the tree.
			/// <br/><br/>
			/// Complexity: O(log n)
			/// </summary>
			public static Node Insert(Node node, Interval<TValue> key)
			{
				if (node == null) {
					return new Node(key);
				}

				if (key.CompareTo(node.Interval) < 0) {
					node.Left = Insert(node.Left, key);

				} else {
					node.Right = Insert(node.Right, key);
				}

				node.Height = 1 + Mathf.Max(GetHeight(node.Left), GetHeight(node.Right));
				node.SetMax();


				int balance = node.GetBalance();


				// Left Left case
				if (balance > 1 && key.CompareTo(node.Left.Interval) < 0) {
					return RightRotate(node);
				}

				// Right Right case
				if (balance < -1 && key.CompareTo(node.Right.Interval) >= 0) {
					return LeftRotate(node);
				}

				// Left Right case
				if (balance > 1 && key.CompareTo(node.Left.Interval) > 0) {
					node.Left = LeftRotate(node.Left);
					return RightRotate(node);
				}

				// Right Left case
				if (balance < -1 && key.CompareTo(node.Right.Interval) < 0) {
					node.Right = RightRotate(node.Right);
					return LeftRotate(node);
				}

				return node;
			}



			/// <summary>
			/// Searches for an interval that contains the given time.
			/// <br/><br/>
			/// Complexity: O(log n)
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
			/// Searches for all intervals that contains the given time and executes an action on each one.
			/// <br/><br/>
			/// Complexity: O(log n + k)
			/// </summary>
			public void FindIntersectInterval(float time, Action<Node> action)
			{
				if (Interval.HasTime(time)) {
					action(this);
				}

				if (Left != null && Left.Max >= time) {
					Left.FindIntersectInterval(time, action);
				}

				if (Start <= time) {
					Right?.FindIntersectInterval(time, action);
				}
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
			for (int i = 0; i < intervals.Count; i++) {
				Add(intervals[i]);
			}
		}


		/// <summary>
		/// Searches for an interval that contains the given scalar. If there's a cached node and it contains
		/// the given scalar, it will be used instead.
		/// <br/><br/>
		/// Complexity: O(1) Best, O(log n) Worst
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
		/// <br/><br/>
		/// Complexity: O(log n)
		/// </summary>
		public Node ForceSearch(float time)
		{
			return Root.FindInterval(time);
		}



		/// <summary>
		/// Adds an interval to the tree like a binary search tree.
		/// <br/><br/>
		/// Complexity: O(log n)
		/// </summary>
		public void Add(Interval<TValue> interval)
		{
			Root = Node.Insert(Root, interval);
		}
	}
}

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
		public class TreeNode
		{
			public Interval<TValue> Interval { get; private set; }

			public TreeNode Left  { get; private set; }
			public TreeNode Right { get; private set; }

			public TValue Value => Interval.Value;

			float Start => Interval.Start;
			float End   => Interval.End;

			int Height { get; set; }
			float Max  { get; set; }


			/// <summary>
			/// Creates a new TreeNode with the specified interval.
			/// </summary>
			public TreeNode(float start, float end, TValue value)
			{
				Interval = new Interval<TValue>(start, end, value);
				Max = end;
			}

			/// <summary>
			/// Creates a new TreeNode with the specified interval.
			/// </summary>
			public TreeNode(Interval<TValue> interval)
			{
				Interval = interval;
				Max = interval.End;
			}

			private static int GetHeight(TreeNode TreeNode)
			{
				if (TreeNode == null) {
					return 0;
				}

				return TreeNode.Height;
			}

			private static float GetMax(TreeNode TreeNode)
			{
				if (TreeNode == null) {
					return int.MinValue;
				}

				return TreeNode.Max;
			}

			private int GetBalance()
			{
				return GetHeight(Left) - GetHeight(Right);
			}

			private void SetMax()
			{
				Max = Mathf.Max(End, Mathf.Max(GetMax(Left), GetMax(Right)));
			}


			private static TreeNode RightRotate(TreeNode y)
			{
				TreeNode x = y?.Left;
				TreeNode T2 = x?.Right;

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

			private static TreeNode LeftRotate(TreeNode x)
			{
				TreeNode y = x?.Right;
				TreeNode T2 = y?.Left;

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




			public float CompareTo(TreeNode other)
			{
				return Interval.CompareTo(other.Interval);
			}


			/// <summary>
			/// Inserts a new TreeNode to the tree.
			/// <br/><br/>
			/// Complexity: O(log n)
			/// </summary>
			public static TreeNode Insert(TreeNode TreeNode, TreeNode keyNode)
			{
				if (TreeNode == null) {
					return keyNode;
				}

				if (keyNode.CompareTo(TreeNode) < 0) {
					TreeNode.Left = Insert(TreeNode.Left, keyNode);

				} else {
					TreeNode.Right = Insert(TreeNode.Right, keyNode);
				}

				TreeNode.Height = 1 + Mathf.Max(GetHeight(TreeNode.Left), GetHeight(TreeNode.Right));
				TreeNode.SetMax();


				int balance = TreeNode.GetBalance();


				// Left Left case
				if (balance > 1 && keyNode.CompareTo(TreeNode.Left) < 0) {
					return RightRotate(TreeNode);
				}

				// Right Right case
				if (balance < -1 && keyNode.CompareTo(TreeNode.Right) >= 0) {
					return LeftRotate(TreeNode);
				}

				// Left Right case
				if (balance > 1 && keyNode.CompareTo(TreeNode.Left) > 0) {
					TreeNode.Left = LeftRotate(TreeNode.Left);
					return RightRotate(TreeNode);
				}

				// Right Left case
				if (balance < -1 && keyNode.CompareTo(TreeNode.Right) < 0) {
					TreeNode.Right = RightRotate(TreeNode.Right);
					return LeftRotate(TreeNode);
				}

				return TreeNode;
			}



			/// <summary>
			/// Searches for an interval that contains the given time.
			/// <br/><br/>
			/// Complexity: O(log n)
			/// </summary>
			public TreeNode FindInterval(float time)
			{
				if (Interval.HasTime(time)) {
					return this;
				}

				if (Left != null && Left.Max >= time) {
					return Left.FindInterval(time);
				}

				if (Start <= time) {
					return Right?.FindInterval(time);
				}

				return null;
			}

			/// <summary>
			/// Searches for all intervals that contains the given time and executes an action on each one.
			/// <br/><br/>
			/// Complexity: O(log n + k)
			/// </summary>
			public void FindIntersectInterval(float time, Action<TreeNode> action)
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

		public TreeNode Root { get; protected set; }
		TreeNode _temporalNode;

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
		/// Searches for an interval that contains the given scalar. If there's a cached TreeNode and it contains
		/// the given scalar, it will be used instead.
		/// <br/><br/>
		/// Complexity: O(1) Best, O(log n) Worst
		/// </summary>
		public TreeNode Search(float time)
		{
			if (_temporalNode != null) {

				if (_temporalNode.Interval.HasTime(time)) {
					return _temporalNode;
				}

				_temporalNode = null;
			}


			_temporalNode = ForceSearch(time);

			return _temporalNode;
		}

		/// <summary>
		/// Forces the tree to search for an interval that contains the given scalar, ignoring any cached TreeNode.
		/// <br/><br/>
		/// Complexity: O(log n)
		/// </summary>
		public TreeNode ForceSearch(float time)
		{
			if (Root == null) {
				return null;
			}

			return Root.FindInterval(time);
		}



		/// <summary>
		/// Adds an interval to the tree like a binary search tree.
		/// <br/><br/>
		/// Complexity: O(log n)
		/// </summary>
		public void Add(Interval<TValue> interval)
		{
			TreeNode keyNode = new(interval);

			Root = TreeNode.Insert(Root, keyNode);
		}
	}
}

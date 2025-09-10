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

			private static int GetHeight(TreeNode root)
			{
				if (root == null) {
					return 0;
				}

				return root.Height;
			}

			private static float GetMax(TreeNode root)
			{
				if (root == null) {
					return int.MinValue;
				}

				return root.Max;
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


			private static TreeNode GetLeftmostLeaf(TreeNode root)
			{
				TreeNode current = root;

				while (current.Left != null) {
					current = current.Left;
				}

				return current;
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
			public static TreeNode Insert(TreeNode root, TreeNode keyNode)
			{
				if (root == null) {
					return keyNode;
				}

				if (keyNode.CompareTo(root) < 0) {
					root.Left = Insert(root.Left, keyNode);

				} else {
					root.Right = Insert(root.Right, keyNode);
				}

				root.Height = 1 + Mathf.Max(GetHeight(root.Left), GetHeight(root.Right));
				root.SetMax();


				int balance = root.GetBalance();


				// Left Left case
				if (balance > 1 && keyNode.CompareTo(root.Left) < 0) {
					return RightRotate(root);
				}

				// Right Right case
				if (balance < -1 && keyNode.CompareTo(root.Right) >= 0) {
					return LeftRotate(root);
				}

				// Left Right case
				if (balance > 1 && keyNode.CompareTo(root.Left) > 0) {
					root.Left = LeftRotate(root.Left);
					return RightRotate(root);
				}

				// Right Left case
				if (balance < -1 && keyNode.CompareTo(root.Right) < 0) {
					root.Right = RightRotate(root.Right);
					return LeftRotate(root);
				}

				return root;
			}


			/// <summary>
			/// Deletes an interval from the tree.
			/// <br/><br/>
			/// Complexity: O(log n)
			/// </summary>
			public static TreeNode Delete(TreeNode root, Interval<TValue> interval)
			{
				if (root == null) {
					return null;
				}

				int comparison = interval.CompareTo(root.Interval);

				if (comparison < 0) {
					root.Left = Delete(root.Left, interval);

				} else if (comparison > 0 || !root.Interval.ID.Equals(interval.ID)) {
					root.Right = Delete(root.Right, interval);

				} else {
					if (root.Left == null || root.Right == null) {
						TreeNode temp = root.Left ?? root.Right;

						if (temp == null) {
							root = null;
						} else {
							root = temp;
						}
					} else {
						TreeNode temp = GetLeftmostLeaf(root.Right);

						root.Interval = temp.Interval;
						root.Right = Delete(root.Right, temp.Interval);
					}
				}


				// If the root is null, this means that a leaf was deleted. Self-balance is not needed.
				if (root == null) {
					return null;
				}

				root.Height = 1 + Mathf.Max(GetHeight(root.Left), GetHeight(root.Right));
				root.SetMax();


				int balance = root.GetBalance();


				// Left Left case
				if (balance > 1 && root.Left.GetBalance() >= 0) {
					return RightRotate(root);
				}

				// Left Right case
				if (balance > 1 && root.Left.GetBalance() < 0) {
					root.Left = LeftRotate(root.Left);
					return RightRotate(root);
				}

				// Right Right case
				if (balance < -1 && root.Right.GetBalance() <= 0) {
					return LeftRotate(root);
				}

				// Right Left case
				if (balance < -1 && root.Right.GetBalance() > 0) {
					root.Right = RightRotate(root.Right);
					return LeftRotate(root);
				}

				return root;
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


		/// <summary>
		/// Deletes an interval to the tree.
		/// <br/><br/>
		/// Complexity: O(log n)
		/// </summary>
		public void Delete(Interval<TValue> interval)
		{
			Root = TreeNode.Delete(Root, interval);
		}
	}
}

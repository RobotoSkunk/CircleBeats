/*
	CircleBeats, a bullet hell rhythmic game.
	Copyright (C) 2024 Edgar Lima <contact@robotoskunk.com>

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
	/// <para>Note: This structure can't retrieve intervals that are partially overlapping.</para>
	/// </summary>
	public class IntervalTree<TValue>
	{
		public class Node
		{
			public Interval<TValue> interval => _interval;
			public Node left => _left;
			public Node right => _right;
			public float max => _max;
			public TValue value => _interval.value;

			Interval<TValue> _interval;
			Node _left;
			Node _right;
			float _max;


			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(float start, float end, TValue value)
			{
				_interval = new Interval<TValue>(start, end, value);
			}

			/// <summary>
			/// Creates a new node with the specified interval.
			/// </summary>
			public Node(Interval<TValue> interval)
			{
				this._interval = interval;
			}

			/// <summary>
			/// Creates a new balanced branch with the specified intervals.
			/// </summary>
			public Node(Interval<TValue>[] intervals)
			{
				if (intervals.Length == 0) {
					return;
				}

				_interval = intervals[0];
				_max = _interval.end;

				if (intervals.Length == 1) {
					return;
				}


				int middleIndex = intervals.Length / 2;

				var left = intervals[..middleIndex];
				var right = intervals[middleIndex..];


				if (left.Length > 0) {
					this._left = new Node(left);
				}

				if (right.Length > 0) {
					this._right = new Node(right);
				}

				if (this._left != null) {
					_max = Mathf.Max(_max, this._left.max);
				}

				if (this._right != null) {
					_max = Mathf.Max(_max, this._right.max);
				}
			}



			/// <summary>
			/// Adds an interval to the tree like a binary search tree.
			/// It should be only called if you're f*cked up with the tree.
			/// </summary>
			public void ForceAdd(Interval<TValue> interval)
			{
				if (interval.start < this._interval.start) {

					if (_left == null) {
						_left = new Node(interval);
					} else {
						_left.ForceAdd(interval);
					}

				} else {
					if (_right == null) {
						_right = new Node(interval);
					} else {
						_right.ForceAdd(interval);
					}
				}
			}

			/// <summary>
			/// Searches for an interval that contains the given interval.
			/// </summary>
			public Node Search(float start, float end)
			{
				if (_interval.Intersects(start, end)) {
					return this;
				}

				if (_left != null && _left.max >= start) {
					return _left.Search(start, end);
				}

				if (_right != null) {
					return _right.Search(start, end);
				}

				return null;
			}

			/// <summary>
			/// Searches for an interval that contains the given scalar.
			/// </summary>
			public Node Search(float x) {
				return Search(x, x);
			}
		}

		public Node root => _root;
		Node _root;
		Node _temporalNode;

		List<Interval<TValue>> _intervals = new();


		/// <summary>
		/// Creates a new empty interval tree.
		/// </summary>
		public IntervalTree() { }

		/// <summary>
		/// Creates a new interval tree with the specified intervals. Remember to call Build() after this.
		/// </summary>
		public IntervalTree(Interval<TValue>[] intervals)
		{
			_intervals = new List<Interval<TValue>>(intervals);
		}


		/// <summary>
		/// Builds the tree from the given intervals. You feel like a charm, the birds sing and the sun shines.
		/// </summary>
		public void Build()
		{
			_root = null;

			_intervals.Sort((a, b) => a.start.CompareTo(b.start));
			_root = new Node(_intervals.ToArray());
		}


		/// <summary>
		/// Forces the tree to search for an interval that contains the given interval instead of using
		/// the cached node.
		/// </summary>
		public Node ForceSearch(float start, float end)
		{
			return _root.Search(start, end);
		}

		/// <summary>
		/// Forces the tree to search for an interval that contains the given scalar instead of using
		/// the cached node.
		/// </summary>
		public Node ForceSearch(float x)
		{
			return _root.Search(x);
		}


		/// <summary>
		/// Searches for an interval that contains the given interval.
		/// </summary>
		public Node Search(float start, float end)
		{
			if (_temporalNode != null) {

				if (_temporalNode.interval.Intersects(start, end)) {
					return _temporalNode;
				} else {
					_temporalNode = null;
				}
			}


			_temporalNode = ForceSearch(start, end);

			return _temporalNode;
		}

		/// <summary>
		/// Searches for an interval that contains the given scalar.
		/// </summary>
		public Node Search(float x)
		{
			return Search(x, x);
		}



		/// <summary>
		/// Adds an interval to the tree like a binary search tree.
		/// Note: it only works if the tree is already built.
		/// </summary>
		public void Add(Interval<TValue> interval)
		{
			_intervals.Add(interval);

			if (_root != null) {
				_root.ForceAdd(interval);
			}
		}

		/// <summary>
		/// Adds an interval to the tree like a binary search tree.
		/// It should be only called if you're f*cked up with the tree.
		/// </summary>
		public void ForceAdd(Interval<TValue> interval)
		{
			_intervals.Add(interval);

			if (_root != null) {
				_root.ForceAdd(interval);
			} else {
				_root = new Node(interval);
			}
		}
	}
}

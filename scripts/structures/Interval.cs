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
using System;


namespace ClockBombGames.CircleBeats.Structures
{
	interface IInterval
	{
		float Start { get; }
		float End { get; }
		float Zoom { get; }

		public bool HasTime(float time);
	}

	interface IInterval<TValue> : IInterval
	{
		public TValue Value { get; }
	}


	/// <summary>
	/// A structure that represents a range of values and contains a value.
	/// </summary>
	public struct Interval<TValue> : IInterval<TValue>, IEquatable<Interval<TValue>>
	{
		public TValue Value { get; private set; }
		public float Start { get; private set; }
		public float End { get; private set; }
		public readonly float Zoom => End - Start;


		public Interval(float start, float end, TValue value)
		{
			if (start > end) {
				throw new ArgumentException("Start must be less than or equal to end.");
			}

			Start = start;
			End = end;
			Value = value;
		}

		public Interval(Interval interval, TValue value)
		{
			Start = interval.Start;
			End = interval.End;
			Value = value;
		}


		public readonly bool HasTime(float time)
		{
			return Start <= time && End >= time;
		}


		public readonly int CompareTo(Interval<TValue> other)
		{
			if (Start < other.Start) {
				return -1;
			}

			if (Start == other.Start) {
				if (End < other.End) {
					return -1;
				}
				
				if (End == other.End) {
					return 0;
				}
			}

			return 1;
		}


		public readonly bool Equals(Interval<TValue> other)
		{
			return Start == other.Start
				&& End == other.End
				&& EqualityComparer<TValue>.Default.Equals(Value, other.Value);
		}

		public readonly override bool Equals(object obj)
		{
			return obj is Interval<TValue> interval && Equals(interval);
		}

		public readonly override int GetHashCode()
		{
			return HashCode.Combine(Start, End, Value);
		}

		public readonly override string ToString()
		{
			return string.Concat("([", Start.ToString(), ", ", End.ToString(), "] ", Value.ToString(), ")");
		}

		public static bool operator ==(Interval<TValue> a, Interval<TValue> b)
		{
			return a.Start == b.Start
				&& a.End == b.End
				&& a.Value.Equals(b.Value);
		}

		public static bool operator !=(Interval<TValue> a, Interval<TValue> b)
		{
			return !(a == b);
		}
	}



	/// <summary>
	/// A structure that represents a range of values.
	/// </summary>
	public struct Interval : IInterval, IEquatable<Interval>
	{
		public float Start { get; private set; }
		public float End { get; private set; }
		public readonly float Zoom => End - Start;


		public Interval(float start, float end)
		{
			if (start > end) {
				throw new ArgumentException("Start must be less than or equal to end.");
			}

			Start = start;
			End = end;
		}


		public readonly bool HasTime(float time)
		{
			return Start <= time && End >= time;
		}

		public readonly int CompareTo(Interval other)
		{
			if (Start < other.Start) {
				return -1;
			}

			if (Start == other.Start) {
				if (End < other.End) {
					return -1;
				}
				
				if (End == other.End) {
					return 0;
				}
			}

			return 1;
		}


		public readonly bool Equals(Interval other)
		{
			return Start == other.Start
				&& End == other.End;
		}

		public readonly override bool Equals(object obj)
		{
			return obj is Interval interval && Equals(interval);
		}

		public readonly override int GetHashCode()
		{
			return HashCode.Combine(Start, End);
		}

		public readonly override string ToString()
		{
			return string.Concat("([", Start.ToString(), ", ", End.ToString(), "])");
		}


		public static bool operator ==(Interval a, Interval b)
		{
			return a.Start == b.Start
				&& a.End == b.End;
		}

		public static bool operator !=(Interval a, Interval b)
		{
			return !(a == b);
		}
	}
}

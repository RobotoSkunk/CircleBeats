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

	interface IInterval<T> : IInterval
	{
		public T Value { get; }
	}


	/// <summary>
	/// A structure that represents a range of values.
	/// </summary>
	public class Interval : IInterval, IEquatable<Interval>
	{
		public float Start { get; private set; }
		public float End { get; private set; }
		public float Zoom => End - Start;


		public Interval(float start, float end)
		{
			if (start > end) {
				throw new ArgumentException("Start must be less than or equal to end.");
			}

			Start = start;
			End = end;
		}


		public bool HasTime(float time)
		{
			return Start <= time && End >= time;
		}

		public int CompareTo(Interval other)
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


		public bool Equals(Interval other)
		{
			return Start == other.Start
				&& End == other.End;
		}

		public override bool Equals(object obj)
		{
			return obj is Interval interval && Equals(interval);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Start, End);
		}

		public override string ToString()
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


	/// <summary>
	/// A structure that represents a range of values and contains a value.
	/// </summary>
	public class Interval<T>(float start, float end, T value) : Interval(start, end), IInterval<T>
	{
		public T Value { get; private set; } = value;


		public bool Equals(Interval<T> other)
		{
			return Start == other.Start
				&& End == other.End
				&& EqualityComparer<T>.Default.Equals(Value, other.Value);
		}

		public override bool Equals(object obj)
		{
			return obj is Interval<T> interval && Equals(interval);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Start, End, Value);
		}

		public override string ToString()
		{
			return string.Concat("([", Start.ToString(), ", ", End.ToString(), "] ", Value.ToString(), ")");
		}

		public static bool operator ==(Interval<T> a, Interval<T> b)
		{
			return a.Start == b.Start
				&& a.End == b.End
				&& a.Value.Equals(b.Value);
		}

		public static bool operator !=(Interval<T> a, Interval<T> b)
		{
			return !(a == b);
		}
	}
}

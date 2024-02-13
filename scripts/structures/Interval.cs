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
using System;


namespace ClockBombGames.CircleBeats.Structures
{
	interface IInterval
	{
		float start { get; }
		float end { get; }

		public bool Contains(float x);
		public bool Intersects(float start, float end);
	}

	interface IInterval<TValue> : IInterval
	{
		public TValue value { get; }
	}


	/// <summary>
	/// A structure that represents a range of values and contains a value.
	/// </summary>
	public struct Interval<TValue> : IInterval<TValue>, IEquatable<Interval<TValue>>
	{
		public TValue value => _value;
		public float start => _start;
		public float end => _end;

		TValue _value;
		float _start;
		float _end;


		public Interval(float start, float end, TValue value)
		{
			if (start > end) {
				throw new ArgumentException("Start must be less than or equal to end.");
			}

			this._start = start;
			this._end = end;
			this._value = value;
		}

		public Interval(Interval interval, TValue value)
		{
			this._start = interval.start;
			this._end = interval.end;
			this._value = value;
		}


		public bool Contains(float x)
		{
			return x >= _start && x <= _end;
		}

		public bool Intersects(float start, float end)
		{
			return this._start <= end && this._end >= start;
		}

		public bool Overlaps(Interval<TValue> other)
		{
			return Intersects(other._start, other._end);
		}


		public bool Equals(Interval<TValue> other)
		{
			return start == other.start
				&& end == other.end
				&& EqualityComparer<TValue>.Default.Equals(value, other.value);
		}

		public override bool Equals(object obj)
		{
			return obj is Interval<TValue> interval && Equals(interval);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(start, end, value);
		}

		public override string ToString()
		{
			return String.Concat("([", start.ToString(), ", ", end.ToString(), "] ", value.ToString(), ")");
		}

		public static bool operator ==(Interval<TValue> a, Interval<TValue> b)
		{
			return a.start == b.start
				&& a.end == b.end
				&& a.value.Equals(b.value);
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
		float _start, _end;

		public float start => _start;
		public float end => _end;


		public Interval(float start, float end)
		{
			if (start > end) throw new ArgumentException("Start must be less than or equal to end.");

			this._start = start;
			this._end = end;
		}


		public bool Contains(float x)
		{
			return x >= _start && x <= _end;
		}

		public bool Intersects(float start, float end)
		{
			return this._start <= end && this._end >= start;
		}

		public bool Overlaps(Interval other)
		{
			return Intersects(other._start, other._end);
		}

		public bool Equals(Interval other)
		{
			return start == other.start
				&& end == other.end;
		}

		public override bool Equals(object obj)
		{
			return obj is Interval interval && Equals(interval);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(start, end);
		}

		public override string ToString()
		{
			return String.Concat("([", start.ToString(), ", ", end.ToString(), "])");
		}


		public static bool operator ==(Interval a, Interval b)
		{
			return a.start == b.start
				&& a.end == b.end;
		}

		public static bool operator !=(Interval a, Interval b)
		{
			return !(a == b);
		}
	}
}

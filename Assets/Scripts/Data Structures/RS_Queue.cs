using System.Collections.Generic;


namespace RobotoSkunk.Structures {
	/// <summary>
	/// A queue that stores intervals with a priority.<br/>
	/// This class was made to be used specifically by the Scenario class.
	/// </summary>
	public class RS_Queue<TValue> {
		public delegate void OnIntervalEnabled(Interval<TValue> interval, float time);
		public delegate void OnIntervalDisabled(Interval<TValue> interval);

		public event OnIntervalEnabled onIntervalCall = delegate { };
		public event OnIntervalDisabled onIntervalDisabled = delegate { };


		List<Interval<TValue>> forwardIntervals = new();
		List<Interval<TValue>> backwardIntervals = new();

		List<Interval<TValue>> currentIntervals = new();

		int currentIntervalIndexForward = 0;
		int currentIntervalIndexBackward = 0;
		float lastTime = 0;


		Dictionary<float, int> timeToIndexForwards = new();
		Dictionary<float, int> timeToIndexBackwards = new();


		public RS_Queue() { }
		public RS_Queue(Interval<TValue>[] intervals) {
			this.forwardIntervals = new List<Interval<TValue>>(intervals);
			this.backwardIntervals = new List<Interval<TValue>>(intervals);
		}
		public RS_Queue(List<Interval<TValue>> intervals) {
			this.forwardIntervals = intervals;
			this.backwardIntervals = new List<Interval<TValue>>(intervals);
		}


		public void Add(Interval<TValue> interval) {
			forwardIntervals.Add(interval);
			backwardIntervals.Add(interval);
		}

		public void Add(Interval interval, TValue value) {
			Add(new Interval<TValue>(interval, value));
		}


		// I know this is a mess, but it works!
		// I'll try to make it better in the future.

		public void Build() {
			forwardIntervals.Sort((a, b) => a.start.CompareTo(b.start));
			backwardIntervals.Sort((a, b) => b.end.CompareTo(a.end));


			float __lastTimeForwards = 0;
			float __lastTimeBackwards = 0;

			for (int i = 0; i < forwardIntervals.Count; i++) {
				if (forwardIntervals[i].start != __lastTimeForwards) timeToIndexForwards.Add(__lastTimeForwards, i);
				__lastTimeForwards = forwardIntervals[i].start;
			}

			for (int i = 0; i < backwardIntervals.Count; i++) {
				if (backwardIntervals[i].end != __lastTimeBackwards) timeToIndexBackwards.Add(__lastTimeBackwards, i);
				__lastTimeBackwards = backwardIntervals[i].end;
			}
		}

		public void Execute(float time) {
			if (time < lastTime) {
				ExecuteBackwards(time);
			} else {
				ExecuteForwards(time);
			}


			for (int i = 0; i < currentIntervals.Count; i++) {
				if (i >= currentIntervals.Count) break;

				var interval = currentIntervals[i];

				if (!interval.Contains(time)) {
					onIntervalDisabled(interval);
					currentIntervals.RemoveAt(i);
					continue;
				}


				onIntervalCall(interval, time);
			}


			lastTime = time;
		}


		void ExecuteForwards(float time) {
			currentIntervalIndexBackward = 0;


			for (int i = currentIntervalIndexForward; i < forwardIntervals.Count; i++) {
				if (forwardIntervals[i].start > time) break;

				currentIntervalIndexForward++;
				if (forwardIntervals[i].Contains(time)) currentIntervals.Add(forwardIntervals[i]);
				if (!timeToIndexForwards.ContainsKey(time)) timeToIndexForwards.Add(time, i);
			}
		}

		void ExecuteBackwards(float time) {
			currentIntervalIndexForward = 0;

			for (int i = currentIntervalIndexBackward; i < backwardIntervals.Count; i++) {
				if (backwardIntervals[i].end < time) break;

				currentIntervalIndexBackward++;
				if (backwardIntervals[i].Contains(time)) currentIntervals.Add(backwardIntervals[i]);
				if (!timeToIndexBackwards.ContainsKey(time)) timeToIndexBackwards.Add(time, i);
			}
		}
	}
}


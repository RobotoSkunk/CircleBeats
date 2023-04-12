using System.Collections.Generic;
using System;

using Cysharp.Threading.Tasks;
using UnityEngine;



// The main namespace of the project. Contains all the classes and structs that RobotoSkunk uses.
namespace RobotoSkunk {
	namespace Music {

		/// <summary>
		/// Analyzes an AudioSource and returns the data.
		/// </summary>
		public class AudioSourceReader {
			const int qSamples = 1024;
			readonly AudioSource m_AudioSource;

			float[] __samples;

			/// <summary>
			/// Returns the current decibels of the audio source.
			/// </summary>
			public float decibels {
				get {
					float __db = -160f;

					for (int channel = 0; channel < m_AudioSource.clip.channels; channel++) {
						__samples = new float[qSamples];
						float sum = 0;

						m_AudioSource.GetOutputData(__samples, channel);

						for (int i = 0; i < qSamples; i++) {
							sum += __samples[i] * __samples[i];
						}

						float rmsVal = Mathf.Sqrt(sum / qSamples);
						float db = 20f * Mathf.Log10(rmsVal / 0.1f);

						if (db > __db) __db = db;
					}


					if (__db < -160f) __db = -160f;
					
					return __db;
				}
			}

			/// <summary>
			/// Returns the current average volume of the audio source.
			/// </summary>
			public float avgData {
				get {
					float avg = 0f;

					for (int channel = 0; channel < m_AudioSource.clip.channels; channel++) {
						__samples = new float[qSamples];
						float tmp = 0;

						m_AudioSource.GetOutputData(__samples, channel);

						for (int i = 0; i < qSamples; i++) {
							tmp += Mathf.Abs(__samples[i]);
						}

						tmp /= qSamples;

						if (tmp > avg) avg = tmp;
					}
					
					return avg;
				}
			}

			public AudioSourceReader(AudioSource audioSource) => m_AudioSource = audioSource;


			/// <summary>
			/// Returns the maximum volume that the audio clip has.
			/// </summary>
			public async UniTask<float> GetMaxVolume() {
				int channels = m_AudioSource.clip.channels;
				float[] samples = new float[m_AudioSource.clip.samples * channels];

				float max = 0f;


				await UniTask.RunOnThreadPool(() => {
					for (int i = 0; i < channels; i++) {
						m_AudioSource.clip.GetData(samples, i);

						for (int j = 0; j < samples.Length; j++)
							if (samples[j] > max) max = samples[j];
					}
				});

				return max;
			}
		}

		/// <summary>
		/// Generates a waveform from an audio clip.
		/// </summary>
		public class AudioWaveform {
			public Sprite sprite;

			int width, height;
			Texture2D tex;

			readonly int chunks = 20;


			public AudioWaveform(int width, int height) {
				this.width = width;
				this.height = height;

				tex = new(width, height, TextureFormat.RGBA32, false);
				tex.filterMode = FilterMode.Trilinear;
				ClearTexture();

				sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), 0.5f * Vector2.one);
			}

			void ClearTexture() {
				Color32[] fillColor = tex.GetPixels32();
				Array.Clear(fillColor, 0, fillColor.Length);

				tex.SetPixels32(fillColor);
				tex.Apply();
			}

			public async UniTask GenerateWaveform(AudioClip clip) => await GenerateWaveform(clip, Color.green);
			public async UniTask GenerateWaveform(AudioClip clip, Color color) {
				int channels = clip.channels;
				List<float[]> waveForms = new();

				ClearTexture();


				#region Get waveform data
				for (int channel = 0; channel < channels; channel++) {
					float[] samples = new float[clip.samples * channels], waveform;
					clip.GetData(samples, channel);

					waveform = new float[width];
					int sx = (samples.Length / width) + 1;

					uint chunk = 0, chunkSize = (uint)(waveform.Length / chunks);

					for (int i = 0; i < waveform.Length; i++) {
						int ix = sx + (i * sx);
						float avg = 0f;

						for (int j = ix; j < ix + sx && j < samples.Length; j += 2) {
							if (j + 1 < samples.Length)
								avg += Math.Abs(samples[j]) + Math.Abs(samples[j + 1]);
							else
								avg += Math.Abs(samples[j]);
						}

						waveform[i] = sx == 0 ? 0 : avg / sx;

						if (chunk++ >= chunkSize) {
							chunk = 0;
							await UniTask.Yield();
						}
					}

					waveForms.Add(waveform);
				}
				#endregion

				#region Render waveform
				UniTask.Void(async () => {
					int chh = height / channels;
					uint chunk = 0, chunkSize = (uint)(width / chunks);


					for (int x = 0; x < width; x++) {
						Color[] col = tex.GetPixels(x, 0, 1, height, 0);
						Color32[] newCol = new Color32[col.Length];

						for (int channel = 0; channel < waveForms.Count; channel++) {
							for (int y = 0; y <= chh * waveForms[channel][x]; y++) {

								int deltaY = (int)(chh * 1.5f) + chh * (channel - 1);

								if (col.InRange(deltaY + y)) newCol[deltaY + y] = color;
								if (col.InRange(deltaY - y)) newCol[deltaY - y] = color * new Color(0.8f, 0.8f, 0.8f, 1f);
							}
						}

						tex.SetPixels32(x, 0, 1, height, newCol, 0);


						if (chunk++ >= chunkSize) {
							chunk = 0;
							tex.Apply();
							await UniTask.Yield();
						}
					}

					tex.Apply();
				});
				#endregion
			}

			struct WaveformData {
				public float[] data;
				public float max, avg;
			}
		}

		/// <summary>
		/// Analizes an audio clip.
		/// </summary>
		public static class AudioClipAnalyzer {
			public async static UniTask<AudioClipData> Analize(AudioClip clip) {
				int channels = clip.channels;
				float[] samples = new float[clip.samples * channels];

				AudioClipData data = new();

				uint chunk = 0, chunkSize = (uint)(samples.Length / 20);

				for (int i = 0; i < channels; i++) {
					clip.GetData(samples, i);

					for (int j = 0; j < samples.Length; j++) {
						float sample = Mathf.Abs(samples[j]);

						data.avg += sample;

						if (sample > data.max) data.max = sample;

						if (chunk++ >= chunkSize) {
							chunk = 0;
							await UniTask.Yield();
						}
					}

					data.avg /= samples.Length;
				}

				return data;
			}

			public struct AudioClipData {
				public float max, avg;

				public float loudness { get => (avg + max == 0f ? 1f : avg + max) / 2f; }
				public bool isLoud { get => loudness >= 0.5f; }
			}
		}
	}

	/// <summary>
	/// A collection of math functions.
	/// </summary>
	public static class RSMath {
		public static int Normalize(int x, int min, int max) => (x - min) / (max - min);
		public static float Normalize(float x, float min, float max) => (x - min) / (max - min);

		public static Vector3 GetDirVector(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		public static float SafeDivision(float numerator, float denominator) => denominator == 0f ? 0f : numerator / denominator;

		public static float Direction(Vector2 from, Vector2 to) => Mathf.Atan2(to.y - from.y, to.x - from.x);

		public static int ToInt(this bool b) => b ? 1 : 0;

		public static Vector2 Clamp(Vector2 vector, Vector2 min, Vector2 max) => new(
			Mathf.Clamp(vector.x, min.x, max.x),
			Mathf.Clamp(vector.y, min.y, max.y)
		);
		public static Vector3 Clamp(Vector3 vector, Vector3 min, Vector3 max) => new(
			Mathf.Clamp(vector.x, min.x, max.x),
			Mathf.Clamp(vector.y, min.y, max.y),
			Mathf.Clamp(vector.z, min.z, max.z)
		);

		public static Vector2 Lerp(Vector2 from, Vector2 to, float t) => new(
			Mathf.Lerp(from.x, to.x, t),
			Mathf.Lerp(from.y, to.y, t)
		);
		public static Vector3 Lerp(Vector3 from, Vector3 to, float t) => new(
			Mathf.Lerp(from.x, to.x, t),
			Mathf.Lerp(from.y, to.y, t),
			Mathf.Lerp(from.z, to.z, t)
		);

		public static Vector2 LerpUnclamped(Vector2 from, Vector2 to, float t) => new(
			Mathf.LerpUnclamped(from.x, to.x, t),
			Mathf.LerpUnclamped(from.y, to.y, t)
		);
		public static Vector3 LerpUnclamped(Vector3 from, Vector3 to, float t) => new(
			Mathf.LerpUnclamped(from.x, to.x, t),
			Mathf.LerpUnclamped(from.y, to.y, t),
			Mathf.LerpUnclamped(from.z, to.z, t)
		);
	}

	/// <summary>
	/// A collection of time functions.
	/// </summary>
	public static class RSTime {
		public static float delta { get => Time.deltaTime / 0.016666f; }
	}

	/// <summary>
	/// Extensions to make your life easier.
	/// </summary>
	public static class Extensions {
		public static bool InRange<T>(this T[] array, int x) => x >= 0 && x < array.Length;
	}


	// A namespace that contains structures that are used in this project.
	namespace Structures {
		// Note: These structures are designed specifically for this project and may not be suitable for other projects.

		/// <summary>
		/// A structure that represents a range of values.
		/// </summary>
		public struct Interval<TValue> {
			public TValue value;
			float _start, _end;

			public float start => _start;
			public float end => _end;


			public Interval(float start, float end, TValue value) {
				if (start > end) throw new ArgumentException("Start must be less than or equal to end.");

				this._start = start;
				this._end = end;
				this.value = value;
			}

			public bool Contains(float x) => x >= _start && x <= _end;
			public bool Intersects(float start, float end) => this._start <= end && this._end >= start;
			public bool Overlaps(Interval<TValue> other) => Intersects(other._start, other._end);

			public bool Equals(Interval<TValue> other) {
				return start == other.start
					&& end == other.end
					&& EqualityComparer<TValue>.Default.Equals(value, other.value);
			}
			public override bool Equals(object obj) {
				return obj is Interval<TValue> interval && Equals(interval);
			}

			public override int GetHashCode() {
				return HashCode.Combine(start, end, value);
			}

			public override string ToString() {
				return String.Concat("([", start.ToString(), ", ", end.ToString(), "] ", value.ToString(), ")");
			}

			public static bool operator ==(Interval<TValue> a, Interval<TValue> b) {
				return a.start == b.start
					&& a.end == b.end
					&& a.value.Equals(b.value);
			}
			public static bool operator !=(Interval<TValue> a, Interval<TValue> b) {
				return !(a == b);
			}
		}


		/// <summary>
		/// A structure of an interval tree. This structure is used to store intervals and retrieve them efficiently.
		/// <para>Note: This structure can't retrieve intervals that are partially overlapping.</para>
		/// </summary>
		public class IntervalTree<TValue> {
			public class Node {
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
				public Node(float start, float end, TValue value) {
					_interval = new Interval<TValue>(start, end, value);
				}

				/// <summary>
				/// Creates a new node with the specified interval.
				/// </summary>
				public Node(Interval<TValue> interval) {
					this._interval = interval;
				}

				/// <summary>
				/// Creates a new balanced branch with the specified intervals.
				/// </summary>
				public Node(Interval<TValue>[] intervals) {
					if (intervals.Length == 0) return;

					_interval = intervals[0];
					_max = _interval.end;
					if (intervals.Length == 1) return;


					int mid = intervals.Length / 2;
					var left = intervals[..mid];
					var right = intervals[mid..];

					if (left.Length > 0) this._left = new Node(left);
					if (right.Length > 0) this._right = new Node(right);

					if (this._left != null) _max = Mathf.Max(_max, this._left.max);
					if (this._right != null) _max = Mathf.Max(_max, this._right.max);
				}



				/// <summary>
				/// Adds an interval to the tree like a binary search tree.
				/// It should be only called if you're f*cked up with the tree.
				/// </summary>
				public void ForceAdd(Interval<TValue> interval) {
					if (interval.start < this._interval.start) {
						if (_left == null) _left = new Node(interval);
						else _left.ForceAdd(interval);
					}
					else {
						if (_right == null) _right = new Node(interval);
						else _right.ForceAdd(interval);
					}
				}

				/// <summary>
				/// Searches for an interval that contains the given interval.
				/// </summary>
				public Node Search(float start, float end) {
					if (_interval.Intersects(start, end)) return this;

					if (_left != null && _left.max >= start) return _left.Search(start, end);
					if (_right != null) return _right.Search(start, end);

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
			public IntervalTree(Interval<TValue>[] intervals) {
				_intervals = new List<Interval<TValue>>(intervals);
			}


			/// <summary>
			/// Builds the tree from the given intervals. You feel like a charm, the birds sing and the sun shines.
			/// </summary>
			public void Build() {
				_root = null;

				_intervals.Sort((a, b) => a.start.CompareTo(b.start));
				_root = new Node(_intervals.ToArray());
			}


			/// <summary>
			/// Forces the tree to search for an interval that contains the given interval instead of using
			/// the cached node.
			/// </summary>
			public Node ForceSearch(float start, float end) {
				return _root.Search(start, end);
			}

			/// <summary>
			/// Forces the tree to search for an interval that contains the given scalar instead of using
			/// the cached node.
			/// </summary>
			public Node ForceSearch(float x) {
				return _root.Search(x);
			}


			/// <summary>
			/// Searches for an interval that contains the given interval.
			/// </summary>
			public Node Search(float start, float end) {
				if (_temporalNode != null) {
					if (_temporalNode.interval.Intersects(start, end)) return _temporalNode;
					else _temporalNode = null;
				}

				_temporalNode = ForceSearch(start, end);
				return _temporalNode;
			}

			/// <summary>
			/// Searches for an interval that contains the given scalar.
			/// </summary>
			public Node Search(float x) {
				return Search(x, x);
			}



			/// <summary>
			/// Adds an interval to the tree like a binary search tree.
			/// Note: it only works if the tree is already built.
			/// </summary>
			public void Add(Interval<TValue> interval) {
				_intervals.Add(interval);

				if (_root != null) _root.ForceAdd(interval);
			}

			/// <summary>
			/// Adds an interval to the tree like a binary search tree.
			/// It should be only called if you're f*cked up with the tree.
			/// </summary>
			public void ForceAdd(Interval<TValue> interval) {
				_intervals.Add(interval);

				if (_root != null) _root.ForceAdd(interval);
				else _root = new Node(interval);
			}
		}


		/// <summary>
		/// A queue that stores intervals with a priority.<br/>
		/// This class was made to be used specifically by the Scenario class.
		/// </summary>
		public class RS_Queue<TValue> {
			public delegate void OnIntervalEnabled(Interval<TValue> interval, float time);
			public delegate void OnIntervalDisabled(Interval<TValue> interval);

			public event OnIntervalEnabled onIntervalCall = delegate { };
			public event OnIntervalDisabled onIntervalDisabled = delegate { };


			List<Interval<TValue>> intervals = new();
			List<Interval<TValue>> currentIntervals = new();

			int currentIntervalIndex = 0;
			float lastTime = 0;


			Dictionary<float, int> timeToIndex = new();


			public RS_Queue() { }
			public RS_Queue(Interval<TValue>[] intervals) {
				this.intervals = new List<Interval<TValue>>(intervals);
			}
			public RS_Queue(List<Interval<TValue>> intervals) {
				this.intervals = intervals;
			}


			public void Add(Interval<TValue> interval) {
				intervals.Add(interval);
			}


			// I know this is a mess, but it works!
			// I'll try to make it better in the future.

			public void Build() {
				intervals.Sort((a, b) => a.start.CompareTo(b.start));


				float __lastTime = 0;

				for (int i = 0; i < intervals.Count; i++) {
					if (intervals[i].start != __lastTime) timeToIndex.Add(__lastTime, i);
					__lastTime = intervals[i].start;
				}
			}

			public void Execute(float time) {
				if (time < lastTime) {
					if (timeToIndex.ContainsKey(time)) {
						currentIntervalIndex = timeToIndex[time];
					} else {
						currentIntervalIndex = 0;
					}
				}


				for (int i = currentIntervalIndex; i < intervals.Count; i++) {
					if (intervals[i].start > time) break;


					currentIntervalIndex++;
					if (intervals[i].Contains(time)) currentIntervals.Add(intervals[i]);
					if (!timeToIndex.ContainsKey(time)) timeToIndex.Add(time, i);
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
		}
	}



	public struct BezierCurve {
		public Vector2 p1, p2;

		public BezierCurve(Vector4 v) {
			p1 = new Vector2(v.x, v.y);
			p2 = new Vector2(v.z, v.w);
		}
		public BezierCurve(Vector2 p1, Vector2 p2) {
			this.p1 = p1;
			this.p2 = p2;
		}
		public BezierCurve(float p1x, float p1y, float p2x, float p2y) {
			p1 = new Vector2(p1x, p1y);
			p2 = new Vector2(p2x, p2y);
		}


		public Vector2 GetTime(float t) {
			Vector2 p0 = Vector2.zero;
			Vector2 p3 = Vector2.one;

			float t2 = t * t;
			float t3 = t2 * t;

			float u = 1 - t;
			float u2 = u * u;
			float u3 = u2 * u;

			return u3 * p0 + 3 * u2 * t * p1 + 3 * u * t2 * p2 + t3 * p3;
		}

		public static readonly BezierCurve linear = new(0, 0, 1, 1);

		public static readonly BezierCurve easeIn = new(0.42f, 0, 1, 1);
		public static readonly BezierCurve easeOut = new(0, 0, 0.58f, 1);
		public static readonly BezierCurve easeInOut = new(0.42f, 0, 0.58f, 1);

		public static readonly BezierCurve cubicIn = new(0.55f, 0.055f, 0.675f, 0.19f);
		public static readonly BezierCurve cubicOut = new(0.215f, 0.61f, 0.355f, 1);
		public static readonly BezierCurve cubicInOut = new(0.645f, 0.045f, 0.355f, 1);

		public static readonly BezierCurve quartIn = new(0.895f, 0.03f, 0.685f, 0.22f);
		public static readonly BezierCurve quartOut = new(0.165f, 0.84f, 0.44f, 1);
		public static readonly BezierCurve quartInOut = new(0.77f, 0, 0.175f, 1);

		public static readonly BezierCurve quintIn = new(0.755f, 0.05f, 0.855f, 0.06f);
		public static readonly BezierCurve quintOut = new(0.23f, 1, 0.32f, 1);
		public static readonly BezierCurve quintInOut = new(0.86f, 0, 0.07f, 1);

		public static readonly BezierCurve sineIn = new(0.47f, 0, 0.745f, 0.715f);
		public static readonly BezierCurve sineOut = new(0.39f, 0.575f, 0.565f, 1);
		public static readonly BezierCurve sineInOut = new(0.445f, 0.05f, 0.55f, 0.95f);


		public static implicit operator BezierCurve((Vector2, Vector2) tuple) => new(tuple.Item1, tuple.Item2);
		public static implicit operator BezierCurve((float, float, float, float) tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
		public static implicit operator BezierCurve(Vector4 v) => new(v);

		public static implicit operator Vector4(BezierCurve curve) => new(curve.p1.x, curve.p1.y, curve.p2.x, curve.p2.y);
		public static implicit operator (Vector2, Vector2)(BezierCurve curve) => (curve.p1, curve.p2);
		public static implicit operator (float, float, float, float)(BezierCurve curve) => (curve.p1.x, curve.p1.y, curve.p2.x, curve.p2.y);

		public static implicit operator Vector2[](BezierCurve curve) => new Vector2[] { curve.p1, curve.p2 };
		public static implicit operator BezierCurve(Vector2[] v) => new(v[0], v[1]);
	}
}

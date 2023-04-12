using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

using RobotoSkunk.Music;
using RobotoSkunk.Structures;

using TMPro;



namespace RobotoSkunk.CircleBeats {
	public class Scenario : MonoBehaviour {
		[Header("Tests")]
		public Image img;
		[Range(0f, 1)] public float volume = 1f;
		public Slider slider;

		[Header("Settings")]
		[SerializeField] uint bpm;
		[SerializeField] [Range(0.5f, 2)] float size = 1f;
		[SerializeField] [Range(0, 1)]    float bpmForce = 1f;
		[SerializeField] [Range(0, 1)]    float spectrumForce = 1f;
		[SerializeField] [Range(0, 1)]    float decibelsForce = 1f;

		[Header("Components")]
		[SerializeField] AudioSource audioSource;
		[SerializeField] AudioMixer audioMixer;

		[Header("Obstacles")]
		[SerializeField] GameObject obstaclePrefab;
		[SerializeField] Transform obstacleParent;

		[Header("Captions")]
		[SerializeField] TextMeshProUGUI captionsText;

		[Header("Background")]
		[SerializeField] GameObject spectrumBar;
		[SerializeField] Image radialPart;
		[SerializeField] Transform radialBg;
		[SerializeField] Transform bgObj;


		IntervalTree<string> captions = new();
		RS_Queue<GameObject> obstacles = new();




		AudioClipAnalyzer.AudioClipData audioClipData = new();
		AudioWaveform waveform;
		AudioSourceReader audioReader;
		float nextSize, sPosTime = 0f, spectrumSize = 0f;
		List<GameObject>[] visualizer = new List<GameObject>[5];
		List<Image> radialBackground = new();


		float[] spectrumBuffer = new float[Globals.spectrumSamples];
		int sPos = 0;

		float bpmDelta { get => audioSource.time / (60f / (bpm == 0 ? 1 : bpm)); }
		Vector2 sceneSize { get => new(size, size); }
		readonly int radialParts = 8;
		float nextBpm = 0f;



		private void Awake() {
			audioReader = new(audioSource);
			waveform = new(4096, 512);

			img.overrideSprite = waveform.sprite;


			for (int i = 0; i < radialParts; i++) {
				float angle = i * (360f / radialParts);

				Image obj = Instantiate(radialPart);
				obj.transform.SetParent(radialBg);
				obj.transform.localPosition = Globals.bgDistance * Vector3.forward;
				obj.transform.localScale = Vector3.one;
				obj.transform.localEulerAngles = (angle - 90f) * Vector3.forward;

				obj.fillAmount = 1f / radialParts;
				obj.color = i % 2 == 0 ? new Color(0.8f, 0.8f, 0.8f) : Color.gray;

				radialBackground.Add(obj);
			}

			UniTask.Void(async () => {
				for (int j = 0; j < visualizer.Length; j++) {
					float jAngle = j * (360f / visualizer.Length);
					int chunk = 0, chunkSize = visualizer.Length * Globals.spectrumSamples / 16;

					visualizer[j] = new();


					for (int i = 0; i < Globals.spectrumSamples; i++) {
						float angle = i * (360f / Globals.spectrumSamples) + jAngle;

						GameObject obj = Instantiate(spectrumBar);
						obj.transform.SetParent(bgObj);
						obj.transform.localEulerAngles = (angle - 90f) * Vector3.forward;
						obj.transform.localPosition = 5f * RSMath.GetDirVector(angle * Mathf.Deg2Rad) + Globals.bgDistance * Vector3.forward;
						obj.transform.localScale = new Vector3(0.25f, 0f, 1f);
						// obj.index = i;
						// obj.maxSize = 6f;
						// obj.enabled = false;

						visualizer[j].Add(obj);

						chunk++;
						if (chunk >= chunkSize) {
							chunk = 0;
							await UniTask.Yield();
						}
					}
				}
			});



			#region Captions
			int[] arr = new int[100];
			for (int i = 0; i < arr.Length; i++) {
				arr[i] = i;
			}

			// Shuffle the array
			for (int i = 0; i < arr.Length; i++) {
				int r = Random.Range(0, arr.Length);
				int t = arr[i];
				arr[i] = arr[r];
				arr[r] = t;
			}

			for (int i = 0; i < arr.Length; i++) {
				captions.ForceAdd(new(arr[i], arr[i] + 1f, $"Caption {arr[i]}"));
			}

			captions.Build();
			#endregion

			#region Obstacles
			for (int i = 0; i < 15000; i++) {
				GameObject obj = Instantiate(obstaclePrefab, obstacleParent);
				// obj.transform.localPosition = new(-10, (i - 2) * 5f, 0f);
				obj.transform.localPosition = 10f * Random.insideUnitCircle;
				obj.transform.localScale = Vector3.one;
				obj.transform.rotation = Quaternion.identity;
				obj.SetActive(false);


				float time = audioSource.clip.length * Random.value;

				obstacles.Add(new(time, time + 5f, obj));
			}

			obstacles.Build();
			#endregion
		}

		private void OnEnable() {
			obstacles.onIntervalDisabled += OnIntervalDisabled;
			obstacles.onIntervalCall += OnIntervalCall;
		}
		private void OnDisable() {
			obstacles.onIntervalDisabled -= OnIntervalDisabled;
			obstacles.onIntervalCall -= OnIntervalCall;
		}


		void OnIntervalDisabled(Interval<GameObject> interval) {
			interval.value.SetActive(false);
		}

		void OnIntervalCall(Interval<GameObject> interval, float time) {
			interval.value.SetActive(true);
		}



		private void Start() {
			UniTask.Void(async () => {
				audioClipData = await AudioClipAnalyzer.Analize(audioSource.clip);

				Debug.Log($"Max: {audioClipData.max} | AVG : {audioClipData.avg} | IsLoud: {audioClipData.isLoud} | Loudness: {audioClipData.loudness}");

				await waveform.GenerateWaveform(audioSource.clip, Color.yellow);
			});
		}

		private void Update() {
			float mscMult = Mathf.Clamp01(audioReader.avgData / audioClipData.loudness);
			float __bpm = 1f + (bpmDelta == 0f ? 0f : 1f - Mathf.Abs(bpmDelta - Mathf.Round(bpmDelta) + 0.75f)) * bpmForce;


			nextSize = Mathf.Lerp(
				nextSize,
				1f - 0.2f * decibelsForce + mscMult * 0.5f * decibelsForce,
				0.45f * RSTime.delta
			);
			nextBpm = Mathf.Lerp(nextBpm, __bpm, 0.45f * RSTime.delta);

			transform.localScale = (Vector3)(nextSize * nextBpm * sceneSize) + Vector3.forward;


			slider.SetValueWithoutNotify(audioSource.time / audioSource.clip.length);
			audioMixer.SetFloat("MusicVolume", Mathf.Log(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);


			#region Spectrum and carousel
			System.Array.Clear(Globals.spectrum, 0, Globals.spectrum.Length);

			for (int channel = 0; channel < audioSource.clip.channels; channel++) {
				audioSource.GetSpectrumData(spectrumBuffer, channel, FFTWindow.Rectangular);

				for (int i = 0; i < Globals.spectrumSamples; i++) {
					int j = i + sPos;
					if (j >= Globals.spectrumSamples) j -= Globals.spectrumSamples;

					if (spectrumBuffer[j] > Globals.spectrum[i])
						Globals.spectrum[i] = spectrumBuffer[j];
				}
			}

			float barY;
			spectrumSize = Mathf.Lerp(spectrumSize, spectrumForce * 6f, 0.2f * RSTime.delta);

			for (int i = 0; i < Globals.spectrumSamples; i++) {
				barY = spectrumSize * Globals.spectrum[i];

				for (int r = 0; r < visualizer.Length; r++) {
					if (visualizer[r] == null) continue;
					if (i >= visualizer[r].Count) continue;

					Vector2 s = visualizer[r][i].transform.localScale;
					s.y = Mathf.Lerp(s.y, 0f, 0.1f * RSTime.delta);

					if (barY > s.y) s.y = barY;

					visualizer[r][i].transform.localScale = s;
				}
			}


			sPosTime += Time.deltaTime;
			if (sPosTime > (1f / 30f)) {
				sPosTime = 0f;
				sPos += 3;

				if (sPos >= Globals.spectrumSamples) sPos = 0;
			}
			#endregion


			#region Captions
			var interval = captions.Search(audioSource.time);

			captionsText.text = interval == null ? "" : interval.value;
			#endregion

			#region Obstacles
			obstacles.Execute(audioSource.time);

			// foreach (var obstacle in obstacles.Query(audioSource.time)) {
			// 	obstacle.value.SetActive(true);
			// }
			// foreach (var obstacle in obstacles.GetDisabledIntervals()) {
			// 	obstacle.value.SetActive(false);
			// }
			#endregion
		}

		public void SongTimeChange() => audioSource.time = slider.value * audioSource.clip.length;
	}
}

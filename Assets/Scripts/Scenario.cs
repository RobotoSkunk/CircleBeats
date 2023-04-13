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
		[SerializeField] [Range(0.5f, 2f)] float size = 1f;
		[SerializeField] [Range(0f, 1f)]   float bpmForce = 1f;
		[SerializeField] [Range(0f, 1f)]   float spectrumForce = 1f;
		[SerializeField] [Range(0f, 1f)]   float decibelsForce = 1f;

		[Header("Components")]
		[SerializeField] AudioSource audioSource;
		[SerializeField] AudioMixer audioMixer;

		[Header("Obstacles")]
		[SerializeField] Obstacle obstaclePrefab;
		[SerializeField] Transform obstacleParent;

		[Header("Captions")]
		[SerializeField] TextMeshProUGUI captionsText;

		[Header("Spectrum")]
		[SerializeField] [Range(0f, 6f)] float spectrumBarMaxSize = 6f;
		[SerializeField] SpectrumBar spectrumBar;

		[Header("Background")]
		[SerializeField] Image radialPart;
		[SerializeField] Transform radialBg;
		[SerializeField] Transform bgObj;


		IntervalTree<string> captions = new();
		RS_Queue<Obstacle> obstacles = new();




		AudioClipAnalyzer.AudioClipData audioClipData = new();
		AudioWaveform waveform;
		AudioSourceReader audioReader;
		float nextSize, spectrumSpikeTimer = 0f, spectrumSize = 0f;
		SpectrumBar[][] carrouselBars;
		List<Image> radialBackground = new();


		float[] spectrumBuffer = new float[Globals.spectrumSamples];
		int spectrumSpikePosition = 0;

		float bpmDelta { get => audioSource.time / (60f / (bpm == 0 ? 1 : bpm)); }
		Vector2 sceneSize { get => new(size, size); }
		readonly int radialParts = 8;
		readonly int carrouselSpikes = 5;
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

			carrouselBars = new SpectrumBar[carrouselSpikes][];
			spectrumBarMaxSize = Mathf.Clamp(spectrumBarMaxSize, 0f, 6f);

			UniTask.Void(async () => {
				for (int j = 0; j < carrouselBars.Length; j++) {
					float jAngle = j * (360f / carrouselBars.Length);
					int chunk = 0, chunkSize = carrouselBars.Length * Globals.spectrumSamples / 16;

					carrouselBars[j] = new SpectrumBar[Globals.spectrumSamples];


					for (int i = 0; i < Globals.spectrumSamples; i++) {
						float angle = i * (360f / Globals.spectrumSamples) + jAngle;

						SpectrumBar obj = Instantiate(spectrumBar, bgObj);
						obj.maxSize = spectrumBarMaxSize;
						obj.angle = angle;

						carrouselBars[j][i] = obj;

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

			//// For random obstacles test only ////

			for (int i = 0; i < 100; i++) {
				Vector2 randomPosition = 10f * Random.insideUnitCircle;

				Obstacle obj = Instantiate(obstaclePrefab, obstacleParent);
				obj.gameObject.SetActive(false);

				float time = audioSource.clip.length * Random.value;

				obj.positions.AddX(0f, 0.5f, randomPosition.x, randomPosition.x + 5f, BezierCurve.easeInOut);
				obj.positions.AddX(0.5f, 1f, randomPosition.x + 5f, randomPosition.x, BezierCurve.easeInOut);

				obj.positions.AddY(0f, 1f, randomPosition.y, randomPosition.y, BezierCurve.linear);

				obj.scales.Add(0f, 0.5f, Vector2.zero, Vector2.one, BezierCurve.linear);
				obj.scales.Add(0.5f, 1f, Vector2.one, Vector2.zero, BezierCurve.linear);

				obj.rotations.Add(0f, 1f, 0f, 360f, BezierCurve.linear);

				obj.shakeStrengths.Add(0f, 0.5f, Vector2.zero, Vector2.one, BezierCurve.linear);
				obj.shakeStrengths.Add(0.5f, 1f, Vector2.one, Vector2.zero, BezierCurve.linear);

				obj.colors.Add(0f, 0.5f, Color.white, Color.red, BezierCurve.linear);
				obj.colors.Add(0.5f, 1f, Color.red, Color.white, BezierCurve.linear);

				obj.Prepare();


				obj.lifeTime = new Interval(time, time + 5f);
				obstacles.Add(obj.lifeTime, obj);
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


		void OnIntervalDisabled(Interval<Obstacle> interval) {
			if (interval.value.gameObject.activeInHierarchy) {
				interval.value.gameObject.SetActive(false);
			}
		}

		void OnIntervalCall(Interval<Obstacle> interval, float time) {
			if (!interval.value.gameObject.activeInHierarchy) {
				interval.value.gameObject.SetActive(true);
			}

			interval.value.SetTime(time);
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

			// Get spectrum data
			for (int channel = 0; channel < audioSource.clip.channels; channel++) {
				audioSource.GetSpectrumData(spectrumBuffer, channel, FFTWindow.Rectangular);

				for (int i = 0; i < Globals.spectrumSamples; i++) {
					int j = i + spectrumSpikePosition;

					if (j >= Globals.spectrumSamples) {
						j -= Globals.spectrumSamples;
					}

					if (spectrumBuffer[j] > Globals.spectrum[i]) {
						Globals.spectrum[i] = spectrumBuffer[j];
					}
				}
			}

			float barY;
			spectrumSize = Mathf.Lerp(spectrumSize, spectrumForce, 0.2f * RSTime.delta);


			// i = spectrum index
			// r = carrousel spike index

			// Update carrousel bars
			for (int i = 0; i < Globals.spectrumSamples; i++) {
				barY = spectrumSize * Globals.spectrum[i];

				for (int r = 0; r < carrouselSpikes; r++) {
					if (carrouselBars[r] == null) continue;
					if (carrouselBars[r][i] == null) continue;


					float currentSize = carrouselBars[r][i].size;
					if (barY > currentSize) carrouselBars[r][i].size = Mathf.Clamp01(barY);

					// float currentSize = Mathf.Lerp(carrouselBars[r][i].transform.localScale.y, 0f, 0.1f * RSTime.delta);
					// if (barY > currentSize) currentSize = barY;

					// carrouselBars[r][i].transform.localScale = new Vector2(1f, currentSize);
				}
			}


			// Spin carrousel
			spectrumSpikeTimer += Time.deltaTime;
			if (spectrumSpikeTimer > (1f / 15f)) {
				spectrumSpikeTimer = 0f;
				spectrumSpikePosition += 5;

				if (spectrumSpikePosition >= Globals.spectrumSamples) spectrumSpikePosition = 0;
			}
			#endregion


			#region Captions
			var interval = captions.Search(audioSource.time);

			captionsText.text = interval == null ? "" : interval.value;
			#endregion

			#region Obstacles
			obstacles.Execute(audioSource.time);
			#endregion
		}

		public void SongTimeChange() => audioSource.time = slider.value * audioSource.clip.length;
	}
}

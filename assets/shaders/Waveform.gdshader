shader_type canvas_item;

uniform vec2 size;
uniform float channels;
uniform vec2[12000] samples;

const float alpha = 0.3;

void vertex() {
	vec2 scale = vec2(MODEL_MATRIX[0][0], MODEL_MATRIX[1][1]);
	vec2 squared_uv = VERTEX * scale;

	UV = squared_uv;
}

void fragment() {
	vec2 p = UV; // To easily change this value
	COLOR = vec4(0, 0, 0, 0);

	float jumps = float(samples.length()) / size.x;
	float peakHeight = size.y / channels;

	int startIndex = int(p.x * jumps);
	int endIndex = min(startIndex + int(jumps), samples.length());

	if (startIndex < endIndex) { // Avoid weird bugs if possible
		vec2 peak = vec2(0, 0); // Left and Right channels

		for (int i = startIndex; i < endIndex; i++) {
			if (samples[i].x > peak.x) {
				peak.x = samples[i].x;
			}

			if (samples[i].y > peak.y) {
				peak.y = samples[i].y;
			}
		}

		peak *= peakHeight / 2.0;
		peak = clamp(peak, 1, peakHeight / 2.0);

		vec2 center = vec2(peakHeight / 2.0, peakHeight + peakHeight / 2.0);
		center = round(center);

		// Left Channel
		if (p.y > center.x - peak.x && p.y < center.x + peak.x) {
			if (p.y < center.x) {
				COLOR = vec4(1, 1, 1, alpha);
			} else {
				COLOR = vec4(0.8, 0.8, 0.8, alpha);
			}
		}

		// Right Channel
		if (channels == 2.0) {
			if (p.y > center.y - peak.y && p.y < center.y + peak.y) {
				if (p.y < center.y) {
					COLOR = vec4(1, 1, 1, alpha);
				} else {
					COLOR = vec4(0.8, 0.8, 0.8, alpha);
				}
			}
		}
	}
}

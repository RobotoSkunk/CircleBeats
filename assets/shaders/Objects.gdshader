shader_type spatial;
render_mode unshaded;

instance uniform vec4 _color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
    ALBEDO = _color.rgb;
}

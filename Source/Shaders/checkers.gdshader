shader_type spatial;

uniform float size = 1.0;
uniform vec3 color1 : source_color;
uniform vec3 color2 : source_color;

void fragment() {
	vec4 wrld_vertex = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);
	float x = 0.0;
	if (wrld_vertex.x != 0.0) x = mod(wrld_vertex.x, size*2.0);
	float z = 0.0;
	if (wrld_vertex.z != 0.0) z = mod(wrld_vertex.z, size*2.0);
	bool useCol2 = false;
	if (x > size) useCol2 = !useCol2;
	if (z > size) useCol2 = !useCol2;
	if (useCol2) ALBEDO = color2;
	else ALBEDO = color1;
}

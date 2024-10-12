shader_type canvas_item;

uniform vec4 colorTint : source_color;
uniform float mixAmount : hint_range(0.0, 1.0, 0.1);
uniform float blurAmountX : hint_range(-1.0, 1.0, 0.1);
uniform float blurAmountY : hint_range(-1.0, 1.0, 0.1);
uniform int blurIterations : hint_range(0, 5, 1);
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

void fragment() {
	vec2 uv = SCREEN_UV;
	vec4 sample = texture(SCREEN_TEXTURE,uv);
	for (int i = 0; i < blurIterations; i++) {
		float blurMix = (float(blurIterations)-float(i)) / (float(blurIterations)+1.0);
		float blurDisplaceX = (blurAmountX/float(blurIterations) * 0.1) * float(i+1);
		float blurDisplaceY = (blurAmountY/float(blurIterations) * 0.1) * float(i+1);
		vec2 blurUv = vec2(uv.x + blurDisplaceX, uv.y + blurDisplaceY);
		vec4 blurSample = texture(SCREEN_TEXTURE,blurUv);
		sample = mix(sample, blurSample, blurMix);
	}
	COLOR = mix(sample, colorTint, mixAmount);
}

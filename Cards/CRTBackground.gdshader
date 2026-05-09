shader_type canvas_item;

// Sliders you can tweak in the Godot Inspector!
uniform float scanline_count : hint_range(0.0, 200.0) = 100.0;
uniform float scanline_opacity : hint_range(0.0, 1.0) = 0.25;
uniform float vignette_strength : hint_range(0.0, 3.0) = 1.2;

void fragment() {
    // 1. Get the original color of your green gradient
    vec4 bg_color = texture(TEXTURE, UV);
    
    // 2. The Scanlines: Use a sine wave based on the Y position
    float s = sin(UV.y * scanline_count * 3.14159 * 2.0);
    s = (s * 0.5 + 0.5) * scanline_opacity + (1.0 - scanline_opacity);
    
    // 3. The Vignette: Calculate distance from the exact center (0.5, 0.5)
    float d = distance(UV, vec2(0.5));
    // Darken the edges smoothly
    float vignette = smoothstep(0.8, 0.2, d * vignette_strength);
    
    // 4. Combine the original green with the scanlines and shadows
    COLOR = vec4(bg_color.rgb * s * vignette, bg_color.a);
}
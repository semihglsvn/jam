shader_type canvas_item;

// This lets us drag a noise texture into the Inspector
uniform sampler2D noise_tex;

// This is the slider we will animate in C# (0.0 is invisible, 1.0 is fully visible)
uniform float threshold : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    // 1. Get the color of the playing card at this exact pixel
    vec4 card_color = texture(TEXTURE, UV);
    
    // 2. Get the color of the static noise at this exact pixel
    vec4 noise_color = texture(noise_tex, UV);
    
    // 3. The Glitch Logic: If the noise is brighter than our threshold, make the pixel invisible
    if (noise_color.r > threshold) {
        card_color.a = 0.0;
    }
    
    // 4. Output the final pixel color to the screen
    COLOR = card_color;
}
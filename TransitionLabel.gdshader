shader_type canvas_item;

uniform float intensity : hint_range(0.0, 0.05) = 0.005; // Subtle horizontal tear
uniform float speed : hint_range(0.0, 20.0) = 15.0;      // Fast flickering
uniform float frequency : hint_range(0.0, 1.0) = 0.05;   // 5% glitch chance

// NEW: Godot 4 CanvasGroup requires reading from the screen texture!
uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;

float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void fragment() {
    // NEW: We use SCREEN_UV instead of standard UV
    vec2 screen_uv = SCREEN_UV;
    
    // Slice the text into horizontal strips
    float strip = floor(screen_uv.y * 60.0);
    float time_step = floor(TIME * speed);
    
    // Calculate a random chance for this specific strip to glitch
    float noise = random(vec2(strip, time_step));
    
    // If the random noise hits our rare 5% frequency...
    if (noise > (1.0 - frequency)) {
        float dir = sign(random(vec2(strip, time_step + 1.0)) - 0.5);
        screen_uv.x += dir * intensity;
    }
    
    // Read the flattened CanvasGroup children from the back buffer!
    COLOR = texture(screen_texture, screen_uv);
}
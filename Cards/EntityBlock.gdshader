shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float pixel_size : hint_range(1.0, 64.0) = 4.0;
uniform float is_active : hint_range(0.0, 1.0) = 0.0; // 0 = Normal, 1 = Glitch

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    
    if (is_active > 0.5) {
        // 1. Chunky pixelation
        vec2 grid_uv = floor(SCREEN_UV / (SCREEN_PIXEL_SIZE * pixel_size)) * (SCREEN_PIXEL_SIZE * pixel_size);
        
        // 2. Random noise for offsets and colors
        float noise = rand(grid_uv + TIME * 3.0);
        vec2 offset = vec2(noise * 0.04 - 0.02, 0.0);
        
        // 3. Chromatic Aberration (Screen-based)
        float r = texture(screen_texture, grid_uv + offset + vec2(0.008, 0.0)).r;
        float g = texture(screen_texture, grid_uv + offset).g;
        float b = texture(screen_texture, grid_uv + offset - vec2(0.008, 0.0)).b;
        
        vec3 final_color = vec3(r, g, b);
        
        // 4. YOUR VIBRANT COLORS
        if (noise > 0.9) {
            final_color = vec3(1.0, 1.0, 1.0); // Pure White
        } else if (noise > 0.75) {
            final_color = mix(final_color, vec3(1.0, 0.0, 0.8), 0.85); // Pink
        } else if (noise > 0.6) {
            final_color = mix(final_color, vec3(0.8, 1.0, 0.0), 0.8);  // Neon Green
        } else if (noise < 0.1) {
            final_color = mix(final_color, vec3(0.0, 1.0, 1.0), 0.85); // Cyan
        } else if (noise < 0.25) {
            final_color = mix(final_color, vec3(1.0, 0.1, 0.1), 0.8);  // Glitch Red
        }
        
        // Apply to the tile but keep it slightly transparent to show it's "pass-through"
        COLOR = vec4(final_color, tex_color.a * 0.8);
    } else {
        // Just show the normal tile
        COLOR = tex_color;
    }
}
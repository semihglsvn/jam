shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, filter_nearest;
uniform float progress : hint_range(0.0, 1.0) = 0.0;
uniform float pixel_size : hint_range(1.0, 64.0) = 8.0;

// Random noise generator
float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}

void fragment() {
    // 1. Calculate distance from the exact center of the screen
    vec2 center = vec2(0.5, 0.5);
    float dist = distance(UV, center);
    
    // 2. Edges-to-center logic
    if (dist > 0.8 - progress) {
        
        // 3. Create the chunky pixel grid
        vec2 resolution = 1.0 / SCREEN_PIXEL_SIZE;
        vec2 grid_uv = floor(SCREEN_UV * resolution / pixel_size) / (resolution / pixel_size);
        
        // 4. Random glitch offset based on the grid and time
        float noise = rand(grid_uv + TIME * 2.0);
        vec2 offset = vec2(noise * 0.05 - 0.025, 0.0);
        
        // 5. Chromatic Aberration
        float r = texture(screen_texture, grid_uv + offset + vec2(0.01, 0.0)).r;
        float g = texture(screen_texture, grid_uv + offset).g;
        float b = texture(screen_texture, grid_uv + offset - vec2(0.01, 0.0)).b;
        
        vec3 final_color = vec3(r, g, b);
        
        // 6. MULTI-COLOR VIBRANT GLITCH INJECTION
        // We slice the noise value (0.0 to 1.0) into chunks to add intense colors
        if (noise > 0.9) {
            final_color = vec3(1.0, 1.0, 1.0); // Pure White flashes
        } else if (noise > 0.75) {
            final_color = mix(final_color, vec3(1.0, 0.0, 0.8), 0.85); // Aggressive Pink
        } else if (noise > 0.6) {
            final_color = mix(final_color, vec3(0.8, 1.0, 0.0), 0.8);  // Toxic Neon Green
        } else if (noise < 0.1) {
            final_color = mix(final_color, vec3(0.0, 1.0, 1.0), 0.85); // Electric Cyan
        } else if (noise < 0.25) {
            final_color = mix(final_color, vec3(1.0, 0.1, 0.1), 0.8);  // Glitch Red
        }
        
        // 7. Darken the glitch as it gets closer to 1.0 so your text stands out
        final_color *= (1.0 - (progress * 0.5));
        
        COLOR = vec4(final_color, 1.0);
    } else {
        // Center of the screen remains completely untouched and clear
        COLOR = vec4(0.0, 0.0, 0.0, 0.0);
    }
}
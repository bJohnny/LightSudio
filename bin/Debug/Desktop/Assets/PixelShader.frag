#ifdef GL_ES
    precision highp float;
#endif
varying vec3 viewpos;
varying vec3 normal;
uniform vec3 albedo;
uniform float shininess;
uniform float specfactor;
uniform vec3 speccolor;
uniform vec3 ambientcolor;
uniform sampler2D texture;
uniform sampler2D normalTex;
uniform float texmix;
varying vec2 uv;
uniform vec3 lightdir; // paralleles Licht - muss normalisiert werden. Richtung aus der das Licht kommt



void main()
{
    vec3 nnormal = normalize(normal);

    // Diffuse
    //vec3 lightdir = vec3(0, 0, -1);

	vec3 lightdirN = normalize(lightdir);

    float intensityDiff = dot(nnormal, lightdirN);
    vec3 resultingAlbedo = (1.0-texmix) * albedo + texmix * vec3(texture2D(texture, uv));

    // Specular
    float intensitySpec = 0.0;
    if (intensityDiff > 0.0)
    {
        vec3 viewdir = -viewpos;
        vec3 h = normalize(viewdir+lightdirN);
        intensitySpec = specfactor * pow(max(0.0, dot(h, nnormal)), shininess);
    }

    gl_FragColor = vec4(ambientcolor + intensityDiff * resultingAlbedo + intensitySpec * speccolor, 1);
}
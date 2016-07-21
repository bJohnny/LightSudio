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

// paralleles Licht - muss normalisiert werden. Richtung aus der das Licht kommt
//uniform vec3 lightdir; 

uniform vec3 lightposFrontLeft;
uniform vec3 lightposBackLeft;
uniform vec3 lightposFrontRight;
uniform vec3 lightposBackRight;

vec4 ApplyLight(vec3 lightpos, vec3 normal) 
{
	vec3 nnormal = normalize(normal);
	vec3 surfaceToLight = lightpos - viewpos;
	vec3 surfaceToLightN = normalize(surfaceToLight);
	vec3 lightposN = normalize(lightpos);
    float intensityDiff =	dot(nnormal, surfaceToLightN); 
    vec3 resultingAlbedo = (1.0-texmix) * albedo + texmix * vec3(texture2D(texture, uv));

    // Specular
    float intensitySpec = 0.0;
    if (intensityDiff > 0.0)
    {
        vec3 viewdir = -viewpos;
        vec3 h = normalize(viewdir+lightposN);
		//vec3 h = normalize(viewdir+intensityDiff);
        intensitySpec = specfactor * pow(max(0.0, dot(h, nnormal)), shininess);
    }

	return vec4(ambientcolor + intensityDiff * resultingAlbedo + intensitySpec * speccolor, 1);
}

void main()
{
	gl_FragColor = ApplyLight(lightposFrontLeft, normal);
	// apply the other lights. The following returns a bad result
	//gl_FragColor += ApplyLight(lightposBackLeft, normal);
}


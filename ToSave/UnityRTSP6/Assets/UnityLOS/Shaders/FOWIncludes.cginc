// Warning: Editing this file with Visual Studio may cause newlines to change
// causing load errors in Unity

half4 LightingNoLighting(SurfaceOutput s, half3 lightDir, half atten) {
	return half4(s.Albedo * 0.75f, s.Alpha);
}


void GrayBrightFromFOW(half4 fow, out half lightness, out half grayscale) {
	grayscale = 1 - fow.b;
	fow.rg = 1 - pow(1 - saturate(fow.rg * 1.5 - 0.25), 2);
	lightness = (fow.r + fow.g * (1 + fow.b)) / 3;
	lightness *= lightness;
}

half4 TransformColourFOW(half4 c, half4 fow) {
	half lightness, grayscale;
	GrayBrightFromFOW(fow, lightness, grayscale);
	half maxC = max(c.r, max(c.g, c.b));
	half3 t = (c.rgb * lightness);
	return half4(lerp(t.rgb, dot(t, half3(0.5f, 0.4f, 0.1f)).rrr, grayscale), c.a);
}
half4 TransformColourFOWAO(half4 c, half4 fow) {
	c.rgb *= fow.a;
	return TransformColourFOW(c, fow);
}

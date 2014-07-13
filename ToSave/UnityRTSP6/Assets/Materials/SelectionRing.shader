Shader "Custom/Selection Ring" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader {
		Tags { "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 200

		Pass {

			Blend One Zero
			Cull Back
			ZTest LEqual
			ZWrite Off
			Lighting Off

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

float4 _Color;

struct vinp {
    float4 vertex : POSITION;
};

struct v2f {
    float4 pos : SV_POSITION;
};
v2f vert (vinp v) {
    v2f o;
	o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
    return o;
}
half4 frag( v2f i ) : COLOR {
	return _Color;
}
ENDCG

		}
	}
}

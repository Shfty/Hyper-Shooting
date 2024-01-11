Shader "Custom/RTTFogBasePass" {
	SubShader {
		Tags {"VolumeFog" = "" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			Cull Front
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float depth : TEXCOORD0;
			};

			v2f vert (appdata_base v) {
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				
				float4 center = mul (UNITY_MATRIX_MVP, float4(0.0, 0.0, 0.0, 1.0));
				o.depth = o.pos.z - center.z;
				
				return o;
			}

			float frag(v2f i) : SV_Target {
				return i.depth;
			}
			ENDCG
		}
	}
}

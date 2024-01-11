Shader "Custom/RTTFog" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_BaseTexture ("Base Texture", 2D) = ""
	}
	SubShader {
		Tags {"VolumeFog" = "" "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			Fog { Mode Off }
			Cull Back
			Blend OneMinusSrcAlpha SrcAlpha
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform float4 _Color;

			struct v2f {
				float4 pos : POSITION;
				float depth : TEXCOORD0;
				float4 GPScreenPos : TEXCOORD1;
			};

			v2f vert (appdata_base v) {
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				
				float4 center = mul (UNITY_MATRIX_MVP, float4(0.0, 0.0, 0.0, 1.0));
				o.depth = center.z - o.pos.z;
				
				o.GPScreenPos.xy = (float2(o.pos.x, -o.pos.y) + o.pos.w) * 0.5;
				o.GPScreenPos.zw = o.pos.zw;
				
				return o;
			}

			sampler2D _BaseTexture;

			float4 frag(v2f i) : SV_Target {
				float depth = -i.depth;
				float grabDepth = tex2D( _BaseTexture, i.GPScreenPos.xy / i.GPScreenPos.w ).x;
				
				float d = grabDepth - depth;
				d = clamp( d, 0.0, 1.0 );
				
				return float4( _Color.xyz, 1.0 - d );
			}
			ENDCG
		}
	}
}

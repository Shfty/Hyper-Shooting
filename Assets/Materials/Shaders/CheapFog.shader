Shader "Custom/CheapFog" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Density ("Density", Range( 0.0, 10.0 )) = 2.0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		Pass {
			Fog { Mode Off }
			Cull Front
	        ZWrite Off
	        Blend SrcAlpha OneMinusSrcAlpha
	        
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform float4 _Color;
			uniform float _Density;

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

			float4 frag(v2f i) : SV_Target {
				return float4( _Color.xyz, i.depth );
			}
			ENDCG
		}
		Pass {
			Fog { Mode Off }
			Cull Back
	        ZWrite Off
	        Blend SrcAlpha OneMinusSrcAlpha
	        
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform float4 _Color;
			uniform float _Density;

			struct v2f {
				float4 pos : SV_POSITION;
				float depth : TEXCOORD0;
			};

			v2f vert (appdata_base v) {
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				float4 center = mul (UNITY_MATRIX_MVP, float4(0.0, 0.0, 0.0, 1.0));
				o.depth = center.z - o.pos.z;
				
				return o;
			}

			float4 frag(v2f i) : SV_Target {
				return float4( _Color.xyz, i.depth );
			}
			ENDCG
		}
	}
}

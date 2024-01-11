Shader "Custom/VolumeFog" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Density ("Density", Range( 0.0, 1.0 )) = 1.0
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		GrabPass { "_GrabBG" }
		Pass {
			Fog { Mode Off }
			Cull Front
	        ZWrite Off
	        
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

			float4 frag(v2f i) : SV_Target {
				return float4( i.depth, 0.0, 0.0, 0.0 );
			}
			ENDCG
		}
		GrabPass {}
		Pass {
			Fog { Mode Off }
			Cull Back
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			uniform float4 _Color;
			uniform float _Density;

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

			sampler2D _GrabBG;
			sampler2D _GrabTexture;

			float4 frag(v2f i) : SV_Target {
				float depth = -i.depth;
				float grabDepth = tex2D( _GrabTexture, i.GPScreenPos.xy / i.GPScreenPos.w ).x;
				
				float d = ( grabDepth - depth ) * _Density;
				d = clamp( d, 0.0, 1.0 );
				
				float3 bgColor = tex2D( _GrabBG, i.GPScreenPos.xy / i.GPScreenPos.w ).xyz;
				float3 color = _Color.xyz;
				
				float3 blend = ( bgColor * (1.0 - d) ) + ( color * d );
				
				return float4( blend, 1.0 );
			}
			ENDCG
		}
	}
}

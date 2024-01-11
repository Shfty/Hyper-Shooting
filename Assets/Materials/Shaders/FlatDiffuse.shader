Shader "Custom/FlatDiffuse" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		
		Pass {
			Tags { "LightMode" = "ForwardBase" }
		
			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			
			uniform float4 _Color;
			uniform sampler2D _MainTex;
			
			uniform float4 _LightColor0;

			struct v2f {
				float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD0;
				float2 uv : TEXCOORD1;
				LIGHTING_COORDS(2,3)
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_base v) {
				v2f o;
				
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.worldPos = mul (_Object2World, v.vertex);
				
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				
				TRANSFER_VERTEX_TO_FRAGMENT(o);
				
				return o;
			}

			half4 frag(v2f i) : SV_Target {
				float3 xDiff = ddx(i.worldPos).xyz;
				float3 yDiff = ddy(i.worldPos).xyz;
				
				float3 normalDirection = normalize(cross(xDiff, yDiff));
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
				float3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				
				float diff = max (0.0f, dot (normalDirection, lightDirection));
				lightColor += _LightColor0.rgb * ( diff * LIGHT_ATTENUATION(i) );
					
				return half4( tex2D(_MainTex, i.uv).xyz * _Color.xyz * lightColor.xyz, 1.0 );
			}
			ENDCG
		}
	}
	
	Fallback "VertexLit"
}

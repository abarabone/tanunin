

Shader "Custom/BonedStructure v3"
{
	
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		LOD 200
		
		Pass
		{
			Lighting Off
			LOD 200
			
			Tags
			{
				"Queue"="Geometry"
				"IgnoreProjector"="True"
				"RenderType"="Opaque"
				"LightMode"="Vertex"
			}
			Fog { Mode Global }
			
			Cull Back
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			
			CGPROGRAM
			//	#pragma only_renderers d3d9 opengl gles
			//	#pragma glsl
				#pragma vertex vert2
				#pragma fragment frag
			//	#pragma target 3.0
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"
				
				struct vtxpos
				{
					half4	vertex : POSITION;
					half3	normal : NORMAL;
					half2	texcoord : TEXCOORD0;
					half4	color : COLOR;
				};
				
				struct v2f {
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					fixed3	color : COLOR;
					UNITY_FOG_COORDS(2)
				};
								
				
				half3 iShadeVertexLights( half4 vertex, half3 normal )
				{
					half3 viewpos = mul( UNITY_MATRIX_V, vertex ).xyz;
					half3 viewN = mul( (half3x3)UNITY_MATRIX_V, normal );
					half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
					
					half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
					half lengthSq = dot( toLight, toLight );
					half atten = 1.0 / ( 1.0 + lengthSq * unity_LightAtten[0].z );
					half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
					lightColor += unity_LightColor[0].rgb * (diff * atten);
					
					return lightColor;
				}
				half3 iiShadeVertexLights( half4 vertex, half3 normal )
				{
					half3 viewpos = mul( UNITY_MATRIX_V, vertex ).xyz;
					half3 viewN = mul( (half3x3)UNITY_MATRIX_V, normal );
					half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
					
					half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
					half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
					lightColor += unity_LightColor[0].rgb * diff;
					
					return lightColor;
				}
				
				half3 rot( half3 v, half4 q )
				{
					half3	qv = cross( v, q.xyz ) - v * q.w;
					return v + 2.0f * cross( qv, q.xyz );
				}
				

				half4	r[110];
				half4	t[110];

				v2f vert2( vtxpos p )
				{
					v2f o;
					
					int i = D3DCOLORtoUBYTE4(p.color).a;

					half3	rpos = rot( p.vertex.xyz, r[i] );
					half4	tpos = half4( rpos, 0.0f ) + t[i];
					
					o.pos = mul( UNITY_MATRIX_VP, tpos );
					
					o.uv = p.texcoord;
					
					o.color.rgb = iiShadeVertexLights( tpos, rot(p.normal,r[i]) ) * p.color.rgb;
					
					UNITY_TRANSFER_FOG( o, o.pos );

					return o;
				}
				
				
				sampler2D _MainTex;

				fixed4 frag( v2f i ) : COLOR
				{
					fixed3 texcol = tex2D(_MainTex, i.uv);
					
					fixed4 col = fixed4( texcol * i.color, 1.0 );// * _Color;
					
					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
			ENDCG
			
		}
		
	}
	
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Radar"
{
	
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	
	
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		
		Pass
		{
			
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha 
			Lighting Off
			Fog { Mode Off }

			CGPROGRAM
			//	#pragma only_renderers d3d9 opengl gles
			//	#pragma glsl
				#pragma vertex vert3
				#pragma fragment frag
			//	#pragma target 3.0
				
				struct vtxpos
				{
					half4	vertex : POSITION;
					half2	texcoord : TEXCOORD0;
					half4	color : COLOR;
				};
				
				struct v2f
				{
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					half4	color : COLOR;
				};
				

				static const fixed4 elms[ 5 ] =
				{
					fixed4( 1.0f, 0.0f, 0.0f, 0.0f ),
					fixed4( 0.0f, 1.0f, 0.0f, 0.0f ),
					fixed4( 0.0f, 0.0f, 1.0f, 0.0f ),
					fixed4( 0.0f, 0.0f, 0.0f, 1.0f ),
					fixed4( 1.0f, 0.0f, 0.0f, 0.0f )
				};
				
				
				half4	c[8];
				float4	s[2];
				half4	p[202];
				
				v2f vert3( vtxpos v )
				{
					
					v2f o;
					
					
					half i = v.color.a * 255;
					
					
					half ic	= p[i].w;
					
					half classId = floor( ic * (1.0 / 16.0) );
					
					half typeId = ic - classId * 16.0;
					
					half size = dot( s[ classId / 4 ], elms[ classId ] );
					
					
					half4	pos = half4( v.vertex.xy * size + p[i].xy, v.vertex.z, v.vertex.w );
					
					o.pos = UnityObjectToClipPos( pos );
					
					
					o.uv = v.texcoord;
					
					
					fixed3	tcol = c[ typeId ].rgb * (1.0f + p[i].z * 1.8f);
					
					o.color.rgba = fixed4( tcol, 1.0f - abs( p[i].z ) );
					
					
					return o;
					
				}
				
				
				
				
				sampler2D _MainTex;

				fixed4 frag( v2f i ) : COLOR
				{
					fixed4 texcol = tex2D(_MainTex, i.uv);

					return texcol * i.color;
				}
				
			ENDCG
			
		}
		
	}
	
}


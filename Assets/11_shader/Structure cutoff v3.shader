// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Custom/Transparent/Structure Cutoff v3"
{
	
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		_Pallet0 ("Pallet Default", Color) = ( 1, 1, 1, 1 )
		_Pallet1 ("Pallet 0", Color) = ( 1, 1, 1, 1 )
		_Pallet2 ("Pallet 1", Color) = ( 1, 1, 1, 1 )
		_Pallet3 ("Pallet 2", Color) = ( 1, 1, 1, 1 )
		_Pallet4 ("Pallet 3", Color) = ( 1, 1, 1, 1 )
	}
	
	
	SubShader
	{
		//Tags { "RenderType"="TransparentCutout" }
		
		//LOD 200
		
		Pass
		{
			Lighting Off
			LOD 200
			
			Tags
			{
				"Queue"="AlphaTest"
				"IgnoreProjector"="True"
				"RenderType"="TransparentCutout"
				//"Queue"="Geometry"
				//"RenderType"="Opaque"
				"LightMode"="Vertex"
			}
			
			AlphaTest Greater 0.5
			Cull Back
			ZWrite On
			ZTest LEqual
			ColorMask RGBA
			
			CGPROGRAM
			//	#pragma only_renderers d3d9 opengl gles
			//	#pragma glsl
				#pragma vertex vert4
				#pragma fragment frag
			//	#pragma target 3.0
				#pragma multi_compile_fog
				
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				
				
				struct vtxpos
				{
					half4	vertex : POSITION;
					half3	normal : NORMAL;
					half2	texcoord : TEXCOORD0;
					float2	offset : TEXCOORD1;
					fixed4	ids : COLOR;
				};
				
				struct v2f
				{
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					fixed4	col : COLOR;
					UNITY_FOG_COORDS(2)
				};
				
				
				
				half3 iShadeVertexLights( half4 vertex, half3 normal )
				{
					//half3 viewpos = mul( UNITY_MATRIX_MV, vertex ).xyz;
					half3 viewpos = UnityObjectToViewPos( vertex.xyz );
					half3 viewN = mul( (half3x3)UNITY_MATRIX_MV, normal );
					half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
					
					half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
					half lengthSq = dot( toLight, toLight );
					half atten = 1.0h / ( 1.0h + lengthSq * unity_LightAtten[0].z );
					half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
					lightColor += unity_LightColor[0].rgb * (diff * atten);
					
					return lightColor;
				}
				half3 iiShadeVertexLights( half4 vertex, half3 normal )
				{
					//half3 viewpos = mul( UNITY_MATRIX_MV, vertex ).xyz;
					half3 viewpos = UnityObjectToViewPos(vertex.xyz);
					half3 viewN = mul( (half3x3)UNITY_MATRIX_MV, normal );
					half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
					
					half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
					half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
					lightColor += unity_LightColor[0].rgb * diff;
					
					return lightColor;
				}
				
				
				static const fixed4 elms[ 4 ] =
				{
					fixed4( 1.0f, 0.0f, 0.0f, 0.0f ),
					fixed4( 0.0f, 1.0f, 0.0f, 0.0f ),
					fixed4( 0.0f, 0.0f, 1.0f, 0.0f ),
					fixed4( 0.0f, 0.0f, 0.0f, 1.0f )
				};
				
				fixed isExsist( float4 f, float i, float mask )
				{
					
					float	fx = dot( f, elms[ i ] );
					
					float	f1 = fx * mask;// / exp2( mask );//
					
					float	f2 = floor( f1 ) * 0.5f;
					
					return ceil( frac( f2 ) );
					
				}
				
				//float4 _MainTex_ST;
				
				sampler2D _MainTex;
				
				
				fixed4	_Pallet[5];
				
				//float	v[64];//[230]
				float4	v[64];//[230]
				// float だとちゃんと配列になってくれないので、コンパイル後に Vector を Float 配列に置換する必要がある
				//float4	v[16];

				v2f vert4( vtxpos p )
				{
					
					int4	i = D3DCOLORtoUBYTE4(p.ids.bgra);
					half2	ofs = p.offset;
					
					v2f o;

					o.pos = UnityObjectToClipPos( p.vertex ) * isExsist( v[i.z], ofs.y, ofs.x );
					
					o.col = fixed4(iiShadeVertexLights(o.pos, p.normal).rgb * _Pallet[i.r].rgb, 1.0f);
					
					o.uv = p.texcoord;//TRANSFORM_TEX( p.texcoord, _MainTex );
					

					UNITY_TRANSFER_FOG( o, o.pos );

					return o;
				}
				
				
				fixed4 frag( v2f i ) : COLOR
				{
					
					fixed4 texcol = tex2D( _MainTex, i.uv );
					
					fixed4 col = texcol * i.col;

					clip( col.a - 0.1f );

					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
			ENDCG
			
		}
	 	
	}
	
}

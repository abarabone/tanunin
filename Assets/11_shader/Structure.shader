// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'



Shader "Custom/Structure"
{
	
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		//_Pallet0 ("Pallet 0", Color) = ( 1, 1, 1, 1 )
		//_Pallet1 ("Pallet 1", Color) = ( 1, 1, 1, 1 )
		//_Pallet2 ("Pallet 2", Color) = ( 1, 1, 1, 1 )
		//_Pallet3 ("Pallet 3", Color) = ( 1, 1, 1, 1 )
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
				"Queue" = "AlphaTest"
				"IgnoreProjector" = "True"
				"RenderType" = "TransparentCutout"
			//"Queue"="Geometry"
			//"RenderType"="Opaque"
			"LightMode" = "Vertex"
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
				//#pragma target 4.0
				#pragma multi_compile_fog

				//#include "UnityCG.cginc"
				//#include "AutoLight.cginc"
				#include "UnityCG.cginc"
				#include "UnityLightingCommon.cginc"


				struct vtxpos
				{
					half4	position : POSITION;
					half3	normal : NORMAL;
					half2	texcoord : TEXCOORD0;
					fixed4	id : COLOR;
				};

				struct v2f
				{
					half4	pos : SV_POSITION;
					half2	uv : TEXCOORD0;
					fixed4	diff : COLOR;
					UNITY_FOG_COORDS(2)
				};


				//half3 iShadeVertexLights( half4 position, half3 normal )
				//{
				//	//half3 viewpos = mul( UNITY_MATRIX_MV, position ).xyz;
				//	half3 viewpos = UnityObjectToViewPos( position.xyz );
				//	half3 viewN = mul( (half3x3)UNITY_MATRIX_MV, normal );
				//	half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				//	
				//	half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
				//	half lengthSq = dot( toLight, toLight );
				//	half atten = 1.0h / ( 1.0h + lengthSq * unity_LightAtten[0].z );
				//	half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
				//	lightColor += unity_LightColor[0].rgb * (diff * atten);
				//	
				//	return lightColor;
				//}
				//half3 iiShadeVertexLights( half4 position, half3 normal )
				//{
				//	//half3 viewpos = mul( UNITY_MATRIX_MV, position ).xyz;
				//	half3 viewpos = UnityObjectToViewPos(position.xyz);
				//	half3 viewN = mul( (half3x3)UNITY_MATRIX_MV, normal );
				//	half3 lightColor = UNITY_LIGHTMODEL_AMBIENT.xyz;
				//	
				//	half3 toLight = unity_LightPosition[0].xyz - viewpos.xyz * unity_LightPosition[0].w;
				//	half diff = max( 0.0h, dot(viewN, normalize(toLight)) );
				//	lightColor += unity_LightColor[0].rgb * diff;
				//	
				//	return lightColor;
				//}



				//float4 _MainTex_ST;

				sampler2D _MainTex;

				fixed4	_Pallet[4];

				int4 isVisibleFlags[8];
				// unity は、int4[] をシェーダーに送れない。（ちなみに mpb では Color[] もダメ、float4[] として贈る）
				// 配列は SetVectorArray() でセットすることになる。
				// この時、float に int イメージを詰めても、シェーダーにはあくまでも float4 として渡しているようで、
				// int->float -転送-> (int4)float4 としてキャストされて受け取る…となってるっぽい？？
				// （試した結果、実用的には 24bit フラグまで行けるみたい。
				//    float の実数部は 23bit で、1 bit 付加されるという話があるから、そこがぎりぎりなのだろう）
				// （ SetVectorArray() -> GetVectorArray() で取り出すと、
				//    int イメージとして復元できるので、深い部分でコンバート処理がされてるのだと思う）

				v2f vert4( vtxpos v )
				{
					
					int4	id = D3DCOLORtoUBYTE4(v.id.rgba);
					
					int palletIdx = id.a;
					int arrIdx = id.r;
					int elmIdx = id.g;
					int maskOfIsVisible = 1 << id.b;

					bool isVisible = ( isVisibleFlags[arrIdx][elmIdx] & maskOfIsVisible ) != 0;
					//bool isVisible = (isVisibleFlags & maskOfIsVisible) != 0;
					float visibleValue = isVisible ? 1.0f : 0.0f;

					v2f o;

					o.pos = UnityObjectToClipPos( v.position ) * visibleValue;
					
					o.uv = v.texcoord;//TRANSFORM_TEX( v.texcoord, _MainTex );

					//o.diff = fixed4(iiShadeVertexLights(o.pos, v.normal).rgb * _Pallet[id.a].rgb, 1.0f);
					half3 worldNormal = UnityObjectToWorldNormal(v.normal);
					half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
					o.diff = nl * _LightColor0;
					o.diff.rgb *= _Pallet[palletIdx].rgb;
					o.diff.rgb += ShadeSH9(half4(worldNormal, 1));


					UNITY_TRANSFER_FOG( o, o.pos );

					return o;
				}
				
				
				fixed4 frag( v2f i ) : COLOR
				{
					
					fixed4 texcol = tex2D( _MainTex, i.uv );
					fixed4 col = texcol * i.diff;
					//clip( col.a - 0.1f );

					UNITY_APPLY_FOG( i.fogCoord, col );

					return col;
				}
				
			ENDCG
			
		}
	 	
	}
	
}

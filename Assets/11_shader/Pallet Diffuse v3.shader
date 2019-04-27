Shader "Custom/Pallet Diffuse v3"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		
		_PalletIdOverride("Pallet Id Overrid", Float) = -1
		
		_Pallet0 ("Pallet Default", Color) = ( 1, 1, 1, 1 )
		
		_Pallet1 ("Pallet 0", Color) = ( 1, 1, 1, 1 )
		_Pallet2 ("Pallet 1", Color) = ( 1, 1, 1, 1 )
		_Pallet3 ("Pallet 2", Color) = ( 1, 1, 1, 1 )
		_Pallet4 ("Pallet 3", Color) = ( 1, 1, 1, 1 )
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;

		struct Input {
			float2	uv_MainTex;
			fixed3	color;
		};
		
		
		fixed4	_Pallet[5];
		half	_PalletIdOverride;
		
		void vert( inout appdata_full v, out Input o )
		{
			
			int isOverride = any( _PalletIdOverride + 1 );
						
			int ic = (v.color.r * 255) * (1 - isOverride) + _PalletIdOverride * isOverride;
			
			o.color.rgb = _Pallet[ic].rgb;
			
			o.uv_MainTex.xy = v.texcoord.xy;
			
		}
				
		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * IN.color.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

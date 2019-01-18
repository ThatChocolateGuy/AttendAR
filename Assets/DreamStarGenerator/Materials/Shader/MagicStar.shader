// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DreamstarGenerator/MagicStar"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1.0, 1.0, 1.0, 1.0)

		_ScrollTex ("Scroll Texture", 2D) = "white" {}
		_ScrollColor ("Scroll Color", Color) = (1.0, 1.0, 1.0, 1.0)



		[Header(Reflection)]	
		_Reflection("Reflection", Cube) = "dummy.jpg" {}
		_ReflectionColor("Reflection Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_ReflectionMulti("Reflection Multiplier", float) = 1
		
		[Header(Animation Effects)]
		_RotationSpeed("Rotation Speed", Float) = 2.0

		_ScrollDirection_X("Scroll Direction X", float) = 0
		_ScrollDirection_Y("Scroll Direction Y", float) = 0
		
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		
		LOD 100

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
					
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;				
				float3 normalDir : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float4 vertex : SV_POSITION;
				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			
			sampler2D _ScrollTex;
			float4 _ScrollTex_ST;
			float4 _ScrollColor;


			samplerCUBE _Reflection;
			float4 _ReflectionColor;
			float _ReflectionMulti;
			
			
			
			float _RotationSpeed;
			float _ScrollSpeed;
			float _ScrollDirection_X;
			float _ScrollDirection_Y;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);	
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.viewDir = normalize(WorldSpaceViewDir(v.vertex)) * -2;
				o.normalDir = normalize(v.normal);		
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float sinX = sin(_RotationSpeed * _Time);
				float cosX = cos(_RotationSpeed * _Time);
				float sinY = sin(_RotationSpeed * _Time);
				float3x3 rotationMatrix = float3x3
				(cosX, -sinX, 0,
					sinY, cosX, 0,
					0, 0, 1);

				float3 uvrot = 

				col = tex2D(_MainTex, i.uv);

				float3 reflectedDir = reflect(i.viewDir + i.normalDir, normalize(i.normalDir));	
				float4 reflectCol = texCUBE(_Reflection, mul(reflectedDir, rotationMatrix));
				reflectCol *= _ReflectionColor;
				reflectCol *= _ReflectionMulti; 
				reflectCol *= _Color.a;

				float4 scrolling = _ScrollTex_ST;
				scrolling.b += _ScrollDirection_X * _Time;
				scrolling.a += _ScrollDirection_Y * _Time;

				fixed4 scrollcol = tex2D(_ScrollTex, (i.uv + scrolling.ba) * scrolling);
				scrollcol *= _ScrollColor;
				
				float4 finalColor = (col* _Color) + reflectCol + scrollcol;
				finalColor.a = col.a * _Color.a;
			
				


				return finalColor ;
			}
			ENDCG
		}
	}
}

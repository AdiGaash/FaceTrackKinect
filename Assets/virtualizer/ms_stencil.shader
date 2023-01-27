// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "yak/projectScreen" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
	[MaterialToggle] _flipped("flipped", Float) = 1
		//[MaterialToggle] _calcPerVertex("calc per vertex (high premormance)", Float) = 0
	}

		SubShader{
		//Tags {"Queue" = "Transparent" "IgnoreProjector" = "true"}
		Tags{ "IgnoreProjector" = "true" }
		LOD 200
		Lighting Off
		//Cull Back

		Pass{

		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One Zero //?
		//Blend One Zero //?

		CGPROGRAM
		// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members vertex)
		//#pragma exclude_renderers d3d11 xbox360

		// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members pos2)
		//#pragma exclude_renderers d3d11 xbox360

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		float4 _Color;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	bool _flipped;
	//bool _calcPerVertex;

	//--------------------------------

	struct v2f {
		float4 pos:SV_POSITION;
		float2 uv:TEXCOORD0;
		float4 screenPos : TEXCOORD1; // <-- screen!!!
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		//o.screenPos = ComputeScreenPosForProjection(mul(_ProjMVP, v.vertex)); // <-- screen!!!
		//o.screenPos = ComputeScreenPosForProjection(o.pos); // <-- screen!!!
		//o.screenPos = ComputeScreenPos(mul(mul(UNITY_MATRIX_MV, unity_CameraProjection), v.vertex)); // <-- screen!!!
		//o.screenPos = ComputeScreenPos(v.vertex); // <-- screen!!!
		o.screenPos = ComputeScreenPos(o.pos); // <-- screen!!!
		o.screenPos.xy = o.screenPos.xy / o.screenPos.w;
		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		float4 screenPos = i.screenPos;

		if (_flipped) {
			screenPos.y = 1 - screenPos.y;
		}

		half4 proj = tex2D(_MainTex, screenPos.xy);
		proj = proj*_Color;
		proj.a = _Color.a;
		return proj;
	}
		ENDCG

	}
	}

		FallBack "Diffuse"
}
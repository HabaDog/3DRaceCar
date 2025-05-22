Shader "Custom/HalfColorShader"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,0,0,1)    // 上半部分颜色（默认红色）
        _BottomColor ("Bottom Color", Color) = (1,1,1,1)  // 下半部分颜色（默认白色）
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Threshold ("Split Threshold", Range(-1,1)) = 0.0  // 分割线的位置
        _BlendZone ("Blend Zone Size", Range(0,1)) = 0.1  // 过渡区域大小
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input
        {
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _TopColor;
        fixed4 _BottomColor;
        float _Threshold;
        float _BlendZone;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 获取物体在世界空间中的高度（y坐标）
            float height = IN.worldPos.y;
            
            // 计算混合因子
            float blend = smoothstep(_Threshold - _BlendZone, _Threshold + _BlendZone, height);
            
            // 混合上下两种颜色
            fixed4 c = lerp(_BottomColor, _TopColor, blend);
            
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
} 
//  This file is part of the Structure SDK.
//  Copyright © 2015 Occipital, Inc. All rights reserved.
//  http://structure.io

Shader "TransparentOccluder" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
 
SubShader {
    // Force this shader to render before the other opaque shaders (like the ball/cat)
    // by subtracting 100 from the default opaque queue ordering
    Tags {"RenderType"="Geometry" "Queue"="Geometry-100"}

    // Render a standard transparent object, but explicity write to the depth buffer
    Pass {
        ZWrite On
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Material {
            Diffuse [_Color]
            Ambient [_Color]
        }
        Lighting On
        SetTexture [_MainTex] {
            Combine texture * primary DOUBLE, texture * primary
        } 
    }
}
}

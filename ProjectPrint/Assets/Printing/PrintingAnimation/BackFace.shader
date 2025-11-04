Shader "Custom/StencilMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" "RenderPipeline" = "UniversalPipeline"}

        Pass 
        {
            Blend Zero One
            ZWrite Off
            Cull Front
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
                // CompFront Always
                // PassFront Zero
            }
        }
    }
}
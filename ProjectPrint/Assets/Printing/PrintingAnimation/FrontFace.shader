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
            Cull Back
            Stencil
            {
                Ref 2
                Comp Always
                Pass Zero
                // CompFront Always
                // PassFront Zero
            }
        }
    }
}
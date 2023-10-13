Shader "CG/StencilMask"
{
    Properties{}

        SubShader{

        Tags {
         "RenderType" = "Opaque"
         }

         Pass{
         ZWrite Off
         }
    }
}
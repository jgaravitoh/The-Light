

#ifndef UNIVERSAL_GLOBAL_ILLUMINATION_INCLUDED
#define UNIVERSAL_GLOBAL_ILLUMINATION_INCLUDED

#include "../../Pipeline/Core/ShaderLibrary/EntityLighting.hlsl"
#include "../../Pipeline/Core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "../../Pipeline/ShaderLibrary/RealtimeLights.hlsl"
#include "../../Pipeline/ShaderLibrary/CommonOperations.hlsl"

#define AMBIENT_PROBE_BUFFER 0
#include "../../Pipeline/Core/ShaderLibrary/AmbientProbe.hlsl"

#if defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
#include "../../Pipeline/Core/Runtime/Lighting/ProbeVolume/ProbeVolume.hlsl"
#endif
#if USE_FORWARD_PLUS
#include "../../Pipeline/Core/ShaderLibrary/Packing.hlsl"
#endif

// If lightmap is not defined than we evaluate GI (ambient + probes) from SH

// Renamed -> LIGHTMAP_SHADOW_MIXING
#if !defined(_MIXED_LIGHTING_SUBTRACTIVE) && defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
    #define _MIXED_LIGHTING_SUBTRACTIVE
#endif

#if defined (_AKMU_CARPAINT)
half3 GetCustomRotationIrradiance(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion, half3 normalWS, half3 viewDirectionWS)
{
    reflectVector = LocalCorrect(reflectVector, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
	
    float normalFaceIndex = -1;
    float reflectFaceIndex = -1;
    half3 tiledReflectVector = reflectVector;



  //  float boxedNormalFaceIndex = -1;
  //  float boxedReflectFaceIndex = -1;
  //  half3 boxedTiledReflectVector = reflectVector;

  //  //if (_EnableRealTimeReflection == 1)
  //  //{
  //  //    tiledReflectVector = LocalCorrect(tiledReflectVector, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
  //  //}

  //  if ((_EnableRealTimeReflection == 2 || _EnableRealTimeReflection == 50))
		////&&
		////_Marker == 1.)
  //  {

		////reset rotation (only x will be active)
    float4 rotation = _EnviRotation;

		////get current faceIndex before x rotation
		////tiledReflectVector = LocalCorrect(tiledReflectVector, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
		////normalWS = LocalCorrect(normalWS, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
    normalFaceIndex = getFaceIndex(normalWS);
    reflectFaceIndex = getFaceIndex(tiledReflectVector);
   
    if (reflectFaceIndex == 0) //right
		//||
		//normalFaceIndex == 0
		//||
		//normalFaceIndex == 4 //front
		//||
		//normalFaceIndex == 5) //back
    {
        reflectFaceIndex = 0; //collect on same value to sample same map

        rotation.y = -rotation.x; //rotate reverse x on y
        rotation.x = 0;
        rotation.z = 0;
    }
    if (reflectFaceIndex == 1)  //left
    {
        reflectFaceIndex = 1; //collect on same value to sample same map

        rotation.y = rotation.x; //rotate on y
        rotation.x = 0;
        rotation.z = 0;
    }
    if (reflectFaceIndex == 2) //top
    {
        reflectFaceIndex = 0; //collect on same value to sample same map

        rotation.y = -rotation.x; //no change
        rotation.x = 0;
        rotation.z = 0;
    }
    if (reflectFaceIndex == 3) //bottom
    {
        reflectFaceIndex = 1; //collect on same value to sample same map

        rotation.y = 0;
        rotation.x = rotation.x;
        rotation.z = 0;
    }
    if (reflectFaceIndex == 4) //front
    {
        reflectFaceIndex = 1; //collect on same value to sample same map

        rotation.y = -rotation.x; //rotate reverse x on y
        rotation.x = 0;
        rotation.z = 0;
    }
    if (reflectFaceIndex == 5) //back
    {
        reflectFaceIndex = 0; //collect on same value to sample same map

        rotation.y = -rotation.x; //rotate reverse x on y
        rotation.x = 0;
        rotation.z = 0;
    }
    

		////rotate according to our movement
  //      tiledReflectVector = CreateRotation(tiledReflectVector, rotation);

		////test!!!!!!!!!!!!!!!!!!!
  //      //float faceIndex = normalFaceIndex;
  //      //if (faceIndex == 0)
  //      //    return float3(255. / 255, 0, 0); //0 RED - right
  //      //if (faceIndex == 1)
  //      //    return float3(0, 0, 255. / 255); //blue - left
  //      //if (faceIndex == 2)
  //      //    return float3(0, 255. / 255, 0); //green - top
  //      //if (faceIndex == 3)
  //      //    return float3(255. / 255, 125. / 255, 0); //orange - bottom
  //      //if (faceIndex == 4)
  //      //    return float3(0, 255. / 255, 255. / 255); //cyan - front
  //      //if (faceIndex == 5)
  //      //    return float3(255. / 255, 255. / 255, 0); //yellow - back
  //  }

  ////  if (_Marker == 0.)
  ////  {
		//////switch to box
  ////      _EnableRealTimeReflection = 4;
		////////rotate around y
		//////float4 rotation = _EnviRotation;
		//////rotation.y = -rotation.x;
		//////rotation.x = 0;
		//////rotation.z = 0;
		//////boxedTiledReflectVector = CreateRotation(boxedTiledReflectVector, rotation);
  ////  }


 //   //float faceIndex = 1;
	//float angleRight = 0;
	//float angleBack = 0;
	//float angleBackRight = 0;
	//float angleFront = 0;
	//float angleFrontRight = 0;
	
	//half3 tiledReflectVector = reflectVector;

	
	//float4 rotation = _EnviRotation;
	//rotation.y = 0;
	//rotation.z = 0;

	////calculate the reflection direction accordingto normal and the right side of the car
	//angleRight = radianV(normalWS, half3(1, 0, 0));
	//angleBack = radianV(normalWS, half3(0, 0, -1));
	//angleBackRight = radianV(normalWS, half3(1, 0, -1));
	//angleFront = radianV(normalWS, half3(0, 0, 1));
	//angleFrontRight = radianV(normalWS, half3(1, 0, 1));

	////cos 0 and 45 is
	////    1 and 0.70710678   <- using them may create good speed??? todo:

	//if (angleRight >= 0.70710678 && angleRight < 1)
	//{
	//    rotation.y = rotation.x;
	//    rotation.x = 0;
	//}
	//else if ((angleBack >= 0.70710678 && angleBack < 1)
	//    ||
	//    (angleBackRight >= 0.70710678 && angleBackRight < 1)
	//    ||
	//    (angleFront >= 0.70710678 && angleFront < 1)
	//    ||
	//    (angleFrontRight >= 0.70710678 && angleFrontRight < 1))
	//{
	//    rotation.y = -rotation.x;
	//    rotation.x = 0;

	//}
	//else {
	//    //so for the top etc. reflection will be a little flat but have nice visuals
	//    //tiledReflectVector = normalWS;
	//    tiledReflectVector = LocalCorrect(tiledReflectVector, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
	//}

	tiledReflectVector = CreateRotation(tiledReflectVector, rotation);
    
    half3 irradiance = 0;
    //half3 irradianceCube = 0;
    //half3 irradianceBox = 0;
    
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    //half4 encodedIrradiance = 0;
    half4 encodedIrradianceCube = 0;
    //half4 encodedIrradianceBox = 0;
    
    if (reflectFaceIndex == 0) //same value to sample side
    {
        encodedIrradianceCube = SAMPLE_TEXTURECUBE_LOD(_EnviCubeMapSecondary, sampler_EnviCubeMapSecondary,
				tiledReflectVector, mip + _EnviCubeMapLength.w);
        irradiance = DecodeHDREnvironment(encodedIrradianceCube, _EnviCubeMapSecondary_HDR);
    }
    if (reflectFaceIndex == 1) //same value to sample top
    {
        encodedIrradianceCube = SAMPLE_TEXTURECUBE_LOD(_EnviCubeMapMain, sampler_EnviCubeMapMain, tiledReflectVector, mip);
        irradiance = DecodeHDREnvironment(encodedIrradianceCube, _EnviCubeMapMain_HDR);
    }
    
    return irradiance;
    
    //todo: close
    //return float3(1,0,0);
}
#endif

// SH Vertex Evaluation. Depending on target SH sampling might be
// done completely per vertex or mixed with L2 term per vertex and L0, L1
// per pixel. See SampleSHPixel
half3 SampleSHVertex(half3 normalWS)
{
#if defined(EVALUATE_SH_VERTEX)
    return EvaluateAmbientProbeSRGB(normalWS);
#elif defined(EVALUATE_SH_MIXED)
    // no max since this is only L2 contribution
    return SHEvalLinearL2(normalWS, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
#endif

    // Fully per-pixel. Nothing to compute.
    return half3(0.0, 0.0, 0.0);
}

// SH Pixel Evaluation. Depending on target SH sampling might be done
// mixed or fully in pixel. See SampleSHVertex
half3 SampleSHPixel(half3 L2Term, half3 normalWS)
{
#if defined(EVALUATE_SH_VERTEX)
    return L2Term;
#elif defined(EVALUATE_SH_MIXED)
    half3 res = L2Term + SHEvalLinearL0L1(normalWS, unity_SHAr, unity_SHAg, unity_SHAb);
#ifdef UNITY_COLORSPACE_GAMMA
    res = LinearToSRGB(res);
#endif
    return max(half3(0, 0, 0), res);
#endif

    // Default: Evaluate SH fully per-pixel
    return EvaluateAmbientProbeSRGB(normalWS);
}

// APV Prove volume
// Vertex and Mixed both use Vertex sampling

#if (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
half3 SampleProbeVolumeVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, out float4 probeOcclusion)
{
    probeOcclusion = 1.0;

#if defined(EVALUATE_SH_VERTEX) || defined(EVALUATE_SH_MIXED)
    half3 bakedGI;
    // The screen space position is used for noise, which is irrelevant when doing vertex sampling
    float2 positionSS = float2(0, 0);
    if (_EnableProbeVolumes)
    {
        EvaluateAdaptiveProbeVolume(absolutePositionWS, normalWS, viewDir, positionSS, GetMeshRenderingLayer(), bakedGI, probeOcclusion);
    }
    else
    {
        bakedGI = EvaluateAmbientProbe(normalWS);
    }
#ifdef UNITY_COLORSPACE_GAMMA
    bakedGI = LinearToSRGB(bakedGI);
#endif
    return bakedGI;
#else
    return half3(0, 0, 0);
#endif
}

half3 SampleProbeVolumePixel(in half3 vertexValue, in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, in float2 positionSS, in float4 vertexProbeOcclusion, out float4 probeOcclusion)
{
    probeOcclusion = 1.0;

#if defined(EVALUATE_SH_VERTEX) || defined(EVALUATE_SH_MIXED)
    probeOcclusion = vertexProbeOcclusion;
    return vertexValue;
#elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
    half3 bakedGI;
    if (_EnableProbeVolumes)
    {
        EvaluateAdaptiveProbeVolume(absolutePositionWS, normalWS, viewDir, positionSS, GetMeshRenderingLayer(), bakedGI, probeOcclusion);
    }
    else
    {
        bakedGI = EvaluateAmbientProbe(normalWS);
    }
#ifdef UNITY_COLORSPACE_GAMMA
        bakedGI = LinearToSRGB(bakedGI);
#endif
    return bakedGI;
#else
    return half3(0, 0, 0);
#endif
}

half3 SampleProbeVolumePixel(in half3 vertexValue, in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, in float2 positionSS)
{
    float4 unusedProbeOcclusion = 0;
    return SampleProbeVolumePixel(vertexValue, absolutePositionWS, normalWS, viewDir, positionSS, unusedProbeOcclusion, unusedProbeOcclusion);
}
#endif

half3 SampleProbeSHVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir, out float4 probeOcclusion)
{
    probeOcclusion = 1.0;

#if (defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2))
    return SampleProbeVolumeVertex(absolutePositionWS, normalWS, viewDir, probeOcclusion);
#else
    return SampleSHVertex(normalWS);
#endif
}

half3 SampleProbeSHVertex(in float3 absolutePositionWS, in float3 normalWS, in float3 viewDir)
{
    float4 unusedProbeOcclusion = 0;
    return SampleProbeSHVertex(absolutePositionWS, normalWS, viewDir, unusedProbeOcclusion);
}

#if defined(UNITY_DOTS_INSTANCING_ENABLED) && !defined(USE_LEGACY_LIGHTMAPS)
// ^ GPU-driven rendering is enabled, and we haven't opted-out from lightmap
// texture arrays. This minimizes batch breakages, but texture arrays aren't
// supported in a performant way on all GPUs.
#define LIGHTMAP_NAME unity_Lightmaps
#define LIGHTMAP_INDIRECTION_NAME unity_LightmapsInd
#define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmaps
#define LIGHTMAP_SAMPLE_EXTRA_ARGS staticLightmapUV, unity_LightmapIndex.x
#else
// ^ Lightmaps are not bound as texture arrays, but as individual textures. The
// batch is broken every time lightmaps are changed, but this is well-supported
// on all GPUs.
#define LIGHTMAP_NAME unity_Lightmap
#define LIGHTMAP_INDIRECTION_NAME unity_LightmapInd
#define LIGHTMAP_SAMPLER_NAME samplerunity_Lightmap
#define LIGHTMAP_SAMPLE_EXTRA_ARGS staticLightmapUV
#endif

// Sample baked and/or realtime lightmap. Non-Direction and Directional if available.
half3 SampleLightmap(float2 staticLightmapUV, float2 dynamicLightmapUV, half3 normalWS)
{
    // The shader library sample lightmap functions transform the lightmap uv coords to apply bias and scale.
    // However, universal pipeline already transformed those coords in vertex. We pass half4(1, 1, 0, 0) and
    // the compiler will optimize the transform away.
    half4 transformCoords = half4(1, 1, 0, 0);

    float3 diffuseLighting = 0;

#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
    diffuseLighting = SampleDirectionalLightmap(TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME),
        TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_INDIRECTION_NAME, LIGHTMAP_SAMPLER_NAME),
        LIGHTMAP_SAMPLE_EXTRA_ARGS, transformCoords, normalWS, true);
#elif defined(LIGHTMAP_ON)
    diffuseLighting = SampleSingleLightmap(TEXTURE2D_LIGHTMAP_ARGS(LIGHTMAP_NAME, LIGHTMAP_SAMPLER_NAME), LIGHTMAP_SAMPLE_EXTRA_ARGS, transformCoords, true);
#endif

#if defined(DYNAMICLIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
    diffuseLighting += SampleDirectionalLightmap(TEXTURE2D_ARGS(unity_DynamicLightmap, samplerunity_DynamicLightmap),
        TEXTURE2D_ARGS(unity_DynamicDirectionality, samplerunity_DynamicLightmap),
         dynamicLightmapUV, transformCoords, normalWS, false);
#elif defined(DYNAMICLIGHTMAP_ON)
    diffuseLighting += SampleSingleLightmap(TEXTURE2D_ARGS(unity_DynamicLightmap, samplerunity_DynamicLightmap),
         dynamicLightmapUV, transformCoords, false);
#endif

    return diffuseLighting;
}

// Legacy version of SampleLightmap where Realtime GI is not supported.
half3 SampleLightmap(float2 staticLightmapUV, half3 normalWS)
{
    float2 dummyDynamicLightmapUV = float2(0,0);
    half3 result = SampleLightmap(staticLightmapUV, dummyDynamicLightmapUV, normalWS);
    return result;
}

// We either sample GI from baked lightmap or from probes.
// If lightmap: sampleData.xy = lightmapUV
// If probe: sampleData.xyz = L2 SH terms
#if defined(LIGHTMAP_ON) && defined(DYNAMICLIGHTMAP_ON)
#define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) SampleLightmap(staticLmName, dynamicLmName, normalWSName)
#elif defined(DYNAMICLIGHTMAP_ON)
#define SAMPLE_GI(staticLmName, dynamicLmName, shName, normalWSName) SampleLightmap(0, dynamicLmName, normalWSName)
#elif defined(LIGHTMAP_ON)
#define SAMPLE_GI(staticLmName, shName, normalWSName) SampleLightmap(staticLmName, 0, normalWSName)
#elif defined(PROBE_VOLUMES_L1) || defined(PROBE_VOLUMES_L2)
#ifdef USE_APV_PROBE_OCCLUSION
    #define SAMPLE_GI(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion) SampleProbeVolumePixel(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion)
#else
    #define SAMPLE_GI(shName, absolutePositionWS, normalWS, viewDir, positionSS, vertexProbeOcclusion, probeOcclusion) SampleProbeVolumePixel(shName, absolutePositionWS, normalWS, viewDir, positionSS)
#endif
#else
#define SAMPLE_GI(staticLmName, shName, normalWSName) SampleSHPixel(shName, normalWSName)
#endif

half3 BoxProjectedCubemapDirection(half3 reflectionWS, float3 positionWS, float4 cubemapPositionWS, float4 boxMin, float4 boxMax)
{
    // Is this probe using box projection?
    if (cubemapPositionWS.w > 0.0f)
    {
        float3 boxMinMax = (reflectionWS > 0.0f) ? boxMax.xyz : boxMin.xyz;
        half3 rbMinMax = half3(boxMinMax - positionWS) / reflectionWS;

        half fa = half(min(min(rbMinMax.x, rbMinMax.y), rbMinMax.z));

        half3 worldPos = half3(positionWS - cubemapPositionWS.xyz);

        half3 result = worldPos + reflectionWS * fa;
        return result;
    }
    else
    {
        return reflectionWS;
    }
}

float CalculateProbeWeight(float3 positionWS, float4 probeBoxMin, float4 probeBoxMax)
{
    float blendDistance = probeBoxMax.w;
    float3 weightDir = min(positionWS - probeBoxMin.xyz, probeBoxMax.xyz - positionWS) / blendDistance;
    return saturate(min(weightDir.x, min(weightDir.y, weightDir.z)));
}

half CalculateProbeVolumeSqrMagnitude(float4 probeBoxMin, float4 probeBoxMax)
{
    half3 maxToMin = half3(probeBoxMax.xyz - probeBoxMin.xyz);
    return dot(maxToMin, maxToMin);
}

half3 CalculateIrradianceFromReflectionProbes(half3 reflectVector, float3 positionWS, half perceptualRoughness, float2 normalizedScreenSpaceUV)
{
    half3 irradiance = half3(0.0h, 0.0h, 0.0h);
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
#if USE_FORWARD_PLUS
    float totalWeight = 0.0f;
    uint probeIndex;
    ClusterIterator it = ClusterInit(normalizedScreenSpaceUV, positionWS, 1);
    [loop] while (ClusterNext(it, probeIndex) && totalWeight < 0.99f)
    {
        probeIndex -= URP_FP_PROBES_BEGIN;

        float weight = CalculateProbeWeight(positionWS, urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
        weight = min(weight, 1.0f - totalWeight);

        half3 sampleVector = reflectVector;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        sampleVector = BoxProjectedCubemapDirection(reflectVector, positionWS, urp_ReflProbes_ProbePosition[probeIndex], urp_ReflProbes_BoxMin[probeIndex], urp_ReflProbes_BoxMax[probeIndex]);
#endif // _REFLECTION_PROBE_BOX_PROJECTION

        uint maxMip = (uint)abs(urp_ReflProbes_ProbePosition[probeIndex].w) - 1;
        half probeMip = min(mip, maxMip);
        float2 uv = saturate(PackNormalOctQuadEncode(sampleVector) * 0.5 + 0.5);

        float mip0 = floor(probeMip);
        float mip1 = mip0 + 1;
        float mipBlend = probeMip - mip0;
        float4 scaleOffset0 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip0];
        float4 scaleOffset1 = urp_ReflProbes_MipScaleOffset[probeIndex * 7 + (uint)mip1];

        half3 irradiance0 = half4(SAMPLE_TEXTURE2D_LOD(urp_ReflProbes_Atlas, sampler_LinearClamp, uv * scaleOffset0.xy + scaleOffset0.zw, 0.0)).rgb;
        half3 irradiance1 = half4(SAMPLE_TEXTURE2D_LOD(urp_ReflProbes_Atlas, sampler_LinearClamp, uv * scaleOffset1.xy + scaleOffset1.zw, 0.0)).rgb;
        irradiance += weight * lerp(irradiance0, irradiance1, mipBlend);
        totalWeight += weight;
    }
#else
    half probe0Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    half probe1Volume = CalculateProbeVolumeSqrMagnitude(unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

    half volumeDiff = probe0Volume - probe1Volume;
    float importanceSign = unity_SpecCube1_BoxMin.w;

    // A probe is dominant if its importance is higher
    // Or have equal importance but smaller volume
    bool probe0Dominant = importanceSign > 0.0f || (importanceSign == 0.0f && volumeDiff < -0.0001h);
    bool probe1Dominant = importanceSign < 0.0f || (importanceSign == 0.0f && volumeDiff > 0.0001h);

    float desiredWeightProbe0 = CalculateProbeWeight(positionWS, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
    float desiredWeightProbe1 = CalculateProbeWeight(positionWS, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);

    // Subject the probes weight if the other probe is dominant
    float weightProbe0 = probe1Dominant ? min(desiredWeightProbe0, 1.0f - desiredWeightProbe1) : desiredWeightProbe0;
    float weightProbe1 = probe0Dominant ? min(desiredWeightProbe1, 1.0f - desiredWeightProbe0) : desiredWeightProbe1;

    float totalWeight = weightProbe0 + weightProbe1;

    // If either probe 0 or probe 1 is dominant the sum of weights is guaranteed to be 1.
    // If neither is dominant this is not guaranteed - only normalize weights if totalweight exceeds 1.
    weightProbe0 /= max(totalWeight, 1.0f);
    weightProbe1 /= max(totalWeight, 1.0f);

    // Sample the first reflection probe
    if (weightProbe0 > 0.01f)
    {
        half3 reflectVector0 = reflectVector;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        reflectVector0 = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
#endif // _REFLECTION_PROBE_BOX_PROJECTION

        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector0, mip));

        irradiance += weightProbe0 * DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    }

    // Sample the second reflection probe
    if (weightProbe1 > 0.01f)
    {
        half3 reflectVector1 = reflectVector;
#ifdef _REFLECTION_PROBE_BOX_PROJECTION
        reflectVector1 = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
#endif // _REFLECTION_PROBE_BOX_PROJECTION
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube1, samplerunity_SpecCube1, reflectVector1, mip));

        irradiance += weightProbe1 * DecodeHDREnvironment(encodedIrradiance, unity_SpecCube1_HDR);
    }
#endif

    // Use any remaining weight to blend to environment reflection cube map
    if (totalWeight < 0.99f)
    {
        half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_GlossyEnvironmentCubeMap, sampler_GlossyEnvironmentCubeMap, reflectVector, mip));

        irradiance += (1.0f - totalWeight) * DecodeHDREnvironment(encodedIrradiance, _GlossyEnvironmentCubeMap_HDR);
    }

    return irradiance;
}

half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion, float2 normalizedScreenSpaceUV, half3 normalWS, half3 viewDirectionWS)
{
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
    half3 irradiance;
	half4 encodedIrradiance;
	half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    
    half3 reflectVectorForMix = reflectVector;

    #if defined(_REFLECTION_PROBE_BLENDING)
	    #if defined (_AKMU_CARPAINT)
		    if (_EnableRealTimeReflection == 1)
		    {
			    reflectVector = LocalCorrect(reflectVector, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
			
			    encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_EnviCubeMapMain, sampler_EnviCubeMapMain, reflectVector, mip));
			    //if reflection is black; mix with other probes
			    if (all(encodedIrradiance.rgb == half3(0, 0, 0)))
			    {
				    irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness, normalizedScreenSpaceUV);
			    }
			    else //get reflection from our probe
			    {
				    irradiance = DecodeHDREnvironment(encodedIrradiance, _EnviCubeMapMain_HDR);
			    }
		    }
            else if (_EnableRealTimeReflection == 2)
		    {
                irradiance = GetCustomRotationIrradiance(reflectVector, positionWS, perceptualRoughness, occlusion, normalWS, viewDirectionWS);
                half3 irradianceReal = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness, normalizedScreenSpaceUV);
                irradiance = lerp(irradianceReal, irradiance, _MixMultiplier);
            }
		    else
		    {
			    irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness, normalizedScreenSpaceUV);
		    }
	    #else
		    irradiance = CalculateIrradianceFromReflectionProbes(reflectVector, positionWS, perceptualRoughness, normalizedScreenSpaceUV);
	    #endif
    #else
        #ifdef _REFLECTION_PROBE_BOX_PROJECTION
		    reflectVector = BoxProjectedCubemapDirection(reflectVector, positionWS, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
        #endif // _REFLECTION_PROBE_BOX_PROJECTION

        #if defined (_AKMU_CARPAINT)
	        if (_EnableRealTimeReflection == 1)
	        {
			    reflectVector = LocalCorrect(reflectVectorForMix, _BBoxMin.xyz, _BBoxMax.xyz, positionWS, _EnviCubeMapPos.xyz);
    
		        encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_EnviCubeMapMain, sampler_EnviCubeMapMain, reflectVector, mip));
		        //if reflection is black; mix with other probes
		        if (all(encodedIrradiance.rgb == half3(0, 0, 0)))
		        {
			        encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
			        irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
		        }
		        else //get from our probe
		        {
			        irradiance = DecodeHDREnvironment(encodedIrradiance, _EnviCubeMapMain_HDR);
		        }
	        }
            else if (_EnableRealTimeReflection == 2)
            {
                irradiance = GetCustomRotationIrradiance(reflectVectorForMix, positionWS, perceptualRoughness, occlusion, normalWS, viewDirectionWS);
                encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
	            half3 irradianceReal = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
                irradiance = lerp(irradianceReal, irradiance, _MixMultiplier);
            }
	        else
	        {
		        encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
		        irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
	        }
        #else
	        encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
	        irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
        #endif
    #endif // _REFLECTION_PROBE_BLENDING
    return irradiance * occlusion * _CustomBrightnessIntensity;
#else
    return _GlossyEnvironmentColor.rgb * occlusion * _CustomBrightnessIntensity;
#endif // _ENVIRONMENTREFLECTIONS_OFF
}

//#if !USE_FORWARD_PLUS
//half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, half occlusion, half3 normalWS, half3 viewDirectionWS)
//{
//    return GlossyEnvironmentReflection(reflectVector, positionWS, perceptualRoughness, occlusion, float2(0.0f, 0.0f), normalWS, viewDirectionWS);
//}
//#endif

half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness, 
    half occlusion, half3 normalWS, half3 viewDirectionWS)
{
#if !defined(_ENVIRONMENTREFLECTIONS_OFF)
    half3 irradiance;
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
	half4 encodedIrradiance;
#if defined (_AKMU_CARPAINT)
	if (_EnableRealTimeReflection == 1)
	{
		encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(_EnviCubeMapMain, sampler_EnviCubeMapMain, reflectVector, mip));
		if (all(encodedIrradiance.rgb == half3(0, 0, 0)))
		{
			encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
			irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
		}
		else 
		{
			irradiance = DecodeHDREnvironment(encodedIrradiance, _EnviCubeMapMain_HDR);
		}
	}
    else if (_EnableRealTimeReflection == 2)
    {
        irradiance = GetCustomRotationIrradiance(reflectVector, positionWS, perceptualRoughness, occlusion, normalWS, viewDirectionWS);
        encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
	    half3 irradianceReal = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
        irradiance = lerp(irradianceReal, irradiance, _MixMultiplier);
    }
	else
	{
		encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
		irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
	}
#else
	encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
	irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
#endif

    return irradiance * occlusion * _CustomBrightnessIntensity;
#else

    return _GlossyEnvironmentColor.rgb * occlusion * _CustomBrightnessIntensity;
#endif // _ENVIRONMENTREFLECTIONS_OFF
}

half3 SubtractDirectMainLightFromLightmap(Light mainLight, half3 normalWS, half3 bakedGI)
{
    // Let's try to make realtime shadows work on a surface, which already contains
    // baked lighting and shadowing from the main sun light.
    // Summary:
    // 1) Calculate possible value in the shadow by subtracting estimated light contribution from the places occluded by realtime shadow:
    //      a) preserves other baked lights and light bounces
    //      b) eliminates shadows on the geometry facing away from the light
    // 2) Clamp against user defined ShadowColor.
    // 3) Pick original lightmap value, if it is the darkest one.


    // 1) Gives good estimate of illumination as if light would've been shadowed during the bake.
    // We only subtract the main direction light. This is accounted in the contribution term below.
    half shadowStrength = GetMainLightShadowStrength();
    half contributionTerm = saturate(dot(mainLight.direction, normalWS));
    half3 lambert = mainLight.color * contributionTerm;
    half3 estimatedLightContributionMaskedByInverseOfShadow = lambert * (1.0 - mainLight.shadowAttenuation);
    half3 subtractedLightmap = bakedGI - estimatedLightContributionMaskedByInverseOfShadow;

    // 2) Allows user to define overall ambient of the scene and control situation when realtime shadow becomes too dark.
    half3 realtimeShadow = max(subtractedLightmap, _SubtractiveShadowColor.xyz);
    realtimeShadow = lerp(bakedGI, realtimeShadow, shadowStrength);

    // 3) Pick darkest color
    return min(bakedGI, realtimeShadow);
}

half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS,
    half3 normalWS, half3 viewDirectionWS, float2 normalizedScreenSpaceUV)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    #if defined (_AKMU_LCRS) && (_LCRS_PROBE_ROTATION)
        reflectVector = CreateRotation(_EnviRotation, reflectVector);
    #endif
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, 1.0h, normalizedScreenSpaceUV,
        normalWS, viewDirectionWS);

    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

    if (IsOnlyAOLightingFeatureEnabled())
    {
        color = half3(1,1,1); // "Base white" for AO debug lighting mode
    }

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfDataClearCoat.perceptualRoughness, 1.0h, 
        normalizedScreenSpaceUV, normalWS, viewDirectionWS);
    // TODO: "grazing term" causes problems on full roughness
    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

    // Blend with base layer using khronos glTF recommended way using NoV
    // Smooth surface & "ambiguous" lighting
    // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
    return (color * (1.0 - coatFresnel * clearCoatMask) + coatColor) * occlusion;
#else
    return color * occlusion;
#endif
}

//#if !USE_FORWARD_PLUS
//half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
//    half3 bakedGI, half occlusion, float3 positionWS,
//    half3 normalWS, half3 viewDirectionWS)
//{
//    return GlobalIllumination(brdfData, brdfDataClearCoat, clearCoatMask, bakedGI, occlusion, positionWS, normalWS, viewDirectionWS, float2(0.0f, 0.0f));
//}
//#endif

//// Backwards compatiblity
//half3 GlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS, half3 normalWS, half3 viewDirectionWS)
//{
//    const BRDFData noClearCoat = (BRDFData)0;
//    return GlobalIllumination(brdfData, noClearCoat, 0.0, bakedGI, occlusion, positionWS, normalWS, viewDirectionWS, 0);
//}

half3 GlobalIllumination(BRDFData brdfData, BRDFData brdfDataClearCoat, float clearCoatMask,
    half3 bakedGI, half occlusion, float3 positionWS, 
    half3 normalWS, half3 viewDirectionWS)
{
    half3 reflectVector = reflect(-viewDirectionWS, normalWS);
    half NoV = saturate(dot(normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);

    half3 indirectDiffuse = bakedGI;
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness, half(1.0), float2(0.0f, 0.0f), normalWS, viewDirectionWS);
   
    half3 color = EnvironmentBRDF(brdfData, indirectDiffuse, indirectSpecular, fresnelTerm);

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half3 coatIndirectSpecular = GlossyEnvironmentReflection(reflectVector, positionWS, brdfDataClearCoat.perceptualRoughness, half(1.0),
        float2(0.0f, 0.0f), normalWS, viewDirectionWS);
    // TODO: "grazing term" causes problems on full roughness
    half3 coatColor = EnvironmentBRDFClearCoat(brdfDataClearCoat, clearCoatMask, coatIndirectSpecular, fresnelTerm);

    // Blend with base layer using khronos glTF recommended way using NoV
    // Smooth surface & "ambiguous" lighting
    // NOTE: fresnelTerm (above) is pow4 instead of pow5, but should be ok as blend weight.
    half coatFresnel = kDielectricSpec.x + kDielectricSpec.a * fresnelTerm;
    return (color * (1.0 - coatFresnel * clearCoatMask) + coatColor) * occlusion;
#else
    return color * occlusion;
#endif
}


half3 GlobalIllumination(BRDFData brdfData, half3 bakedGI, half occlusion, float3 positionWS, half3 normalWS, half3 viewDirectionWS)
{
    const BRDFData noClearCoat = (BRDFData) 0;
    return GlobalIllumination(brdfData, noClearCoat, 0.0, bakedGI, occlusion, positionWS, normalWS, viewDirectionWS);
}

void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI)
{
#if defined(LIGHTMAP_ON) && defined(_MIXED_LIGHTING_SUBTRACTIVE)
    bakedGI = SubtractDirectMainLightFromLightmap(light, normalWS, bakedGI);
#endif
}

// Backwards compatibility
void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI, half4 shadowMask)
{
    MixRealtimeAndBakedGI(light, normalWS, bakedGI);
}

void MixRealtimeAndBakedGI(inout Light light, half3 normalWS, inout half3 bakedGI, AmbientOcclusionFactor aoFactor)
{
    if (IsLightingFeatureEnabled(DEBUGLIGHTINGFEATUREFLAGS_AMBIENT_OCCLUSION))
    {
        bakedGI *= aoFactor.indirectAmbientOcclusion;
    }

    MixRealtimeAndBakedGI(light, normalWS, bakedGI);
}

#endif

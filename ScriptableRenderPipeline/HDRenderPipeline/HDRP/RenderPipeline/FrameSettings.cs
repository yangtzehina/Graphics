﻿using System;
using UnityEngine.XR;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    // The settings here are per frame settings.
    // Each camera must have its own per frame settings
    [Serializable]
    public class FrameSettings
    {
        public static string kEnableShadow = "Enable Shadows";
        public static string kEnableContactShadows = "Enable Contact Shadows";
        public static string kEnableSSR = "Enable SSR";
        public static string kEnableSSAO = "Enable SSAO";
        public static string kEnableSubsurfaceScattering = "Enable SubsurfaceScattering";
        public static string kEnableTransmission = "Enable Transmission";

        public static string kForwardOnly = "Forward Only";
        public static string kDeferredDepthPrepass = "Deferred Depth Prepass";
        public static string kDeferredDepthPrepassATestOnly = "Deferred Depth Prepass ATest Only";

        public static string KEnableTransparentPrepass = "Enable Transparent Prepass";
        public static string kEnableMotionVectors = "Enable Motion Vectors";
        public static string KEnableObjectMotionVectors = "Enable Object Motion Vectors";
        public static string kEnableDBuffer = "Enable DBuffer";
        public static string kEnableAtmosphericScattering = "Enable Atmospheric Scattering";
        public static string kEnableRoughRefraction = "Enable Rough Refraction";
        public static string kEnableTransparentPostpass = "Enable Transparent Postpass";
        public static string kEnableDistortion = "Enable Distortion";
        public static string kEnablePostprocess = "Enable Postprocess";

        public static string kEnableStereoRendering = "Enable Stereo Rendering";
        public static string kEnableAsyncCompute = "Enable Async Compute";

        public static string kEnableOpaqueObjects = "Enable Opaque Objects";
        public static string kEnableTransparentObjects = "Enable Transparent Objects";

        public static string kEnableMSAA = "Enable MSAA";
        public static string kEnableShadowMask = "Enable ShadowMask";

        // Lighting
        // Setup by users
        public bool enableShadow = true;
        public bool enableContactShadows = true;
        public bool enableSSR = true; // Depends on DepthPyramid
        public bool enableSSAO = true;
        public bool enableSubsurfaceScattering = true;
        public bool enableTransmission = true;  // Caution: this is only for debug, it doesn't save the cost of Transmission execution

        // Setup by system
        public float diffuseGlobalDimmer = 1.0f;
        public float specularGlobalDimmer = 1.0f;

        // View
        public bool enableForwardRenderingOnly = false; // TODO: Currently there is no way to strip the extra forward shaders generated by the shaders compiler, so we can switch dynamically.
        public bool enableDepthPrepassWithDeferredRendering = false;
        public bool enableAlphaTestOnlyInDeferredPrepass = false;

        public bool enableTransparentPrepass = true;
        public bool enableMotionVectors = true; // Enable/disable whole motion vectors pass (Camera + Object).
        public bool enableObjectMotionVectors = true;
        public bool enableDBuffer = true;
        public bool enableAtmosphericScattering = true;
        public bool enableRoughRefraction = true; // Depends on DepthPyramid - If not enable, just do a copy of the scene color (?) - how to disable rough refraction ?
        public bool enableTransparentPostpass = true;
        public bool enableDistortion = true;
        public bool enablePostprocess = true;

        public bool enableStereo = true;
        public bool enableAsyncCompute = true;

        public bool enableOpaqueObjects = true;
        public bool enableTransparentObjects = true;

        public bool enableMSAA = false;

        public bool enableShadowMask = false;

        public LightLoopSettings lightLoopSettings = new LightLoopSettings();

        public void CopyTo(FrameSettings frameSettings)
        {
            frameSettings.enableShadow = this.enableShadow;
            frameSettings.enableContactShadows = this.enableContactShadows;
            frameSettings.enableSSR = this.enableSSR;
            frameSettings.enableSSAO = this.enableSSAO;
            frameSettings.enableSubsurfaceScattering = this.enableSubsurfaceScattering;
            frameSettings.enableTransmission = this.enableTransmission;

            frameSettings.diffuseGlobalDimmer = this.diffuseGlobalDimmer;
            frameSettings.specularGlobalDimmer = this.specularGlobalDimmer;

            frameSettings.enableForwardRenderingOnly = this.enableForwardRenderingOnly;
            frameSettings.enableDepthPrepassWithDeferredRendering = this.enableDepthPrepassWithDeferredRendering;
            frameSettings.enableAlphaTestOnlyInDeferredPrepass = this.enableAlphaTestOnlyInDeferredPrepass;

            frameSettings.enableTransparentPrepass = this.enableTransparentPrepass;
            frameSettings.enableMotionVectors = this.enableMotionVectors;
            frameSettings.enableObjectMotionVectors = this.enableObjectMotionVectors;
            frameSettings.enableDBuffer = this.enableDBuffer;
            frameSettings.enableAtmosphericScattering = this.enableAtmosphericScattering;
            frameSettings.enableRoughRefraction = this.enableRoughRefraction;
            frameSettings.enableTransparentPostpass = this.enableTransparentPostpass;
            frameSettings.enableDistortion = this.enableDistortion;
            frameSettings.enablePostprocess = this.enablePostprocess;

            frameSettings.enableStereo = this.enableStereo;

            frameSettings.enableOpaqueObjects = this.enableOpaqueObjects;
            frameSettings.enableTransparentObjects = this.enableTransparentObjects;

            frameSettings.enableAsyncCompute = this.enableAsyncCompute;

            frameSettings.enableMSAA = this.enableMSAA;

            frameSettings.enableShadowMask = this.enableShadowMask;

            this.lightLoopSettings.CopyTo(frameSettings.lightLoopSettings);
        }

        // Init a FrameSettings from renderpipeline settings, frame settings and debug settings (if any)
        // This will aggregate the various option
        public static void InitializeFrameSettings(Camera camera, RenderPipelineSettings renderPipelineSettings, FrameSettings srcFrameSettings, ref FrameSettings aggregate)
        {
            if (aggregate == null)
                aggregate = new FrameSettings();

            // When rendering reflection probe we disable specular as it is view dependent
            if (camera.cameraType == CameraType.Reflection)
            {
                aggregate.diffuseGlobalDimmer = 1.0f;
                aggregate.specularGlobalDimmer = 0.0f;
            }
            else
            {
                aggregate.diffuseGlobalDimmer = 1.0f;
                aggregate.specularGlobalDimmer = 1.0f;
            }

            aggregate.enableShadow = srcFrameSettings.enableShadow;
            aggregate.enableContactShadows = srcFrameSettings.enableContactShadows;
            aggregate.enableSSR = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableSSR && renderPipelineSettings.supportSSR;
            aggregate.enableSSAO = srcFrameSettings.enableSSAO && renderPipelineSettings.supportSSAO;
            aggregate.enableSubsurfaceScattering = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableSubsurfaceScattering && renderPipelineSettings.supportSubsurfaceScattering;
            aggregate.enableTransmission = srcFrameSettings.enableTransmission;

            // We have to fall back to forward-only rendering when scene view is using wireframe rendering mode
            // as rendering everything in wireframe + deferred do not play well together
            aggregate.enableForwardRenderingOnly = srcFrameSettings.enableForwardRenderingOnly || GL.wireframe || renderPipelineSettings.supportsForwardOnly;
            aggregate.enableDepthPrepassWithDeferredRendering = srcFrameSettings.enableDepthPrepassWithDeferredRendering;
            aggregate.enableAlphaTestOnlyInDeferredPrepass = srcFrameSettings.enableAlphaTestOnlyInDeferredPrepass;

            aggregate.enableTransparentPrepass = srcFrameSettings.enableTransparentPrepass;
            aggregate.enableMotionVectors = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableMotionVectors && renderPipelineSettings.supportsMotionVectors;
            aggregate.enableObjectMotionVectors = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableObjectMotionVectors && renderPipelineSettings.supportsMotionVectors;
            aggregate.enableDBuffer = srcFrameSettings.enableDBuffer && renderPipelineSettings.supportDBuffer;
            aggregate.enableAtmosphericScattering = srcFrameSettings.enableAtmosphericScattering;
            aggregate.enableRoughRefraction = srcFrameSettings.enableRoughRefraction;
            aggregate.enableTransparentPostpass = srcFrameSettings.enableTransparentPostpass;
            aggregate.enableDistortion = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableDistortion;

            // Planar and real time cubemap doesn't need post process and render in FP16
            aggregate.enablePostprocess = camera.cameraType != CameraType.Reflection && srcFrameSettings.enablePostprocess;

            aggregate.enableStereo = camera.cameraType != CameraType.Reflection && srcFrameSettings.enableStereo && XRSettings.isDeviceActive && (camera.stereoTargetEye == StereoTargetEyeMask.Both) && renderPipelineSettings.supportsStereo;
            // Force forward if we request stereo. TODO: We should not enforce that, users should be able to chose deferred
            aggregate.enableForwardRenderingOnly = aggregate.enableForwardRenderingOnly || aggregate.enableStereo;

            aggregate.enableAsyncCompute = srcFrameSettings.enableAsyncCompute && SystemInfo.supportsAsyncCompute;

            aggregate.enableOpaqueObjects = srcFrameSettings.enableOpaqueObjects;
            aggregate.enableTransparentObjects = srcFrameSettings.enableTransparentObjects;

            aggregate.enableMSAA = srcFrameSettings.enableMSAA && renderPipelineSettings.supportMSAAAntiAliasing;
            if (QualitySettings.antiAliasing < 1)
                aggregate.enableMSAA = false;
            aggregate.ConfigureMSAADependentSettings();

            aggregate.enableShadowMask = srcFrameSettings.enableShadowMask && renderPipelineSettings.supportShadowMask;

            LightLoopSettings.InitializeLightLoopSettings(camera, aggregate, renderPipelineSettings, srcFrameSettings, ref aggregate.lightLoopSettings);
        }

        public void ConfigureMSAADependentSettings()
        {
            if (enableMSAA)
            {
                // Initially, MSAA will only support forward
                enableForwardRenderingOnly = true;

                // TODO: Should we disable enableFptlForForwardOpaque in here, instead of in InitializeLightLoopSettings?
                // We'd have to move this method to after InitializeLightLoopSettings if we did.  It would be nice to centralize
                // all MSAA-dependent settings in this method.

                // Assuming MSAA is being used, TAA, and therefore, motion vectors are not needed
                enableMotionVectors = false;

                // TODO: The work will be implemented piecemeal to support all passes
                enableDBuffer = false; // no decals
                enableDistortion = false; // no gaussian final color
                enablePostprocess = false;
                enableRoughRefraction = false; // no gaussian pre-refraction
                enableSSAO = false;
                enableSSR = false;
                enableSubsurfaceScattering = false;
                enableTransparentObjects = false; // waiting on depth pyramid generation
            }
        }

        static public void RegisterDebug(String menuName, FrameSettings frameSettings)
        {
            // Register the camera into the debug menu
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableShadow, () => frameSettings.enableShadow, (value) => frameSettings.enableShadow = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableContactShadows, () => frameSettings.enableContactShadows, (value) => frameSettings.enableContactShadows = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableSSR, () => frameSettings.enableSSR, (value) => frameSettings.enableSSR = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableSSAO, () => frameSettings.enableSSAO, (value) => frameSettings.enableSSAO = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableSubsurfaceScattering, () => frameSettings.enableSubsurfaceScattering, (value) => frameSettings.enableSubsurfaceScattering = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableTransmission, () => frameSettings.enableTransmission, (value) => frameSettings.enableTransmission = (bool)value);

            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kForwardOnly, () => frameSettings.enableForwardRenderingOnly, (value) => frameSettings.enableForwardRenderingOnly = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kDeferredDepthPrepass, () => frameSettings.enableDepthPrepassWithDeferredRendering, (value) => frameSettings.enableDepthPrepassWithDeferredRendering = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kDeferredDepthPrepassATestOnly, () => frameSettings.enableAlphaTestOnlyInDeferredPrepass, (value) => frameSettings.enableAlphaTestOnlyInDeferredPrepass = (bool)value);

            DebugMenuManager.instance.AddDebugItem<bool>(menuName, KEnableTransparentPrepass, () => frameSettings.enableTransparentPrepass, (value) => frameSettings.enableTransparentPrepass = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableMotionVectors, () => frameSettings.enableMotionVectors, (value) => frameSettings.enableMotionVectors = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, KEnableObjectMotionVectors, () => frameSettings.enableObjectMotionVectors, (value) => frameSettings.enableObjectMotionVectors = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableDBuffer, () => frameSettings.enableDBuffer, (value) => frameSettings.enableDBuffer = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableAtmosphericScattering, () => frameSettings.enableAtmosphericScattering, (value) => frameSettings.enableAtmosphericScattering = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableRoughRefraction, () => frameSettings.enableRoughRefraction, (value) => frameSettings.enableRoughRefraction = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableTransparentPostpass, () => frameSettings.enableTransparentPostpass, (value) => frameSettings.enableTransparentPostpass = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableDistortion, () => frameSettings.enableDistortion, (value) => frameSettings.enableDistortion = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnablePostprocess, () => frameSettings.enablePostprocess, (value) => frameSettings.enablePostprocess = (bool)value);

            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableStereoRendering, () => frameSettings.enableStereo, (value) => frameSettings.enableStereo = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableAsyncCompute, () => frameSettings.enableAsyncCompute, (value) => frameSettings.enableAsyncCompute = (bool)value);

            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableOpaqueObjects, () => frameSettings.enableOpaqueObjects, (value) => frameSettings.enableOpaqueObjects = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableTransparentObjects, () => frameSettings.enableTransparentObjects, (value) => frameSettings.enableTransparentObjects = (bool)value);

            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableMSAA, () => frameSettings.enableMSAA, (value) => frameSettings.enableMSAA = (bool)value);
            DebugMenuManager.instance.AddDebugItem<bool>(menuName, kEnableShadowMask, () => frameSettings.enableShadowMask, (value) => frameSettings.enableShadowMask = (bool)value);

            LightLoopSettings.RegisterDebug(menuName, frameSettings.lightLoopSettings);
       }

        static public void UnRegisterDebug(String menuName)
        {
            // Register the camera into the debug menu
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableShadow);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableSSR);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableSSAO);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableSubsurfaceScattering);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableTransmission);

            DebugMenuManager.instance.RemoveDebugItem(menuName, kForwardOnly);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kDeferredDepthPrepassATestOnly);
            DebugMenuManager.instance.RemoveDebugItem(menuName, KEnableTransparentPrepass);

            DebugMenuManager.instance.RemoveDebugItem(menuName, KEnableTransparentPrepass);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableMotionVectors);
            DebugMenuManager.instance.RemoveDebugItem(menuName, KEnableObjectMotionVectors);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableDBuffer);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableAtmosphericScattering);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableRoughRefraction);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableTransparentPostpass);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableDistortion);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnablePostprocess);

            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableStereoRendering);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableAsyncCompute);

            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableOpaqueObjects);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableTransparentObjects);

            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableMSAA);
            DebugMenuManager.instance.RemoveDebugItem(menuName, kEnableShadowMask);

            LightLoopSettings.UnRegisterDebug(menuName);
        }
    }
}

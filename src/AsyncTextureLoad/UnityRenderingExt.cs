using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AsyncTextureLoad;

[StructLayout(LayoutKind.Sequential)]
public struct UnityRenderingExtTextureUpdateParamsV2
{
    /// <summary>
    /// The source data for the texture update. Must be set by the plugin.
    /// </summary>
    public IntPtr texData;

    /// <summary>
    /// User defined data. Set by the plugin.
    /// </summary>
    public uint userData;

    /// <summary>
    /// The texture ID of the texture to be updated.
    /// </summary>
    public uint textureID;

    /// <summary>
    /// The format of the texture to be updated.
    /// </summary>
    public TextureFormat format;

    /// <summary>
    /// The width of the texture.
    /// </summary>
    public uint width;

    /// <summary>
    /// The height of the texture.
    /// </summary>
    public uint height;

    /// <summary>
    /// Texture bytes per pixel.
    /// </summary>
    public uint bpp;
}

enum UnityRenderingExtEventType
{
    /// <summary>
    /// issued during SetStereoTarget and carrying the current 'eye' index as parameter
    /// </summary>
    SetStereoTarget,

    /// <summary>
    /// issued during stereo rendering at the beginning of each eye's rendering loop. It carries the current 'eye' index as parameter
    /// </summary>
    SetStereoEye,

    /// <summary>
    /// issued after the rendering has finished
    /// </summary>
    StereoRenderingDone,

    /// <summary>
    /// issued during BeforeDrawCall and carrying UnityRenderingExtBeforeDrawCallParams as parameter
    /// </summary>
    BeforeDrawCall,

    /// <summary>
    /// issued during AfterDrawCall. This event doesn't carry any parameters
    /// </summary>
    AfterDrawCall,

    /// <summary>
    /// issued during GrabIntoRenderTexture since we can't simply copy the resources
    /// when custom rendering is used - we need to let plugin handle this. It carries over
    /// a UnityRenderingExtCustomBlitParams params = { X, source, dest, 0, 0 } ( X means it's irrelevant )
    /// </summary>
    CustomGrab,

    /// <summary>
    ///  issued by plugin to insert custom blits. It carries over UnityRenderingExtCustomBlitParams as param.
    /// </summary>
    CustomBlit,
    UpdateTextureBegin, // Deprecated.
    UpdateTextureEnd, // Deprecated.

    /// <summary>
    /// Deprecated. Issued to update a texture. It carries over UnityRenderingExtTextureUpdateParamsV1
    /// </summary>
    UpdateTextureBeginV1 = UpdateTextureBegin,

    /// <summary>
    /// Deprecated. Issued to signal the plugin that the texture update has finished. It carries over the same UnityRenderingExtTextureUpdateParamsV1 as kUnityRenderingExtEventUpdateTextureBeginV1
    /// </summary>
    UpdateTextureEndV1 = UpdateTextureEnd,

    /// <summary>
    /// Issued to update a texture. It carries over UnityRenderingExtTextureUpdateParamsV2
    /// </summary>
    UpdateTextureBeginV2,

    /// <summary>
    /// Issued to signal the plugin that the texture update has finished. It carries over the same UnityRenderingExtTextureUpdateParamsV2 as kUnityRenderingExtEventUpdateTextureBeginV2
    /// </summary>
    UpdateTextureEndV2,

    // keep this last
    EventCount,
    UserEventsStart = EventCount,
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KSP.UI.Screens;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Profiling;

namespace AsyncTextureLoad;

[KSPAddon(KSPAddon.Startup.AllGameScenes, once: false)]
internal class DebugUI : MonoBehaviour
{
    const int DefaultWidth = 600;
    const int DefaultHeight = 100;
    const int CloseButtonSize = 15;
    const int CloseButtonMargin = 5;

    static ApplicationLauncherButton button;
    static Texture2D ButtonTexture;
    static bool InitializedStatics = false;

    Rect window;
    bool showGUI = false;
    int iterations = 50;

    void Start()
    {
        if (!InitializedStatics)
        {
            ButtonTexture = GameDatabase.Instance.GetTexture(
                "AsyncTextureLoad/Textures/ToolbarIcon",
                false
            );
            InitializedStatics = true;
        }

        window = new Rect(
            Screen.width / 2 - DefaultWidth / 2,
            Screen.height / 2 - DefaultHeight / 2,
            DefaultWidth,
            DefaultHeight
        );

        if (button != null)
            return;

        button = ApplicationLauncher.Instance.AddModApplication(
            ShowToolbarGUI,
            HideToolbarGUI,
            Nothing,
            Nothing,
            Nothing,
            Nothing,
            ApplicationLauncher.AppScenes.ALWAYS,
            ButtonTexture
        );
    }

    void ShowToolbarGUI()
    {
        showGUI = true;
    }

    void HideToolbarGUI()
    {
        showGUI = false;
    }

    void Nothing() { }

    void OnGUI()
    {
        if (!showGUI)
            return;

        window = GUILayout.Window(
            GetInstanceID(),
            window,
            DrawWindow,
            "Async Texture Load",
            HighLogic.Skin.window
        );
    }

    void DrawWindow(int windowId)
    {
        using var skin = new PushGUISkin(HighLogic.Skin);

        var closeButtonRect = new Rect(
            window.width - CloseButtonSize - CloseButtonMargin,
            CloseButtonMargin,
            CloseButtonSize,
            CloseButtonSize
        );
        if (GUI.Button(closeButtonRect, "X"))
            HideToolbarGUI();

        GUILayout.BeginVertical();

        if (GUILayout.Button("Load PNG"))
            DoLoadPNG();
        if (GUILayout.Button("Load DDS"))
            DoLoadDDS();
        if (GUILayout.Button("Load Asset Bundle"))
            DoLoadAssetBundle();

        GUILayout.EndVertical();
    }

    void DoLoadPNG()
    {
        var path = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/AsyncTextureLoad/Textures/Kerbin_Color.png"
        );
        var uri = new Uri(path);
        for (int i = 0; i < iterations; ++i)
        {
            using var sample = new Sample("LoadPNG");
            var request = UnityWebRequestTexture.GetTexture(uri, nonReadable: true);
            StartCoroutine(LoadPNGCoroutine(request));
        }
    }

    IEnumerator LoadPNGCoroutine(UnityWebRequest request)
    {
        yield return request.SendWebRequest();

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError($"Failed to load texture: {request.error}");
            yield break;
        }

        _ = DownloadHandlerTexture.GetContent(request);
    }

    void DoLoadDDS()
    {
        for (int i = 0; i < iterations; ++i)
        {
            using var sample = new Sample("LoadDDS");
            TextureLoadManager.LoadTextureAsync(
                "AsyncTextureLoad/Textures/Kerbin_Color.dds",
                false,
                true
            );
        }
    }

    void DoLoadAssetBundle()
    {
        var path = Path.Combine(
            KSPUtil.ApplicationRootPath,
            "GameData/AsyncTextureLoad/Textures/kerbin"
        );
        var uri = new Uri(path);

        using var sample = new Sample("LoadAssetBundle");
        var request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        StartCoroutine(LoadAssetBundleCoroutine(request, "Assets/Textures/Kerbin_Color.dds"));
    }

    IEnumerator LoadAssetBundleCoroutine(UnityWebRequest request, string name)
    {
        yield return request.SendWebRequest();

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.LogError($"Failed to load asset bundle: {request.error}");
            yield break;
        }

        var bundle = DownloadHandlerAssetBundle.GetContent(request);

        List<Coroutine> coroutines = [];
        for (int i = 0; i < iterations; ++i)
        {
            coroutines.Add(StartCoroutine(LoadAssetBundleTexture(bundle, name)));
        }

        foreach (var coro in coroutines)
            yield return coro;

        bundle.Unload(false);
    }

    IEnumerator LoadAssetBundleTexture(AssetBundle bundle, string name)
    {
        var texreq = bundle.LoadAssetAsync<Texture2D>(name);
        yield return texreq;

        _ = (Texture2D)texreq.asset;
    }

    readonly struct PushGUISkin : IDisposable
    {
        readonly GUISkin prev;

        public PushGUISkin(GUISkin skin)
        {
            prev = GUI.skin;
            GUI.skin = skin;
        }

        public readonly void Dispose()
        {
            GUI.skin = prev;
        }
    }
}

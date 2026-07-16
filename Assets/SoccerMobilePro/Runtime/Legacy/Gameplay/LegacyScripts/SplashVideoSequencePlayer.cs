namespace SoccerMobilePro.Legacy.Gameplay
{
using SoccerMobilePro.Legacy.Compatibility;
using GUIText = SoccerMobilePro.Legacy.Compatibility.GUIText;
using GUITexture = SoccerMobilePro.Legacy.Compatibility.GUITexture;
using SoccerMobilePro.Legacy.TeamSelection;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

[UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "SplashVideoSequencePlayer")]

public sealed class SplashVideoSequencePlayer : MonoBehaviour
{
    public VideoClip[] clips;
    public RawImage targetImage;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public string nextSceneName = SoccerMobilePro.Platform.SceneIds.MainMenu;
    public float prepareTimeoutSeconds = 8f;

    private RenderTexture renderTexture;
    private int lastScreenWidth = -1;
    private int lastScreenHeight = -1;

    private void Awake()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (targetImage == null)
        {
            targetImage = CreateRuntimeTargetImage();
        }

        ConfigureTargetImage();
        DisableLegacySplashTextures();
        RemoveSplashAudioManagers();
        EnsureAudioListener();
        ConfigureVideoPlayer();
    }

    private IEnumerator Start()
    {
        if (clips != null)
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    yield return PlayClip(clips[i]);
                }
            }
        }

        LoadNextScene();
    }

    private void LateUpdate()
    {
        if (!HasScreenSizeChanged())
        {
            return;
        }

        ConfigureTargetImage();

        if (videoPlayer != null)
        {
            EnsureRenderTexture();
        }
    }

    private void ConfigureVideoPlayer()
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.isLooping = false;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.FitOutside;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.DSPTime;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        if (audioSource != null)
        {
            ConfigureAudioSource(audioSource);
        }
    }

    private static void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.mute = false;
        source.volume = 1f;
        source.spatialBlend = 0f;
    }

    private RawImage CreateRuntimeTargetImage()
    {
        GameObject canvasObject = new GameObject("SplashVideoCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject backgroundObject = new GameObject("BlackBackground");
        backgroundObject.transform.SetParent(canvasObject.transform, false);
        Image background = backgroundObject.AddComponent<Image>();
        background.color = Color.black;
        Stretch(background.rectTransform);

        GameObject videoObject = new GameObject("VideoImage");
        videoObject.transform.SetParent(canvasObject.transform, false);
        RawImage rawImage = videoObject.AddComponent<RawImage>();
        rawImage.color = Color.white;
        Stretch(rawImage.rectTransform);
        return rawImage;
    }

    private void ConfigureTargetImage()
    {
        if (targetImage == null)
        {
            return;
        }

        AspectRatioFitter fitter = targetImage.GetComponent<AspectRatioFitter>();
        if (fitter != null)
        {
            fitter.enabled = false;
        }

        Stretch(targetImage.rectTransform);
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.localScale = Vector3.one;
    }

    private bool HasScreenSizeChanged()
    {
        int width = Mathf.Max(1, Screen.width);
        int height = Mathf.Max(1, Screen.height);

        if (width == lastScreenWidth && height == lastScreenHeight)
        {
            return false;
        }

        lastScreenWidth = width;
        lastScreenHeight = height;
        return true;
    }

    private void DisableLegacySplashTextures()
    {
        GUITexture[] legacyTextures = FindObjectsOfType<GUITexture>(true);
        foreach (GUITexture legacyTexture in legacyTextures)
        {
            if (legacyTexture != null && legacyTexture.gameObject.scene == gameObject.scene)
            {
                legacyTexture.enabled = false;
            }
        }
    }

    private void RemoveSplashAudioManagers()
    {
        AudioManager[] managers = FindObjectsOfType<AudioManager>(true);
        foreach (AudioManager manager in managers)
        {
            if (manager != null && manager.gameObject.scene == gameObject.scene)
            {
                Destroy(manager.gameObject);
            }
        }
    }

    private void EnsureAudioListener()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>(true);
        foreach (AudioListener listener in listeners)
        {
            if (listener != null
                && listener.enabled
                && listener.gameObject.activeInHierarchy
                && listener.gameObject.scene == gameObject.scene)
            {
                return;
            }
        }

        AudioListener localListener = GetComponent<AudioListener>();
        if (localListener == null)
        {
            localListener = gameObject.AddComponent<AudioListener>();
        }

        localListener.enabled = true;
    }

    private IEnumerator PlayClip(VideoClip clip)
    {
        bool finished = false;
        bool failed = false;

        VideoPlayer.EventHandler onFinished = source => finished = true;
        VideoPlayer.ErrorEventHandler onError = (source, message) =>
        {
            failed = true;
            Debug.LogWarning("Splash video failed: " + clip.name + " - " + message);
        };

        ConfigureVideoPlayer();
        EnsureRenderTexture();

        videoPlayer.loopPointReached += onFinished;
        videoPlayer.errorReceived += onError;
        videoPlayer.clip = clip;
        ConfigureClipAudio(clip);
        videoPlayer.Prepare();

        float deadline = Time.realtimeSinceStartup + Mathf.Max(0.1f, prepareTimeoutSeconds);
        while (!videoPlayer.isPrepared && !failed && Time.realtimeSinceStartup < deadline)
        {
            yield return null;
        }

        if (!videoPlayer.isPrepared)
        {
            Debug.LogWarning("Splash video prepare timed out or failed: " + clip.name);
            CleanupHandlers(onFinished, onError);
            yield break;
        }

        videoPlayer.Play();
        while (!failed && !finished)
        {
            yield return null;
        }

        videoPlayer.Stop();
        CleanupHandlers(onFinished, onError);
    }

    private void ConfigureClipAudio(VideoClip clip)
    {
        if (clip == null || videoPlayer == null)
        {
            return;
        }

        if (clip.audioTrackCount == 0)
        {
            Debug.LogWarning("Splash video has no audio track: " + clip.name);
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            return;
        }

        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetDirectAudioMute(0, false);
        videoPlayer.SetDirectAudioVolume(0, 1f);
    }

    private void EnsureRenderTexture()
    {
        int width = Mathf.Max(16, Screen.width);
        int height = Mathf.Max(16, Screen.height);

        if (renderTexture != null && renderTexture.width == width && renderTexture.height == height)
        {
            AssignRenderTexture();
            return;
        }

        ReleaseRenderTexture();
        renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        renderTexture.name = "SplashVideoRuntimeTexture";
        renderTexture.Create();
        AssignRenderTexture();
    }

    private void AssignRenderTexture()
    {
        if (videoPlayer != null)
        {
            videoPlayer.targetTexture = renderTexture;
        }

        if (targetImage != null)
        {
            targetImage.texture = renderTexture;
            targetImage.color = Color.white;
        }
    }

    private void CleanupHandlers(VideoPlayer.EventHandler onFinished, VideoPlayer.ErrorEventHandler onError)
    {
        if (videoPlayer == null)
        {
            return;
        }

        videoPlayer.loopPointReached -= onFinished;
        videoPlayer.errorReceived -= onError;
    }

    private void LoadNextScene()
    {
        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogError("Splash next scene is not in Build Settings: " + nextSceneName);
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }

        if (targetImage != null)
        {
            targetImage.texture = null;
        }

        ReleaseRenderTexture();
    }

    private void ReleaseRenderTexture()
    {
        if (renderTexture == null)
        {
            return;
        }

        renderTexture.Release();
        Destroy(renderTexture);
        renderTexture = null;
    }
}
}

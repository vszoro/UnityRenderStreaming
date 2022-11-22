using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.RenderStreaming
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public static class RenderStreaming
    {
        private static RenderStreamingSettings s_Settings;

        public static RenderStreamingSettings Settings
        {
            get => s_Settings;
            set => s_Settings = value;
        }

        public static bool IsAutomaticEnabled()
        {
            return s_Settings != null && s_Settings.automaticStreaming;
        }

        public static T GetSignalingSettings<T>() where T : SignalingSettings
        {
            return s_Settings.signalingSettings as T;
        }

        static RenderStreaming()
        {
            // todo: load from assets
            var settings = ScriptableObject.CreateInstance<RenderStreamingSettings>();
            var signalingSettings = new WebSocketSignalingSettings
            {
                urlSignaling = "http://localhost:3000",
                iceServers = new[] {new ICEServer {urls = new[] {"stun:stun.l.google.com:19302"}}}
            };

            settings.automaticStreaming = true;
            settings.signalingSettings = signalingSettings;
            s_Settings = settings;

            // load settings
#if UNITY_EDITOR
            InitializeInEditor();
#else
            InitializeInPlayer();
#endif
        }

        // copy from InputSystem.cs
        ////FIXME: Unity is not calling this method if it's inside an #if block that is not
        ////       visible to the editor; that shouldn't be the case
        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RunInitializeInPlayer()
        {
            // We're using this method just to make sure the class constructor is called
            // so we don't need any code in here. When the engine calls this method, the
            // class constructor will be run if it hasn't been run already.

            // IL2CPP has a bug that causes the class constructor to not be run when
            // the RuntimeInitializeOnLoadMethod is invoked. So we need an explicit check
            // here until that is fixed (case 1014293).
#if !UNITY_EDITOR
            InitializeInPlayer();
#endif
        }

#if UNITY_EDITOR
        private static void InitializeInEditor()
        {
        }

#else
        private static void InitializeInPlayer()
        {
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunInitialize()
        {
            // common initialization

            if (IsAutomaticEnabled())
            {
                var automaticGo = new GameObject("AutomaticStreaming");
                automaticGo.AddComponent<AutomaticStreaming>();
                Object.DontDestroyOnLoad(automaticGo);
            }
        }
    }
}

using System;
using Unity.RenderStreaming.Signaling;
using Unity.WebRTC;
using UnityEngine;

namespace Unity.RenderStreaming
{
    [CreateAssetMenu]
    public class RenderStreamingSettings : ScriptableObject
    {
        [SerializeField] public bool automaticStreaming;
        [SerializeReference] public SignalingSettings signalingSettings = new WebSocketSignalingSettings();
        [SerializeReference] public CodecSettings codecSettings = new CodecSettings();
    }

    public abstract class SignalingSettings
    {
        public bool runOnAwake;
        public string urlSignaling = "http://localhost";
        public ICEServer[] iceServers;
        public abstract Type signalingClass { get; }
    }

    [Serializable]
    public class ICEServer
    {
        public string credential;
        public RTCIceCredentialType credentialType;
        public string[] urls;
        public string username;

        public static implicit operator RTCIceServer(ICEServer source)
        {
            return new RTCIceServer
            {
                credential = source.credential,
                credentialType = source.credentialType,
                urls = source.urls,
                username = source.username
            };
        }

        public static implicit operator ICEServer(RTCIceServer source)
        {
            return new ICEServer
            {
                credential = source.credential,
                credentialType = source.credentialType,
                urls = source.urls,
                username = source.username
            };
        }
    }

    [Serializable]
    public class HttpSignalingSettings : SignalingSettings
    {
        public float interval = 5.0f;
        public override Type signalingClass => typeof(HttpSignaling);
    }

    [Serializable]
    public class WebSocketSignalingSettings : SignalingSettings
    {
        public override Type signalingClass => typeof(WebSocketSignaling);
    }

    [Serializable]
    public class FurioosSignalingSettings : SignalingSettings
    {
        public override Type signalingClass => typeof(FurioosSignaling);
    }

    [Serializable]
    public class CodecSettings
    {
    }
}

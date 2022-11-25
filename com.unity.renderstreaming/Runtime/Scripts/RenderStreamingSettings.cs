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
        public string urlSignaling = "http://127.0.0.1:80";
        public ICEServer[] iceServers;
        public abstract Type signalingClass { get; }
    }

    public enum IceCredentialType
    {
        Password,
        OAuth,
    }

    [Serializable]
    public class ICEServer
    {
        public string credential;
        public IceCredentialType credentialType;
        public string[] urls;
        public string username;

        public static implicit operator RTCIceServer(ICEServer source)
        {
            return new RTCIceServer
            {
                credential = source.credential,
                credentialType = ConvertCredentialType(source.credentialType),
                urls = source.urls,
                username = source.username
            };
        }

        public static implicit operator ICEServer(RTCIceServer source)
        {
            return new ICEServer
            {
                credential = source.credential,
                credentialType = ConvertCredentialType(source.credentialType),
                urls = source.urls,
                username = source.username
            };
        }

        private static IceCredentialType ConvertCredentialType(RTCIceCredentialType source)
        {
            switch (source)
            {
                case RTCIceCredentialType.Password:
                    return IceCredentialType.Password;
                case RTCIceCredentialType.OAuth:
                    return IceCredentialType.OAuth;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }

        private static RTCIceCredentialType ConvertCredentialType(IceCredentialType source)
        {
            switch (source)
            {
                case IceCredentialType.Password:
                    return RTCIceCredentialType.Password;
                case IceCredentialType.OAuth:
                    return RTCIceCredentialType.OAuth;
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
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

using System;
using System.Linq;
using Unity.RenderStreaming.InputSystem;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

namespace Unity.RenderStreaming
{
    internal class AutomaticStreaming : MonoBehaviour
    {
        private RenderStreamingHandler renderstreaming;
        private Broadcast broadcast;
        private VideoStreamSender videoStreamSender;
        private AudioStreamSender audioStreamSender;
        private AutoInputReceiver inputReceiver;

        private void Awake()
        {
            gameObject.hideFlags = HideFlags.HideInHierarchy;

            videoStreamSender = gameObject.AddComponent<VideoStreamSender>();
            videoStreamSender.source = VideoStreamSource.Screen;
            videoStreamSender.SetTextureSize(new Vector2Int(Screen.width, Screen.height));

            audioStreamSender = gameObject.AddComponent<AudioStreamSender>();
            audioStreamSender.source = AudioStreamSource.APIOnly;

            inputReceiver = gameObject.AddComponent<AutoInputReceiver>();

            broadcast = gameObject.AddComponent<Broadcast>();
            broadcast.AddComponent(videoStreamSender);
            broadcast.AddComponent(audioStreamSender);
            broadcast.AddComponent(inputReceiver);

            renderstreaming = gameObject.AddComponent<RenderStreamingHandler>();
            renderstreaming.AddSignalingHandler(broadcast);
            renderstreaming.SetSignalingSettings(RenderStreaming.GetSignalingSettings<WebSocketSignalingSettings>());
            renderstreaming.Run();

            SceneManager.activeSceneChanged += (scene1, scene2) =>
            {
                Debug.Log($"scene changed {scene1.name} to {scene2.name}");
                var audioListener = FindObjectOfType<AudioListener>();
                if (audioListener == null || audioListener.gameObject.GetComponent<AutoAudioFilter>() != null)
                {
                    return;
                }

                var autoFilter = audioListener.gameObject.AddComponent<AutoAudioFilter>();
                autoFilter.SetSender(audioStreamSender);
            };
        }

        private void OnDestroy()
        {
            renderstreaming.Stop();
            renderstreaming = null;
            broadcast = null;
            videoStreamSender = null;
            audioStreamSender = null;
        }

        class AutoAudioFilter : MonoBehaviour
        {
            private AudioStreamSender sender;
            private int m_sampleRate;

            public void SetSender(AudioStreamSender sender)
            {
                this.sender = sender;
            }

            private void Awake()
            {
                this.hideFlags = HideFlags.HideInInspector;
            }

            private void OnEnable()
            {
                OnAudioConfigurationChanged(false);
                AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
            }

            private void OnDisable()
            {
                AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
            }

            private void OnAudioConfigurationChanged(bool deviceWasChanged)
            {
                m_sampleRate = AudioSettings.outputSampleRate;
            }

            private void OnAudioFilterRead(float[] data, int channels)
            {
                if (sender.source != AudioStreamSource.APIOnly)
                {
                    return;
                }

                sender?.SetData(data, channels, m_sampleRate);
            }

            private void OnDestroy()
            {
                sender = null;
            }
        }

        class AutoInputReceiver : InputChannelReceiverBase
        {
            public override event Action<InputDevice, InputDeviceChange> onDeviceChange;

            protected virtual void OnEnable()
            {
                m_Enabled = true;
                onDeviceChange += OnDeviceChange;
            }

            protected virtual void OnDisable()
            {
                m_Enabled = false;
                onDeviceChange -= OnDeviceChange;
            }

            private void PerformPairingWithDevice(InputDevice device)
            {
                m_InputUser = InputUser.PerformPairingWithDevice(device, m_InputUser);
            }

            private void UnpairDevices(InputDevice device)
            {
                if (!m_InputUser.valid)
                    return;
                m_InputUser.UnpairDevice(device);
            }

            public override void SetChannel(string connectionId, RTCDataChannel channel)
            {
                if (channel == null)
                {
                    Dispose();
                }
                else
                {
                    AssignUserAndDevices();
                    receiver = new Receiver(channel);
                    receiver.onDeviceChange += onDeviceChange;
                    receiverInput = new InputSystem.InputRemoting(receiver);
                    subscriberDisposer = receiverInput.Subscribe(receiverInput);
                    receiverInput.StartSending();
                }

                base.SetChannel(connectionId, channel);
            }

            protected virtual void OnDestroy()
            {
                Dispose();
            }

            protected virtual void Dispose()
            {
                UnassignUserAndDevices();
                receiverInput?.StopSending();
                subscriberDisposer?.Dispose();
                receiver?.Dispose();
                receiver = null;
            }

            [NonSerialized] private bool m_Enabled;
            [NonSerialized] private InputUser m_InputUser;

            [NonSerialized] private Receiver receiver;
            [NonSerialized] private InputSystem.InputRemoting receiverInput;
            [NonSerialized] private IDisposable subscriberDisposer;

            private void AssignUserAndDevices()
            {
                m_InputUser = InputUser.all.FirstOrDefault();
            }

            private void UnassignUserAndDevices()
            {
                if (!m_InputUser.valid)
                {
                    return;
                }

                m_InputUser.UnpairDevicesAndRemoveUser();
            }

            protected virtual void OnDeviceChange(InputDevice device, InputDeviceChange change)
            {
                switch (change)
                {
                    case InputDeviceChange.Added:
                        PerformPairingWithDevice(device);
                        return;
                    case InputDeviceChange.Removed:
                        UnpairDevices(device);
                        return;
                }
            }
        }
    }
}

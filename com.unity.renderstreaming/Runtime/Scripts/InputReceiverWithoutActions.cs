using System;
using System.Linq;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Unity.RenderStreaming.InputSystem;

namespace Unity.RenderStreaming
{
    using InputRemoting = InputSystem.InputRemoting;

    public class InputReceiverWithoutActions : InputChannelReceiverBase
    {
        /// <summary>
        ///
        /// </summary>
        public override event Action<InputDevice, InputDeviceChange> onDeviceChange;

        /// <summary>
        ///
        /// </summary>
        protected virtual void OnEnable()
        {
            m_Enabled = true;
            onDeviceChange += OnDeviceChange;
        }

        /// <summary>
        ///
        /// </summary>
        protected virtual void OnDisable()
        {
            m_Enabled = false;
            onDeviceChange -= OnDeviceChange;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        public void PerformPairingWithDevice(InputDevice device)
        {
            m_InputUser = InputUser.PerformPairingWithDevice(device, m_InputUser);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        public void UnpairDevices(InputDevice device)
        {
            if (!m_InputUser.valid)
                return;
            m_InputUser.UnpairDevice(device);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="track"></param>
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
                receiverInput = new InputRemoting(receiver);
                subscriberDisposer = receiverInput.Subscribe(receiverInput);
                receiverInput.StartSending();
            }

            base.SetChannel(connectionId, channel);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="size">Texture Size.</param>
        /// <param name="region">Region of the texture in world coordinate system.</param>
        public void CalculateInputRegion(Vector2Int size, Rect region)
        {
            receiver.CalculateInputRegion(new Rect(Vector2.zero, size), region);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="enabled"></param>
        public void SetEnableInputPositionCorrection(bool enabled)
        {
            receiver.EnableInputPositionCorrection = enabled;
        }


        /// <summary>
        ///
        /// </summary>
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
        [NonSerialized] private InputRemoting receiverInput;
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

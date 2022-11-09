﻿using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Metadata for encryption device
    ///
    /// The encryption device is a stateful container that represents abstract hardware
    /// authorized to participate in a specific election.
    ///
    /// </summary>
    public class EncryptionDevice : DisposableBase
    {
        internal unsafe NativeInterface.EncryptionDevice.EncryptionDeviceHandle Handle;

        /// <summary>
        /// Create a new EncryptionDevice
        /// </summary>
        /// <param name="deviceUuid">a unique identifier tied to the device hardware</param>
        /// <param name="sessionUuid">a unique identifier tied to the runtime session</param>
        /// <param name="launchCode">a unique identifer tied to the election</param>
        /// <param name="location">an arbitrary string meaningful to the external system
        ///                        such as a friendly name, description, or some other value</param>
        public unsafe EncryptionDevice(
            ulong deviceUuid,
            ulong sessionUuid,
            ulong launchCode,
            string location)
        {
            var status = NativeInterface.EncryptionDevice.New(
                deviceUuid, sessionUuid, launchCode, location, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Get a new hash value
        ///
        /// <return>An `ElementModQ`</return>
        /// </summary>
        public unsafe ElementModQ GetHash()
        {
            var status = NativeInterface.EncryptionDevice.GetHash(Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            return new ElementModQ(value);
        }

        /// <summary>
        /// produces encription device when given json
        /// </summary>
        /// <param name="json"></param>
        public unsafe EncryptionDevice(string json)
        {
            var status = NativeInterface.EncryptionDevice.FromJson(json, out Handle);
            status.ThrowIfError();
        }
        /// <Summary>
        /// Export the encryptiondevice representation as JSON
        /// </Summary>
        public unsafe string ToJson()
        {
            var status = NativeInterface.EncryptionDevice.ToJson(Handle, out IntPtr pointer, out ulong size);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }



#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }
    }
}
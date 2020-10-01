﻿using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    using NativeInternalElectionDescription = NativeInterface.InternalElectionDescription.InternalElectionDescriptionType;
    using NativeCiphertextElectionContext = NativeInterface.CiphertextElectionContext.CiphertextElectionType;

    public class InternalElectionDescription
    {
        public unsafe ElementModQ DescriptionHash
        {
            get
            {
                var val = NativeInterface.InternalElectionDescription.GetDescriptionHash(Handle);
                return new ElementModQ(val);
            }
        }

        internal unsafe NativeInternalElectionDescription* Handle;

        public unsafe InternalElectionDescription(string json)
        {
            Handle = NativeInterface.InternalElectionDescription.FromJson(json);
        }

        public unsafe string ToJson()
        {
            _ = NativeInterface.InternalElectionDescription.ToJson(Handle, out IntPtr pointer);
            var json = Marshal.PtrToStringAnsi(pointer);
            return json;
        }
    }

    public class CiphertextElectionContext
    {
        public unsafe ElementModP ElGamalPublicKey
        {
            get
            {
                var val = NativeInterface.CiphertextElectionContext.GetElGamalPublicKey(Handle);
                return new ElementModP(val);
            }
        }

        public unsafe ElementModQ DescriptionHash
        {
            get
            {
                var val = NativeInterface.CiphertextElectionContext.GetDescriptionHash(Handle);
                return new ElementModQ(val);
            }
        }

        public unsafe ElementModQ CryptoBaseHash
        {
            get
            {
                var val = NativeInterface.CiphertextElectionContext.GetCryptoBaseHash(Handle);
                return new ElementModQ(val);
            }
        }

        public unsafe ElementModQ CryptoExtendedBaseHash
        {
            get
            {
                var val = NativeInterface.CiphertextElectionContext.GetCryptoExtendedBaseHash(Handle);
                return new ElementModQ(val);
            }
        }

        internal unsafe NativeCiphertextElectionContext* Handle;

        public unsafe CiphertextElectionContext(string json)
        {
            Handle = NativeInterface.CiphertextElectionContext.FromJson(json);           
        }

        public unsafe string ToJson()
        {
            _ = NativeInterface.CiphertextElectionContext.ToJson(Handle, out IntPtr pointer);
            var json = Marshal.PtrToStringAnsi(pointer);
            return json;
        }
    }
}
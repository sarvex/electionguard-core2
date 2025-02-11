﻿using System;
using Microsoft.Win32.SafeHandles;

namespace ElectionGuard
{
    internal abstract class ElectionGuardSafeHandle<T>
        : SafeHandleZeroOrMinusOneIsInvalid
        where T : unmanaged
    {
        // Objects constructed with the default constructor
        // own the context
        internal ElectionGuardSafeHandle()
            : base(ownsHandle: true)
        {

        }

        // Objects constructed from a structure pointer
        // do not own the context
        internal ElectionGuardSafeHandle(
            IntPtr handle)
            : base(ownsHandle: false)
        {
            SetHandle(handle);
        }

        internal ElectionGuardSafeHandle(
            IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        internal unsafe ElectionGuardSafeHandle(
            T* handle)
            : base(ownsHandle: false)
        {
            SetHandle((IntPtr)handle);
        }

        internal unsafe ElectionGuardSafeHandle(
            T* handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle((IntPtr)handle);
        }

        public unsafe T* TypedPtr => (T*)handle;

        public IntPtr Ptr => handle;

        public void ThrowIfInvalid()
        {
            if (IsInvalid)
            {
                throw new ElectionGuardException($"{nameof(T)} ERROR Invalid Handle");
            }
        }

        protected bool IsFreed = false;

        protected abstract bool Free();

        protected override bool ReleaseHandle()
        {
            try
            {
                var freed = Free();
                if (freed)
                {
                    IsFreed = true;
                    Close();
                }
                return freed;
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException($"{nameof(T)} ERROR ReleaseHandle: {ex.Message}", ex);
            }
        }
    }
}

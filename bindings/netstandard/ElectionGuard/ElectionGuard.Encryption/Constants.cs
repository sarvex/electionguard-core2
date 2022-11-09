﻿using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Static class of constant values used in the encryption system
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// G in Hacl_Bignum4096 format
        /// </summary>
        public unsafe static ElementModP G
        {
            get
            {
                var status = NativeInterface.Constants.G(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"G Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// Max P value in Hacl_Bignum4096 format
        /// </summary>
        public unsafe static ElementModP P
        {
            get
            {
                var status = NativeInterface.Constants.P(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"P Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// R value in Hacl_Bignum4096 format
        /// </summary>
        public unsafe static ElementModP R
        {
            get
            {
                var status = NativeInterface.Constants.R(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"R Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// zero as data for `ElementModP`
        /// </summary>
        public unsafe static ElementModP ZERO_MOD_P
        {
            get
            {
                var status = NativeInterface.Constants.ZERO_MOD_P(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ZERO_MOD_P Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// one as data for `ElementModP`
        /// </summary>
        public unsafe static ElementModP ONE_MOD_P
        {
            get
            {
                var status = NativeInterface.Constants.ZERO_MOD_P(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ONE_MOD_P Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// two as data for `ElementModP`
        /// </summary>
        public unsafe static ElementModP TWO_MOD_P
        {
            get
            {
                var status = NativeInterface.Constants.ZERO_MOD_P(out NativeInterface.ElementModP.ElementModPHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"TWO_MOD_P Error Status: {status}");
                }
                return new ElementModP(handle);
            }
        }

        /// <summary>
        /// Max Q value in Hacl_Bignum256 format
        /// </summary>
        public unsafe static ElementModQ Q
        {
            get
            {
                var status = NativeInterface.Constants.Q(out NativeInterface.ElementModQ.ElementModQHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"Q Error Status: {status}");
                }
                return new ElementModQ(handle);
            }
        }

        /// <summary>
        /// zero as data for `ElementModQ`
        /// </summary>
        public unsafe static ElementModQ ZERO_MOD_Q
        {
            get
            {
                var status = NativeInterface.Constants.ZERO_MOD_Q(out NativeInterface.ElementModQ.ElementModQHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ZERO_MOD_Q Error Status: {status}");
                }
                return new ElementModQ(handle);
            }
        }

        /// <summary>
        /// one as data for `ElementModQ`
        /// </summary>
        public unsafe static ElementModQ ONE_MOD_Q
        {
            get
            {
                var status = NativeInterface.Constants.ONE_MOD_Q(out NativeInterface.ElementModQ.ElementModQHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ONE_MOD_Q Error Status: {status}");
                }
                return new ElementModQ(handle);
            }
        }

        /// <summary>
        /// two as data for `ElementModQ`
        /// </summary>
        public unsafe static ElementModQ TWO_MOD_Q
        {
            get
            {
                var status = NativeInterface.Constants.TWO_MOD_Q(out NativeInterface.ElementModQ.ElementModQHandle handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"TWO_MOD_Q Error Status: {status}");
                }
                return new ElementModQ(handle);
            }
        }

        /// <Summary>
        /// Export the representation as JSON
        /// </Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public unsafe static string ToJson()
        {
            var status = NativeInterface.Constants.ToJson(
                out IntPtr pointer, out ulong size);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

    }
}
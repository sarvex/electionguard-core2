using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A CiphertextBallotContest represents the selections made by a voter for a specific ContestDescription
    ///
    /// CiphertextBallotContest can only be a complete representation of a contest dataset.  While
    /// PlaintextBallotContest supports a partial representation, a CiphertextBallotContest includes all data
    /// necessary for a verifier to verify the contest.  Specifically, it includes both explicit affirmative
    /// and negative selections of the contest, as well as the placeholder selections that satisfy
    /// the ConstantChaumPedersen proof.
    ///
    /// Similar to `CiphertextBallotSelection` the consuming application can choose to discard or keep both
    /// the `nonce` and the `proof` in some circumstances.  For deterministic nonce's derived from the
    /// seed nonce, both values can be regenerated.  If the `nonce` for this contest is completely random,
    /// then it is required in order to regenerate the proof.
    /// </summary>
    public class CiphertextBallotContest : DisposableBase
    {
        /// <Summary>
        /// Get the objectId of the contest, which is the unique id for
        /// the contest in a specific ballot style described in the election manifest.
        /// </Summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.CiphertextBallotContest.GetObjectId(
                    Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Get the sequence order of the contest
        /// </Summary>
        public unsafe ulong SequenceOrder => NativeInterface.CiphertextBallotContest.GetSequenceOrder(Handle);

        /// <summary>
        /// The hash of the string representation of the Contest Description from the election manifest
        /// </summary>
        public unsafe ElementModQ DescriptionHash
        {
            get
            {
                var status = NativeInterface.CiphertextBallotContest.GetDescriptionHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// Get the Size of the selections collection
        /// </summary>
        public unsafe ulong SelectionsSize
        {
            get
            {
                var size = NativeInterface.CiphertextBallotContest.GetSelectionsSize(Handle);
                return size;
            }
        }

        /// <summary>
        /// Hash of the encrypted values
        /// </summary>
        public unsafe ElementModQ CryptoHash
        {
            get
            {
                var status = NativeInterface.CiphertextBallotContest.GetCryptoHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// The nonce used to generate the encryption. Sensitive &amp; should be treated as a secret
        /// </summary>
        public unsafe ElementModQ Nonce
        {
            get
            {
                var status = NativeInterface.CiphertextBallotContest.GetNonce(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// The proof demonstrates the sum of the selections does not exceed the maximum
        /// available selections for the contest, and that the proof was generated with the nonce
        /// </summary>
        public unsafe ConstantChaumPedersenProof Proof
        {
            get
            {
                var status = NativeInterface.CiphertextBallotContest.GetProof(
                    Handle, out NativeInterface.ConstantChaumPedersenProof.ConstantChaumPedersenProofHandle value);
                status.ThrowIfError();
                return new ConstantChaumPedersenProof(value);
            }
        }

        internal unsafe NativeInterface.CiphertextBallotContest.CiphertextBallotContestHandle Handle;

        internal unsafe CiphertextBallotContest(NativeInterface.CiphertextBallotContest.CiphertextBallotContestHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Get a selection at a specific index.
        /// </summary>
        public unsafe CiphertextBallotSelection GetSelectionAt(ulong index)
        {
            var status = NativeInterface.CiphertextBallotContest.GetSelectionAtIndex(
                Handle, index, out CiphertextBallotSelection.External.CiphertextBallotSelectionHandle value);
            status.ThrowIfError();
            return new CiphertextBallotSelection(value);
        }

        /// <summary>
        /// Given an encrypted BallotContest, generates a hash, suitable for rolling up
        /// into a hash / tracking code for an entire ballot. Of note, this particular hash examines
        /// the `encryptionSeed` and `message`, but not the proof.
        /// This is deliberate, allowing for the possibility of ElectionGuard variants running on
        /// much more limited hardware, wherein the Constant Chaum-Pedersen proofs might be computed
        /// later on.
        ///
        /// In most cases the encryption_seed should match the `description_hash`
        /// </summary>
        public unsafe ElementModQ CryptoHashWith(ElementModQ encryptionSeed)
        {
            var status = NativeInterface.CiphertextBallotContest.CryptoHashWith(
                Handle, encryptionSeed.Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// An aggregate nonce for the contest composed of the nonces of the selections.
        /// Used when constructing the proof of selection limit
        /// </summary>
        public unsafe ElementModQ AggregateNonce()
        {
            var status = NativeInterface.CiphertextBallotContest.AggregateNonce(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Add the individual ballot_selections `message` fields together, suitable for use
        /// when constructing the proof of selection limit.
        /// </summary>
        public unsafe ElGamalCiphertext ElGamalAccumulate()
        {
            var status = NativeInterface.CiphertextBallotContest.ElGamalAccumulate(
                Handle, out NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle value);
            status.ThrowIfError();
            return new ElGamalCiphertext(value);
        }

        /// <summary>
        /// Given an encrypted BallotContest, validates the encryption state against
        /// a specific encryption seed and public key
        /// by verifying the accumulated sum of selections match the proof.
        /// Calling this function expects that the object is in a well-formed encrypted state
        /// with the `ballot_selections` populated with valid encrypted ballot selections,
        /// the ElementModQ `description_hash`, the ElementModQ `crypto_hash`,
        /// and the ConstantChaumPedersenProof all populated.
        /// Specifically, the seed hash in this context is the hash of the ContestDescription,
        /// or whatever `ElementModQ` was used to populate the `description_hash` field.
        /// </summary>
        public unsafe bool IsValidEncryption(
            ElementModQ encryptionSeed,
            ElementModP elGamalPublicKey,
            ElementModQ cryptoExtendedBaseHash)
        {
            return NativeInterface.CiphertextBallotContest.IsValidEncryption(
                Handle, encryptionSeed.Handle, elGamalPublicKey.Handle,
                cryptoExtendedBaseHash.Handle);
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
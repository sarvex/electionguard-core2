using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// A Guardian's Partial Decryption of a selection.
/// </summary>
public class CiphertextDecryptionSelectionShare
    : DisposableBase, IElectionSelection, IEquatable<CiphertextDecryptionSelectionShare>
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the selection.
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the SelectionDescription.
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    /// <summary>
    /// The Guardian's Id.
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// The Guardian's share of the partial decryption. `M_i` in the spec
    /// </summary>
    public ElementModP Share { get; init; }

    /// <summary>
    /// The proof that the share was decrypted correctly.
    /// </summary>
    // TODO: only store commitment and response, not the full proof
    public ChaumPedersenProof Proof { get; init; }

    public CiphertextDecryptionSelectionShare(
        string objectId,
        ulong sequenceOrder,
        ElementModQ descriptionHash,
        string guardianId,
        ElementModP share,
        ChaumPedersenProof proof)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        DescriptionHash = new(descriptionHash);
        GuardianId = guardianId;
        Share = new(share);
        Proof = proof;
    }

    public CiphertextDecryptionSelectionShare(
        ICiphertextSelection selection,
        string guardianId,
        ElementModP share,
        ChaumPedersenProof proof)
    {
        ObjectId = selection.ObjectId;
        SequenceOrder = selection.SequenceOrder;
        DescriptionHash = new(selection.DescriptionHash);
        GuardianId = guardianId;
        Share = new(share);
        Proof = proof;
    }

    public CiphertextDecryptionSelectionShare(
        CiphertextDecryptionSelectionShare share)
    {
        ObjectId = share.ObjectId;
        SequenceOrder = share.SequenceOrder;
        DescriptionHash = new(share.DescriptionHash);
        GuardianId = share.GuardianId;
        Share = new(share.Share);
        Proof = share.Proof;
    }

    /// <summary>
    /// Verify that this CiphertextDecryptionSelection is valid for a
    /// specific guardian, and extended base hash. We allow any ICiphertextSelection
    /// to be passed in, so that we can use this method to verify either
    /// a ballot or a tally.
    /// </summary>
    public bool IsValid(
        ICiphertextSelection message,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (guardian.OwnerId != GuardianId)
        {
            return false;
        }

        if (message.ObjectId != ObjectId)
        {
            return false;
        }

        if (message.DescriptionHash is null
            || !message.DescriptionHash.Equals(DescriptionHash))
        {
            return false;
        }

        return IsValidEncryption(
            message.Ciphertext,
            guardian.Key!,
            extendedBaseHash);
    }

    /// <summary>
    /// Verify that this CiphertextDecryptionSelection is valid for a 
    /// specific ciphertext, guardian public key, and extended base hash.
    /// </summary>
    public bool IsValidEncryption(
        ElGamalCiphertext message,
        ElementModP guardianPublicKey,
        ElementModQ extendedBaseHash)
    {
        var proofIsValid = Proof.IsValid(
            message,
            guardianPublicKey,
            Share,
            extendedBaseHash);

        return proofIsValid;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        DescriptionHash.Dispose();
        Share.Dispose();
    }

    #region Equality

    public bool Equals(CiphertextDecryptionSelectionShare? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ObjectId == other.ObjectId &&
               SequenceOrder == other.SequenceOrder &&
               DescriptionHash.Equals(other.DescriptionHash) &&
               Share.Equals(other.Share); // TODO: proof comparison
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is CiphertextDecryptionSelectionShare other && Equals(other));
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ObjectId, SequenceOrder, DescriptionHash, Share);
    }

    public static bool operator ==(CiphertextDecryptionSelectionShare? left, CiphertextDecryptionSelectionShare? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(CiphertextDecryptionSelectionShare? left, CiphertextDecryptionSelectionShare? right)
    {
        return !Equals(left, right);
    }

    #endregion

}

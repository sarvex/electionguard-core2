using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Ballot;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// A Guardian's Partial Decryption of a contest. 
/// This object is used both for Tally's and Ballot partial decryptions.
/// </summary>
public record CiphertextDecryptionContestShare
    : DisposableRecordBase, IElectionContest, IEquatable<CiphertextDecryptionContestShare>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the contest
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The hash of the contest description
    /// </summary>
    public ElementModQ DescriptionHash { get; init; }

    // partial decryption of extended data for the contest
    public ElementModP? ExtendedData { get; init; } = default!;

    // TODO: commitment for generating the cp proof as part of decryption
    public ElGamalCiphertext? Commitment { get; init; } = default!;

    /// <summary>
    /// Collection of Selection Shares
    /// the key is the selection object id
    /// </summary>
    public Dictionary<string, CiphertextDecryptionSelectionShare> Selections { get; init; } = default!;

    public CiphertextDecryptionContestShare(
        IElectionContest contest,
        Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        Selections = selections.Select(
            x => new CiphertextDecryptionSelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public CiphertextDecryptionContestShare(
        IElectionContest contest,
        ElementModP extendedData,
        ElGamalCiphertext? commitment, // TODO: non-nullable?
        Dictionary<string, CiphertextDecryptionSelectionShare> selections)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
        DescriptionHash = new(contest.DescriptionHash);
        ExtendedData = new(extendedData);
        Commitment = commitment != null ? new(commitment) : null;
        Selections = selections.Select(
            x => new CiphertextDecryptionSelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public CiphertextDecryptionContestShare(CiphertextDecryptionContestShare other) : base(other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        DescriptionHash = new(other.DescriptionHash);
        ExtendedData = other.ExtendedData != null ? new(other.ExtendedData) : null;
        Commitment = other.Commitment != null ? new(other.Commitment) : null;
        Selections = other.Selections.Select(
            x => new CiphertextDecryptionSelectionShare(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    /// <summary>
    /// Verify the validity of the contest share against a ballot.
    /// </summary>
    public bool IsValid(
        CiphertextBallotContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        // TODO: verify the extended data

        // validate each selection
        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.ObjectId))
            {
                return false;
            }

            if (!Selections[selection.ObjectId].IsValid(
                selection, guardian, extendedBaseHash))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Verify the validity of the contest share against a tally.
    /// </summary>
    public bool IsValid(
        CiphertextTallyContest contest,
        ElectionPublicKey guardian,
        ElementModQ extendedBaseHash)
    {
        if (contest.ObjectId != ObjectId)
        {
            return false;
        }

        // validate each selection
        foreach (var selection in contest.Selections)
        {
            if (!Selections.ContainsKey(selection.Key))
            {
                return false;
            }

            if (!Selections[selection.Key].IsValid(
                selection.Value, guardian, extendedBaseHash))
            {
                return false;
            }
        }
        return true;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        ExtendedData?.Dispose();
        Commitment?.Dispose();
        DescriptionHash.Dispose();
        if (Selections != null)
        {
            foreach (var selection in Selections.Values)
            {
                selection.Dispose();
            }
            Selections.Clear();
        }
    }
}

using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Encryption.Ballot;

namespace ElectionGuard.Decryption.Decryption;

/// <summary>
/// Decryption extension methods for the <see cref="Guardian" /> class
/// </summary>
public static class GuardianDecryptionExtensions
{
    /// <summary>
    /// Conmpute decryption shares for a tally and a list of ballots. 
    /// Usially the list of ballots is the spoiled ballots in the tally.
    /// </summary>
    public static Tuple<CiphertextDecryptionTallyShare, Dictionary<string, CiphertextDecryptionBallotShare>> ComputeDecryptionShares(
        this Guardian guardian,
        CiphertextTally tally,
        List<CiphertextBallot> ballots)
    {
        var share = guardian.ComputeDecryptionShare(tally)!;
        var shares = guardian.ComputeDecryptionShares(tally.TallyId, ballots)!;
        return new Tuple<CiphertextDecryptionTallyShare, Dictionary<string, CiphertextDecryptionBallotShare>>(share, shares);
    }

    /// <summary>
    /// Compute decryption shares for a list of ballots.
    /// </summary>
    public static Dictionary<string, CiphertextDecryptionBallotShare> ComputeDecryptionShares(
        this Guardian guardian,
        string tallyId,
        List<CiphertextBallot> ballots)
    {
        var shares = new Dictionary<string, CiphertextDecryptionBallotShare>();
        foreach (var ballot in ballots)
        {
            shares.Add(
                ballot.ObjectId, guardian.ComputeDecryptionShare(tallyId, ballot)!);
        }

        return shares;
    }

    /// <summary>
    /// Compute a decryption share for a tally
    /// </summary>
    public static CiphertextDecryptionTallyShare? ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTally tally)
    {
        var contests = new Dictionary<string, CiphertextDecryptionContestShare>();
        foreach (var contest in tally.Contests.Values)
        {
            contests.Add(contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new CiphertextDecryptionTallyShare(
            guardian.GuardianId, tally.TallyId, contests
        );
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a ballot
    /// </summary>
    public static CiphertextDecryptionBallotShare? ComputeDecryptionShare(
        this Guardian guardian,
        string tallyId,
        CiphertextBallot ballot)
    {
        var contests = new Dictionary<string, CiphertextDecryptionContestShare>();
        foreach (var contest in ballot.Contests)
        {
            contests.Add(
                contest.ObjectId, guardian.ComputeDecryptionShare(contest)!);
        }

        var share = new CiphertextDecryptionBallotShare(
            guardian.GuardianId, tallyId, ballot, contests
        );
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a contest on a tally
    /// </summary>
    public static CiphertextDecryptionContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextTallyContest contest)
    {
        var selections = new Dictionary<string, CiphertextDecryptionSelectionShare>();
        foreach (var selection in contest.Selections.Values)
        {
            selections.Add(
                selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new CiphertextDecryptionContestShare(
            contest, selections);
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a contest on a ballot
    /// </summary>
    public static CiphertextDecryptionContestShare ComputeDecryptionShare(
        this Guardian guardian,
        CiphertextBallotContest contest)
    {
        var extendedData = guardian.PartialDecrypt(contest.ExtendedData);
        ElGamalCiphertext? commitment = null;

        var selections = new Dictionary<string, CiphertextDecryptionSelectionShare>();
        foreach (var selection in contest.Selections)
        {
            selections.Add(
                selection.ObjectId, guardian.ComputeDecryptionShare(selection)!);
        }

        var share = new CiphertextDecryptionContestShare(
            contest, extendedData, commitment, selections);
        return share;
    }

    /// <summary>
    /// Compute a decryption share for a selection on a tally or a ballot.
    /// </summary>
    public static CiphertextDecryptionSelectionShare ComputeDecryptionShare(
        this Guardian guardian,
        ICiphertextSelection selection)
    {
        var partial = guardian.PartialDecrypt(selection.Ciphertext);

        // TODO: real proof
        var proof = new ChaumPedersenProof();

        var share = new CiphertextDecryptionSelectionShare(
            selection,
            guardian.GuardianId,
            partial,
            proof);
        return share;
    }

    // TODO: compute decryption share proof
}

﻿using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace ElectionGuard.UI.ViewModels;

[QueryProperty(CurrentTallyIdParam, nameof(TallyId))]
public partial class TallyProcessViewModel : BaseViewModel
{
    public const string CurrentTallyIdParam = "TallyId";

    [ObservableProperty]
    private string _tallyId = string.Empty;

    [ObservableProperty]
    private TallyRecord _tally = new();

    [ObservableProperty]
    private Election _currentElection = new();

    [ObservableProperty]
    private List<BallotUpload> _ballotUploads = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(JoinTallyCommand))]
    private List<TallyJoinedRecord> _joinedGuardians = new();

    private ElectionService _electionService;
    private TallyService _tallyService;
    private BallotUploadService _ballotUploadService;
    private TallyJoinedService _tallyJoinedService;

    public TallyProcessViewModel(
        IServiceProvider serviceProvider,
        TallyService tallyService,
        TallyJoinedService tallyJoinedService,
        ElectionService electionService,
        BallotUploadService ballotUploadService) :
        base("TallyProcess", serviceProvider)
    {
        _tallyService = tallyService;
        _electionService = electionService;
        _tallyJoinedService = tallyJoinedService;
        _ballotUploadService = ballotUploadService;
    }

    partial void OnTallyIdChanged(string value)
    {
        _ = Shell.Current.CurrentPage.Dispatcher.DispatchAsync(async () =>
        {
            var firstValue = value;
            var Tally = await _tallyService.GetByTallyIdAsync(firstValue);

            if (Tally != null)
            {
                JoinedGuardians = await _tallyJoinedService.GetByTallyIdAsync(firstValue);
                var electionId = Tally.ElectionId ?? string.Empty;
                var election = await _electionService.GetByElectionIdAsync(electionId);
                if (election is null)
                {
                    throw new ElectionGuardException($"ElectionId {electionId} not found! Tally {Tally.Id}");
                    // TODO: put up some error message somewhere, over the rainbow.
                }

                CurrentElection = election;

                BallotUploads = await _ballotUploadService.GetByElectionIdAsync(electionId);

                JoinedGuardians = await _tallyJoinedService.GetByTallyIdAsync(Tally.TallyId);
            }
        });
    }

    [RelayCommand(CanExecute = nameof(CanJoinTally))]
    private void JoinTally()
    {
        var joiner = new TallyJoinedRecord()
        {
            TallyId = TallyId,
            GuardianId = UserName!, // can assume not null, since you need to be signed into 
        };

        Task.Run(async () => await _tallyJoinedService.JoinTally(joiner));
        JoinedGuardians.Add(joiner);
    }

    private bool CanJoinTally() => CurrentUserJoinedAlready() && Tally.State == TallyState.PendingGuardiansJoin;

    private bool CurrentUserJoinedAlready() => JoinedGuardians.SingleOrDefault(g => g.GuardianId == UserName) is object;

    [RelayCommand(CanExecute =nameof(CanJoinTally))]
    private void RejectTally() 
    {
        // TODO: what happens when we don't want to join a tally, nothing? Can we change our mind later?
    }

}

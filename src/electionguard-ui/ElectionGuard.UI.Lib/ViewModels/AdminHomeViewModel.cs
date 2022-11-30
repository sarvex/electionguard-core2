﻿using CommunityToolkit.Mvvm.Input;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.ViewModels
{
    public partial class AdminHomeViewModel : BaseViewModel
    {
        public AdminHomeViewModel(IServiceProvider serviceProvider) : base("AdminHome", serviceProvider)
        {
            _elections.Add(new Election { Name = "Pilot Election" });
        }

        [ObservableProperty]
        private ObservableCollection<Election> _elections = new();

        [ObservableProperty]
        private Election? _currentElection;

        [RelayCommand]
        public async Task GoKeyCeremony()
        {
            await NavigationService.GoToPage(typeof(AdminHomeViewModel));
        }

        [RelayCommand]
        private void CreateElection()
        {
        }

        [RelayCommand]
        private async Task SelectionChanged()
        {
            if (CurrentElection is not null)
            {
                var pageParams = new Dictionary<string, object>
                {
                    { ElectionViewModel.CurrentElectionParam, CurrentElection }
                };
                await NavigationService.GoToPage(typeof(ElectionViewModel), pageParams);
                CurrentElection = null;
            }
        }
    }
}
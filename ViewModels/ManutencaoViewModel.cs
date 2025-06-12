﻿using Celer.Models;
using Celer.Services;
using Celer.Views.UserControls.MainApp.ManutencaoViews;

namespace Celer.ViewModels
{
    public partial class ManutencaoViewModel : BaseNavigationViewModel
    {
        private readonly Dictionary<string, NavigationSubView> _views;

        protected override Dictionary<string, NavigationSubView> SubViews => _views;

        public ManutencaoViewModel(NavigationService navigationService, Repair repairView)
            : base(navigationService, "Manutencao")
        {
            _views = new Dictionary<string, NavigationSubView>
            {
                { "Repair", new NavigationSubView("Recuperação", repairView) },
            };
        }
    }
}

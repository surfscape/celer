using Celer.Resources;
using Celer.Utilities;
using Celer.Views.Windows.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Interaction logic for Onboarding.xaml
	/// </summary>
	public partial class Onboarding : BaseWindow
	{
		public Onboarding()
		{
			InitializeComponent();
			OnboardingOptions.DataContext = new OnboardingViewModel { OnCompleted = Close };
			TextUtilities.SetLinkedText(AcceptLegalCheckbox, Resource.Onboarding_LegalCheckboxLabel, (Resource.TermsOfUse, "https://surfscape.eu/celer/legal/terms/"), (Resource.PrivacyPolicy, "https://surfscape.eu/celer/legal/privacy/"));
		}

		public partial class OnboardingViewModel : ObservableObject
		{
			public Action? OnCompleted { get; set; }

			[ObservableProperty]
			private bool acceptTerms = false;

			[ObservableProperty]
			private bool autoUpdates = false;

			[ObservableProperty]
			private bool autoStartup = false;

			[RelayCommand]
			private void Start()
			{
				Properties.MainConfiguration.Default.HasUserDoneSetup = AcceptTerms;
				Properties.MainConfiguration.Default.EnableAutoSurfScapeGateway = AutoUpdates;
				Properties.MainConfiguration.Default.AutoStartup = AutoStartup;
				Properties.MainConfiguration.Default.CloseShouldMinimize = AutoStartup;
				Properties.MainConfiguration.Default.Save();

				if (Properties.MainConfiguration.Default.AutoStartup)
					UserLand.SetAutoStartup();
				var gateway = App.AppHost?.Services.GetService<SurfScapeGateway>();
				if (gateway is not null)
				{
					gateway.MainWindowTrigger = true;
					OnCompleted?.Invoke();
					gateway.ShowDialog();

				}
			}
		}
	}
}

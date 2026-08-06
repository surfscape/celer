using Celer.Utilities;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace Celer.Views.Windows
{
	/// <summary>
	/// Displays a message box that contains a message and actions. Modern substitute to MessageBox
	/// </summary>
	public partial class Dialog : BaseWindow
	{
		public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;
		public Dialog(string message, string title, string[]? buttonLabels, MessageBoxButtons buttons)
		{
			InitializeComponent();
			Title = title;
			TitleText.Text = title;
			MessageText.Text = message;
			CancelButton.Visibility = (buttons is 0 or (MessageBoxButtons)4) ? Visibility.Collapsed : Visibility.Visible;
			NoButton.Visibility = (buttons is 0 or (MessageBoxButtons)1) ? Visibility.Collapsed : Visibility.Visible;
			if (NoButton.Visibility == Visibility.Collapsed || CancelButton.Visibility == Visibility.Collapsed)
			{
				ButtonGrid.ColumnDefinitions.RemoveAt(2);
				if (NoButton.Visibility == Visibility.Visible)
					NoButton.Margin = new Thickness(0);
			}
			else if (NoButton.Visibility == Visibility.Collapsed || CancelButton.Visibility == Visibility.Collapsed)
			{
				ButtonGrid.ColumnDefinitions.RemoveAt(1);
				ButtonGrid.ColumnDefinitions.RemoveAt(2);
			}
			if (buttonLabels is not null)
				switch (buttonLabels.Length)
				{
					case 1:
						ConfirmButton.Content = buttonLabels[0];
						break;
					case 2:
						ConfirmButton.Content = buttonLabels[0];
						ConfirmButton.Content = buttonLabels[1];
						break;
					case 3:
						ConfirmButton.Content = buttonLabels[0];
						ConfirmButton.Content = buttonLabels[1];
						ConfirmButton.Content = buttonLabels[2];
						break;
				}
		}

		private void BtnYes_Click(object sender, RoutedEventArgs e)
		{
			Result = MessageBoxResult.Yes;
			DialogResult = true;
		}

		private void BtnNo_Click(object sender, RoutedEventArgs e)
		{
			Result = MessageBoxResult.No;
			DialogResult = false;
		}

		private void BtnCancel_Click(object sender, RoutedEventArgs e)
		{
			Result = MessageBoxResult.Cancel;
			DialogResult = false;
		}
		public static MessageBoxResult Show(string title, string message, string[]? buttonLabels, MessageBoxButtons buttons = MessageBoxButtons.YesNoCancel)
		{
			var msgBox = new Dialog(title, message, buttonLabels, buttons);

			if (System.Windows.Application.Current != null)
			{
				msgBox.Owner = System.Windows.Application.Current.Windows
					.OfType<Window>()
					.FirstOrDefault(w => w.IsActive)
					?? System.Windows.Application.Current.MainWindow;
			}

			msgBox.ShowDialog();
			return msgBox.Result;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			if (Result == MessageBoxResult.None)
				Result = MessageBoxResult.Cancel;
		}
	}
}

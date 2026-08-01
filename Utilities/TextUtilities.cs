using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Celer.Utilities
{
	public class TextUtilities
	{
		public static void SetLinkedText(TextBlock target, string format, params (string Text, string Uri)[] links)
		{
			target.Inlines.Clear();

			foreach (var part in Regex.Split(format, @"(\{\d+\})"))
			{
				var token = Regex.Match(part, @"^\{(\d+)\}$");

				if (token.Success && int.TryParse(token.Groups[1].Value, out int i) && i < links.Length)
				{
					var link = new Hyperlink(new Run(links[i].Text))
					{
						NavigateUri = new Uri(links[i].Uri),
						Foreground = (Brush)Application.Current.FindResource("AccentFillColorDefaultBrush")
					};
					HyperlinkExtensions.SetIsExternal(link, true);
					target.Inlines.Add(link);
				}
				else if (part.Length > 0)
				{
					target.Inlines.Add(new Run(part));
				}
			}
		}
	}
}

using MahApps.Metro.IconPacks;
using System.Windows.Controls;

namespace Celer.Models;

public class TabModule
{
    public string Title { get; set; } = "Tab";
    public PackIconLucideKind Icon { get; set; } = PackIconLucideKind.House;
    public object? Content { get; set; }
    public ScrollBarVisibility VerticalScrollMode { get; set; } = ScrollBarVisibility.Auto;
}
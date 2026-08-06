using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Celer.Models
{
    public class SubviewDescriptor
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Type ViewModelType { get; init; } = typeof(ObservableObject);
    }
}

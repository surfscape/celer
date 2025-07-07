﻿using Celer.ViewModels.OtimizacaoVM;
using System.Windows.Controls;

namespace Celer.Views.UserControls.MainApp.OtimizacaoViews
{
    /// <summary>
    /// Interaction logic for MemoryManagement.xaml
    /// </summary>
    public partial class MemoryManagement : UserControl
    {
        public MemoryManagement(MemoryViewModel memoryViewModel)
        {
            InitializeComponent();
            DataContext = memoryViewModel;
        }
    }
}

using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class AutorunView : UserControl
{
    public AutorunView(AutorunViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

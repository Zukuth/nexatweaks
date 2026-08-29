using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class CleanupView : UserControl
{
    public CleanupView(CleanupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

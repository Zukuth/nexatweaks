using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class BackupView : UserControl
{
    public BackupView(BackupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

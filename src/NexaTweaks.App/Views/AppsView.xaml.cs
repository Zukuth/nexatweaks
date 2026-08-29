using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class AppsView : UserControl
{
    public AppsView(AppsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

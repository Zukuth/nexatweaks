using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class PresetsView : UserControl
{
    public PresetsView(PresetsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

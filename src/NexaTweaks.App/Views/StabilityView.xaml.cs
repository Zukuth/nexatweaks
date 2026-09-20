using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class StabilityView : UserControl
{
    public StabilityView(StabilityViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class BoosterView : UserControl
{
    public BoosterView(BoosterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

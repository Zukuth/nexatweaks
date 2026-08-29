using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class RepairView : UserControl
{
    public RepairView(RepairViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

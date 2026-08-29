using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class NetworkView : UserControl
{
    public NetworkView(NetworkViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

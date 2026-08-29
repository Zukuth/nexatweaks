using System.Windows.Controls;
using NexaTweaks.App.Services;

namespace NexaTweaks.App.Views;

public partial class BusyOverlay : UserControl
{
    public BusyOverlay()
    {
        InitializeComponent();
        DataContext = BusyService.Instance;
    }
}

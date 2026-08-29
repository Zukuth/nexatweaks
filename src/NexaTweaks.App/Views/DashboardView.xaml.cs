using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class DashboardView : UserControl
{
    private readonly DashboardViewModel _viewModel;

    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.HistoryUpdated += OnHistoryUpdated;
        Loaded += (_, _) => viewModel.Resume();
        Unloaded += (_, _) => viewModel.Pause();
    }

    private void OnHistoryUpdated()
    {
        Dispatcher.Invoke(() =>
        {
            CpuSpark.Values = _viewModel.CpuHistory.ToList();
            RamSpark.Values = _viewModel.RamHistory.ToList();
            GpuSpark.Values = _viewModel.GpuHistory.ToList();
        });
    }
}

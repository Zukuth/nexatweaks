using System.Collections.Specialized;
using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class ActivityLogPanel : UserControl
{
    public ActivityLogPanel()
    {
        InitializeComponent();
        var viewModel = ActivityLogViewModel.Instance;
        DataContext = viewModel;

        // Keep the newest line in sight, the way a console does.
        ((INotifyCollectionChanged)viewModel.Entries).CollectionChanged += (_, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add) LogScroll.ScrollToEnd();
        };
    }
}

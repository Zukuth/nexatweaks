using System.Windows.Controls;
using NexaTweaks.App.ViewModels;

namespace NexaTweaks.App.Views;

public partial class TweakCategoryView : UserControl
{
    public TweakCategoryView(TweakCategoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}

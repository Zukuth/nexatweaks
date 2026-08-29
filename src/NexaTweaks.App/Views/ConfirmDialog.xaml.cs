using System.Windows;
using System.Windows.Media;
using NexaTweaks.Core;

namespace NexaTweaks.App.Views;

public partial class ConfirmDialog : Window
{
    public bool Confirmed { get; private set; }

    public ConfirmDialog(string title, string message, RiskLevel risk)
    {
        InitializeComponent();
        TitleBlock.Text = title;
        MessageBlock.Text = message;

        var brushKey = risk switch
        {
            RiskLevel.Risky => "RiskyBrush",
            RiskLevel.Advanced => "AdvancedBrush",
            _ => "SafeBrush",
        };
        RiskBadge.Background = (Brush)FindResource(brushKey);
    }

    private void Confirm_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = true;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        Confirmed = false;
        DialogResult = false;
    }

    public static bool Ask(string title, string message, RiskLevel risk, Window? owner = null)
    {
        var dialog = new ConfirmDialog(title, message, risk) { Owner = owner ?? Application.Current.MainWindow };
        return dialog.ShowDialog() == true && dialog.Confirmed;
    }
}

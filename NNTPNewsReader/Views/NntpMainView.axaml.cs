using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using NNTPNewsReader.ViewModels;

namespace NNTPNewsReader.Views;

public partial class NntpMainView : UserControl
{
    public NntpMainView()
    {
        InitializeComponent();
        
        //https://docs.avaloniaui.net/docs/events/lifecycle-events
        //Loaded fires only after fetching the server data
        Loaded += async (_, _) =>
        {
            if (DataContext is NntpMainViewModel vm)
            {
                await vm.LoadGroupsAsync();
            }
        };
    }
}
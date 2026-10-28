using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NNTPNewsReader.Services;
using NNTPNewsReader.ViewModels;
using NNTPNewsReader.Views;

namespace NNTPNewsReader;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddCommonServices();
        var services = collection.BuildServiceProvider();
        
        var navigation =  services.GetRequiredService< NavigationService>();
        
        navigation.NavigateTo<LoginViewModel>();
        

        DataTemplates.Add(new ViewLocator(services)); 

        var vm = services.GetRequiredService<MainWindowViewModel>();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}
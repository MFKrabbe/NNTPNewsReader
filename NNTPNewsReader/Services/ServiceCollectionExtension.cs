using Microsoft.Extensions.DependencyInjection;
using NNTPNewsReader.ViewModels;
using NNTPNewsReader.Views;

namespace NNTPNewsReader.Services;

public static class ServiceCollectionExtension
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<MainWindowViewModel>();
        
        collection.AddSingleton<LoginViewModel>();
        collection.AddTransient<LoginView>();
        
        collection.AddSingleton<NntpMainViewModel>();
        collection.AddTransient<NntpMainView>();
        
        collection.AddSingleton<NavigationService>();
        
        collection.AddSingleton<INntpClient, NntpClient>();
    }   
}
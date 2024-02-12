using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FeedbackExtractor.Client.Entities;
using FeedbackExtractor.Client.ViewModels;
using FeedbackExtractor.Core.Implementations;
using FeedbackExtractor.Core.Interfaces;
using FeedbackExtractor.DocumentIntelligence.Implementations;
using FeedbackExtractor.OpenAI.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Data;
using System.IO;
using System.ServiceProcess;
using System.Windows;

namespace FeedbackExtractor.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configBuilder = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.local.json", optional: false)
                      .AddJsonFile("appsettings.json", optional: false);

            IConfiguration config = configBuilder.Build();

            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton(config)
                .AddTransient<IMessenger>(sp=> WeakReferenceMessenger.Default)
                .AddKeyedSingleton<IFeedbackExtractor, MockFeedbackExtractor>(ExtractorImplementations.Mock)
                .AddKeyedSingleton<IFeedbackExtractor, DocumentFeedbackExtractor>(ExtractorImplementations.DocumentIntelligence_Base)
                .AddKeyedSingleton<IFeedbackExtractor, CustomFeedbackExtractor>(ExtractorImplementations.DocumentIntelligence_Custom)
                .AddKeyedSingleton<IFeedbackExtractor, OpenAIFeedbackExtractor>(ExtractorImplementations.OpenAI)
                .AddTransient<MainWindowViewModel>()
                .AddLogging()
                .BuildServiceProvider()
            );

            
        }
    }

}

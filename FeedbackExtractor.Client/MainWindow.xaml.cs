using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using FeedbackExtractor.Client.Messages;
using FeedbackExtractor.Client.ViewModels;
using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeedbackExtractor.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMessenger _messenger;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<MainWindowViewModel>();
            _messenger = Ioc.Default.GetRequiredService<IMessenger>();

            _messenger.Register<OpenFileDialogMessage>(this, OpenFileMessageHandler);
        }

        public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public void OpenFileMessageHandler(object sender ,OpenFileDialogMessage message)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = message.Filter;
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;

            var openDialogResult = openFileDialog.ShowDialog();
            if (openDialogResult.HasValue && openDialogResult.Value)
            {
                this._messenger.Send<FileSelectedMessage>(new FileSelectedMessage(openFileDialog.FileName));
                this.FileView.Navigate(openFileDialog.FileName);
            }
        }
    }
}
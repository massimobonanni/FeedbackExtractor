using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FeedbackExtractor.Client.Entities;
using FeedbackExtractor.Client.Messages;
using FeedbackExtractor.Core.Entities;
using FeedbackExtractor.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedbackExtractor.Client.ViewModels
{
    public partial class MainWindowViewModel : ObservableRecipient
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IMessenger _messenger;

        public MainWindowViewModel(IServiceProvider serviceProvider, IMessenger messenger)
        {
            _serviceProvider = serviceProvider;
            _messenger = messenger;

            ExtractionTypes =
            [
                ExtractorImplementations.OpenAI,
                ExtractorImplementations.DocumentIntelligence_Base,
                ExtractorImplementations.DocumentIntelligence_Custom,
            ];
            this.ExtractedSessionFeedback = null;
            this.IsExtractedSessionValid = false;

            _messenger.Register<FileSelectedMessage>(this, FileSelectedMessageHandler);
        }

        [RelayCommand(CanExecute = nameof(CanExtractSessionFeedback))]
        private async Task ExtractSessionFeedbackAsync(CancellationToken token)
        {
            this.IsBusy = true;

            IFeedbackExtractor feedbackExtractor = _serviceProvider.GetRequiredKeyedService<IFeedbackExtractor>(SelectedExtractionType);

            using var fileStream = File.OpenRead(DocumentFilePath);

            this.ExtractedSessionFeedback = await feedbackExtractor.ExtractSessionFeedbackAsync(fileStream);

            if (this.ExtractedSessionFeedback != null)
                this.IsExtractedSessionValid = this.ExtractedSessionFeedback.IsValid();
            else
                this.IsExtractedSessionValid = false;

            this.IsBusy = false;
        }

        private bool CanExtractSessionFeedback()
        {
            return !string.IsNullOrEmpty(this.DocumentFilePath) && !this.IsBusy;
        }

        [RelayCommand(CanExecute = nameof(CanSelectFile))]
        private void SelectFile()
        {
            string filter = "";

            switch (SelectedExtractionType)
            {
                case ExtractorImplementations.OpenAI:
                    filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                    break;
                case ExtractorImplementations.DocumentIntelligence_Base:
                case ExtractorImplementations.DocumentIntelligence_Custom:
                    filter = "PDF Documents (*.pdf)|*.pdf|Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";
                    break;
            }
            this._messenger.Send(new OpenFileDialogMessage(filter));
        }

        private bool CanSelectFile()
        {
            return this.SelectedExtractionType.HasValue && !this.IsBusy;
        }

        public void FileSelectedMessageHandler(object sender, FileSelectedMessage message)
        {
            this.ExtractedSessionFeedback = null;
            this.DocumentFilePath = message.Value;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ExtractSessionFeedbackCommand))]
        private string? documentFilePath;


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectFileCommand))]
        private ExtractorImplementations? selectedExtractionType;


        [ObservableProperty]
        private ObservableCollection<ExtractorImplementations> extractionTypes;


        [ObservableProperty]
        private SessionFeedback? extractedSessionFeedback;

        [ObservableProperty]
        private bool isExtractedSessionValid;


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SelectFileCommand))]
        [NotifyCanExecuteChangedFor(nameof(ExtractSessionFeedbackCommand))]
        private bool isBusy;
        
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using MaximDl;
using ReactiveUI;

namespace Photometer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MaxImDlApp AppInstance { get; set; }
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        public ReactiveCommand<Unit, string[]> LoadFilesCommand { get; private set; }

        public MainWindowViewModel()
        {
            SetUpCommands();
            SetUpObservables();
        }

        private void SetUpCommands()
        {
            LoadFilesCommand = ReactiveCommand.CreateFromTask(ShowOpenFileDialogAsync)
                .DisposeWith(_disposables);
        }

        private void SetUpObservables()
        {
            LoadFilesCommand
                .Select(x => x.Select(System.IO.Path.GetFullPath).ToList())
                .SelectMany(OpenDocumentsAsync)
                .Subscribe()
                .DisposeWith(_disposables);
        }

        private Task <string[]> ShowOpenFileDialogAsync()
        {
            var dialog = new OpenFileDialog()
            {
                AllowMultiple = true,
                Title = @"Open fits files",
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Extensions = new List<string> {"fits", "fts"},
                        Name = "FITS files"
                    }
                }
            };

            return dialog.ShowAsync(Avalonia.Application.Current.MainWindow);
        }

        public async Task<Unit> OpenDocumentsAsync(List<string> input)
        {
            foreach (var item in input)
            {
                await Task.Run(() =>
                {
                    var doc = AppInstance.CreateDocument();
                    doc.OpenFile(item);
                });
            }

            return Unit.Default;
        }

        protected override void Dispose(bool disposing)
        {
            _disposables.Dispose();
            base.Dispose(disposing);
        }
    }
}

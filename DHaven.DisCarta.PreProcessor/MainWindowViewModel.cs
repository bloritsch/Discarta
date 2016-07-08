namespace DHaven.DisCarta.PreProcessor
{
    using Internal;
    using Model;
    using Ookii.Dialogs.Wpf;
    using Properties;

    public class MainWindowViewModel : ViewModel<Preprocessor>
    {
        public MainWindowViewModel() : base(new Preprocessor())
        {
            LoadFileCommand = new RelayCommand(LoadFile);
        }

        private RasterInfo selectedFile;

        public RasterInfo SelectedFile
        {
            get { return selectedFile; }
            set
            {
                if (selectedFile == value)
                {
                    return;
                }

                selectedFile = value;
                RaisePropertyChanged(nameof(SelectedFile));
            }
        }

        public RelayCommand LoadFileCommand { get; }

        private async void LoadFile()
        {
            VistaFileDialog fileDialog = new VistaOpenFileDialog
            {
                Title = Resources.MainWindowViewModel_LoadFile,
                Multiselect = true
            };

            if (fileDialog.ShowDialog() != true)
            {
                return;
            }

            foreach (var name in fileDialog.FileNames)
            {
                await Model.LoadMetadata(name);
            }
        }
    }
}

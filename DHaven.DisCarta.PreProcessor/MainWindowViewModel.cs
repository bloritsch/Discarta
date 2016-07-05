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
            VistaFileDialog fileDialog = new VistaOpenFileDialog();
            fileDialog.Title = Resources.MainWindowViewModel_LoadFile;

            if (fileDialog.ShowDialog() == true)
            {
                await Model.LoadMetadata(fileDialog.FileName);
            }
        }
    }
}

namespace DHaven.DisCarta.PreProcessor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            GdalConfiguration.ConfigureGdal();
            GdalConfiguration.ConfigureOgr();
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}

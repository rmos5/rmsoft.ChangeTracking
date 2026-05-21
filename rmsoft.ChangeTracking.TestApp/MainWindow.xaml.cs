using System.Windows;

namespace rmsoft.ChangeTracking.TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public new MainContext DataContext
        {
            get => (MainContext)base.DataContext;
            set => base.DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainContext();
        }
    }
}

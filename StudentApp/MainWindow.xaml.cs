using System.Windows;

namespace StudentApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
            DataContext = new MainViewModel();
        }
    }
}
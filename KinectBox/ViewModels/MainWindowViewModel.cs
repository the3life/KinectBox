using KinectBox.Views;

namespace KinectBox.ViewModels
{
    public class MainWindowViewModel : ViewModel<MainWindow>
    {
        public string Title { get; } = "Main Window";

        public MainWindowViewModel()
        {
        }
    }
}
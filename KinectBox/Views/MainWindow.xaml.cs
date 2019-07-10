using KinectBox.ViewModels;

namespace KinectBox.Views
{
    public partial class MainWindow : IView
    {
        public MainWindow()
        {
            InitializeComponent();

            ((ViewModel<MainWindow>) DataContext).View = this;
        }
    }
}
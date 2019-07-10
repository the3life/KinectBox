using System.Windows;
using KinectBox.ViewModels;

namespace KinectBox.Views
{
    public partial class BoxViewer : IView
    {
        public BoxViewer()
        {
            InitializeComponent();

            ((ViewModel<BoxViewer>) DataContext).View = this;
        }

        private void BoxViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel<BoxViewer>) DataContext).OnLoaded();
        }
    }
}
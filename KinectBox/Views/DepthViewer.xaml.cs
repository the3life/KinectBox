using System.Windows;
using KinectBox.ViewModels;

namespace KinectBox.Views
{
    public partial class DepthViewer : IView
    {
        public DepthViewer()
        {
            InitializeComponent();

            ((ViewModel<DepthViewer>) DataContext).View = this;
        }

        private void DepthViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel<DepthViewer>) DataContext).OnLoaded();
        }
    }
}
using System.Windows;
using KinectBox.ViewModels;

namespace KinectBox.Views
{
    public partial class ImageViewer : IView
    {
        public ImageViewer()
        {
            InitializeComponent();
            
            ((ViewModel<ImageViewer>) DataContext).View = this;
        }

        private void ImageViewer_OnLoaded(object sender, RoutedEventArgs e)
        {
            ((ViewModel<ImageViewer>) DataContext).OnLoaded();
        }
    }
}
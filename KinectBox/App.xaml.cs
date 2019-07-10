using System.Windows;
using KinectBox.Kinect;
using KinectBox.Kinect.Logic;
using KinectBox.Views;
using Prism.Ioc;

namespace KinectBox
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<KinectManager>();
            containerRegistry.RegisterSingleton<DepthDrawer>();
        }
    }
}
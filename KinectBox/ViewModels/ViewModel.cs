using KinectBox.Views;
using Prism.Mvvm;

namespace KinectBox.ViewModels
{
    public class ViewModel<V> : BindableBase where V : IView
    {
        private V _view;

        public V View
        {
            get { return _view; }
            set { SetProperty(ref _view, value); }
        }

        public virtual void OnLoaded()
        {
        }
    }
}
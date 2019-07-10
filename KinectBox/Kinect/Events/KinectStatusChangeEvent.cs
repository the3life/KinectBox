using Microsoft.Kinect;
using Prism.Events;

namespace KinectBox.Kinect.Events
{
    public class KinectStatusChangeEvent : PubSubEvent<StatusChangedEventArgs>
    {
    }
}
using Microsoft.Kinect;
using Prism.Events;

namespace KinectBox.Kinect.Events
{
    public class KinectColorImageFrameReadyEvent : PubSubEvent<ColorImageFrame>
    {
    }
}
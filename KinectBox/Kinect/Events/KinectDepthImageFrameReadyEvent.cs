using Microsoft.Kinect;
using Prism.Events;

namespace KinectBox.Kinect.Events
{
    public class KinectDepthImageFrameReadyEvent : PubSubEvent<DepthImageFrame>
    {
    }
}
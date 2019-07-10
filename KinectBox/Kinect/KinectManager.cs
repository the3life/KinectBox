using KinectBox.Kinect.Events;
using Microsoft.Kinect;
using Prism.Events;

namespace KinectBox.Kinect
{
    public class KinectManager
    {
        private readonly IEventAggregator _eventAggregator;

        public KinectManager(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            KinectSensor.KinectSensors.StatusChanged += KinectSensorsOnStatusChanged;

            ActiveSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            ActiveSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            //ActiveSensor.ColorFrameReady += ActiveSensorOnColorFrameReady;
            ActiveSensor.DepthFrameReady += ActiveSensorOnDepthFrameReady;

            ActiveSensor.Start();
        }

        public KinectSensor ActiveSensor => KinectUtils.ActiveSensor;

        private void KinectSensorsOnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            _eventAggregator.GetEvent<KinectStatusChangeEvent>().Publish(e);
        }

        private void ActiveSensorOnColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenColorImageFrame())
            {
                if (frame == null) return;

                _eventAggregator.GetEvent<KinectColorImageFrameReadyEvent>().Publish(frame);
            }
        }

        private void ActiveSensorOnDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (var frame = e.OpenDepthImageFrame())
            {
                if (frame == null) return;

                _eventAggregator.GetEvent<KinectDepthImageFrameReadyEvent>().Publish(frame);
            }
        }
    }
}
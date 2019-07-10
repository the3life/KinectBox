using System.Linq;
using Microsoft.Kinect;

namespace KinectBox.Kinect
{
    public class KinectUtils
    {
        public static KinectSensor ActiveSensor
        {
            get
            {
                return KinectSensor.KinectSensors.SingleOrDefault(sensor => sensor.Status == KinectStatus.Connected);
            }
        }
    }
}
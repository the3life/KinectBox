using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Microsoft.Kinect;

namespace KinectBox.Kinect
{
    public class KinectImageProcess
    {
        private KinectSensor _sensor;
        private EventHandler<AllFramesReadyEventArgs> _eventHandler;
        private Action _resetAction;

        private Dispatcher _dispatcher;

        public KinectImageProcess(KinectSensor sensor, EventHandler<AllFramesReadyEventArgs> eventHandler,
            Action resetAction)
        {
            _sensor = sensor;
            _eventHandler = eventHandler;
            _resetAction = resetAction;

            var startEvent = new ManualResetEventSlim();
            var thread = new Thread(ProcessDepthThread) {Name = "KinectDepthViewer-ProcessingThread"};

            thread.Start(startEvent);

            startEvent.Wait();

            if (_dispatcher == null)
            {
                throw new InvalidOperationException("StartEvent was signaled, but no Dispatcher was found.");
            }

            SensorChanged(null, sensor);
        }

        public void SensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (_dispatcher == null)
            {
                throw new InvalidOperationException();
            }

            _dispatcher.BeginInvoke((Action) (() =>
            {
                if (oldSensor != null)
                {
                    _sensor.AllFramesReady -= _eventHandler;
                    _sensor = null;
                }

                if (newSensor != null)
                {
                    _sensor = newSensor;
                    _sensor.AllFramesReady += _eventHandler;
                }

                _resetAction();
            }));
        }

        private void ProcessDepthThread(object e)
        {
            var startEvent = (ManualResetEventSlim) e;

            try
            {
                var dispatcher = Dispatcher.CurrentDispatcher;

                dispatcher.BeginInvoke((Action) (() =>
                {
                    _dispatcher = dispatcher;

                    startEvent.Set();
                }));

                dispatcher.ShutdownStarted += (sender, args) =>
                {
                    if (_sensor != null)
                    {
                        _sensor.AllFramesReady -= _eventHandler;
                        _sensor = null;
                    }
                };

                Dispatcher.Run();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
            finally
            {
                startEvent.Set();
            }
        }
    }
}
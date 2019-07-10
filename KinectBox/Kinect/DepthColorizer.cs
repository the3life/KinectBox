using System;
using System.Windows.Media;
using Microsoft.Kinect;

namespace KinectBox.Kinect
{
    public class DepthColorizer
    {
        private static readonly int[] IntensityShiftByPlayerR = {0, 2, 0, 2, 0, 0, 2, 0};
        private static readonly int[] IntensityShiftByPlayerG = {0, 2, 2, 0, 2, 0, 0, 0};
        private static readonly int[] IntensityShiftByPlayerB = {0, 0, 2, 2, 0, 2, 0, 0};

        private static readonly Color UnknownDepthColor = Colors.Black;
        private static readonly Color TooNearColor = Colors.Green;
        private static readonly Color TooFarColor = Colors.Red;
        private static readonly Color NearestDepthColor = Colors.Blue;

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        private const int RedIndex = 2;
        private const int GreenIndex = 1;
        private const int BlueIndex = 0;

        private const int MinMinDepth = 400;
        private const int MaxMaxDepth = 16383;
        private const int UnknownDepth = 0;

        private static readonly byte[] intensityTable = new byte[MaxMaxDepth + 1]; // 16 KiB

        private Color[] colorMappingTable =
            new Color[short.MaxValue - short.MinValue + 1]; // 192 KiB

        private bool initializeColorMappingTable = true;

        private int currentMinDepth;
        private int currentMaxDepth;
        private KinectDepthTreatment currentDepthTreatment;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance",
            "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification =
                "Loops are necessary to initialize intensityTable")]
        static DepthColorizer()
        {
            for (int i = 0; i < MinMinDepth; i++)
            {
                intensityTable[i] = byte.MaxValue;
            }

            for (int i = MinMinDepth; i < MaxMaxDepth; i++)
            {
                const float DepthRangeScale = 500.0f;
                const int IntensityRangeScale = 74;
                intensityTable[i] = (byte) (~(byte) Math.Min(
                    byte.MaxValue,
                    Math.Log((((double) (i - MinMinDepth)) / DepthRangeScale) + 1) * IntensityRangeScale));
            }
        }

        public void ConvertDepthFrame(
            DepthImagePixel[] depthFrame,
            int minDepth,
            int maxDepth,
            KinectDepthTreatment depthTreatment,
            byte[] colorFrame)
        {
            if ((depthFrame.Length * Bgr32BytesPerPixel) != colorFrame.Length)
            {
                throw new InvalidOperationException();
            }

            Color[] mappingTable = GetColorMappingTable(minDepth, maxDepth, depthTreatment);

            for (int depthIndex = 0, colorIndex = 0;
                colorIndex < colorFrame.Length;
                depthIndex++, colorIndex += Bgr32BytesPerPixel)
            {
                short depth = depthFrame[depthIndex].Depth;
                Color color = mappingTable[(ushort) depth];

                int player = depthFrame[depthIndex].PlayerIndex;

                colorFrame[colorIndex + RedIndex] = (byte) (color.R >> IntensityShiftByPlayerR[player]);
                colorFrame[colorIndex + GreenIndex] = (byte) (color.G >> IntensityShiftByPlayerG[player]);
                colorFrame[colorIndex + BlueIndex] = (byte) (color.B >> IntensityShiftByPlayerB[player]);
            }
        }

        private Color[] GetColorMappingTable(int minDepth, int maxDepth, KinectDepthTreatment depthTreatment)
        {
            if (this.initializeColorMappingTable ||
                minDepth != this.currentMinDepth ||
                maxDepth != this.currentMaxDepth ||
                depthTreatment != this.currentDepthTreatment)
            {
                this.initializeColorMappingTable = false;
                this.currentMinDepth = minDepth;
                this.currentMaxDepth = maxDepth;
                this.currentDepthTreatment = depthTreatment;

                Array.Clear(this.colorMappingTable, 0, this.colorMappingTable.Length);

                this.colorMappingTable[UnknownDepth] = UnknownDepthColor;

                switch (depthTreatment)
                {
                    case KinectDepthTreatment.ClampUnreliableDepths:
                        for (int i = 1; i < minDepth; i++)
                        {
                            this.colorMappingTable[i] = TooNearColor;
                        }

                        for (int i = maxDepth + 1; i < MaxMaxDepth; i++)
                        {
                            this.colorMappingTable[i] = TooFarColor;
                        }

                        break;

                    case KinectDepthTreatment.TintUnreliableDepths:
                        for (int i = 1; i < minDepth; i++)
                        {
                            byte intensity = intensityTable[i];

                            this.colorMappingTable[i] = Color.FromRgb((byte) (intensity >> 3), (byte) (intensity >> 1),
                                intensity);
                        }

                        for (int i = maxDepth + 1; i < MaxMaxDepth; i++)
                        {
                            byte intensity = intensityTable[i];

                            this.colorMappingTable[i] = Color.FromRgb(intensity, (byte) (intensity >> 3),
                                (byte) (intensity >> 1));
                        }

                        break;

                    case KinectDepthTreatment.DisplayAllDepths:
                        for (int i = 1; i < MinMinDepth; i++)
                        {
                            this.colorMappingTable[i] = NearestDepthColor;
                        }

                        minDepth = MinMinDepth;
                        maxDepth = MaxMaxDepth;
                        break;
                }

                for (int i = minDepth; i < maxDepth; i++)
                {
                    byte intensity = intensityTable[i];
                    this.colorMappingTable[i] = Color.FromRgb(intensity, intensity, intensity);
                }
            }

            return this.colorMappingTable;
        }
    }
}
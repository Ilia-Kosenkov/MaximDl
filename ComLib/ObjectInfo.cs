// ReSharper disable InconsistentNaming
using System;

namespace MaxImDL
{



    public readonly struct ObjectInfo
    {
        public float PixelValue { get; }
        public float MaximumValue { get; }
        public float MinimumValue { get; }
        public float MedianValue { get; }
        public float AverageValue { get; }
        public float StdDev { get; }
        public float X { get; }
        public float Y { get; }
        public float Flatness { get; }
        public float FWHM { get; }
        public float HalfFluxDiameter { get; }
        public float FullIntensity { get; }
        public float SNR { get; }
        public float Background { get; }
        public float BackgroundStdDev { get; }
        public float? RightAscension { get; }
        public float? Declination { get; }

        internal ObjectInfo(object[] objects)
        {
            if (objects is null || objects.Length != 17)
                throw new ArgumentException("Incorrect input array format.", nameof(objects));

            PixelValue = (float) objects[0];
            MaximumValue = (float) objects[1];
            MinimumValue = (float) objects[2];
            MedianValue = (float) objects[3];
            AverageValue = (float) objects[4];
            StdDev = (float) objects[5];
            X = (float) objects[6];
            Y = (float) objects[7];
            Flatness = (float) objects[8];
            FWHM = (float) objects[9];
            HalfFluxDiameter = (float) objects[10];
            FullIntensity = (float) objects[11];
            SNR = (float) objects[12];
            Background = (float) objects[13];
            BackgroundStdDev = (float) objects[14];
            RightAscension = objects[15] is float dblRa ? dblRa : (float?) null;
            Declination = objects[15] is float dblDec ? dblDec : (float?) null;
        }

        public static implicit operator ObjectInfo(object[] array)
            => new ObjectInfo(array);

    }
}
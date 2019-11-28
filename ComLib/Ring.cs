using System;

namespace MaxImDL
{


    public enum ImagePlaneType : ushort
    {
        MonochromeOrAverage = 0,
        Red = 1,
        Green = 2,
        Blue = 3
    }

    public readonly struct Ring : IEquatable<Ring>
    {
        public short Aperture { get; }
        public short Gap { get; }
        public short Annulus { get; }
        public ImagePlaneType Type { get; }

        public Ring(short aperture, short gap, short annulus, ImagePlaneType type = ImagePlaneType.MonochromeOrAverage)
        {
            Aperture = aperture;
            Gap = gap;
            Annulus = annulus;
            Type = type;
        }

        internal int[] MarshalAsArray()
            => new int[] {Aperture, Gap, Annulus, (short) Type};


        public static implicit operator int[](Ring r)
            => r.MarshalAsArray();

        public bool Equals(Ring other)
            => Aperture == other.Aperture && Gap == other.Gap && Annulus == other.Annulus && Type == other.Type;

        public override bool Equals(object? obj)
            => obj is Ring other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Aperture.GetHashCode();
                hashCode = (hashCode * 397) ^ Gap.GetHashCode();
                hashCode = (hashCode * 397) ^ Annulus.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }

        public static bool operator ==(Ring left, Ring right)
            => left.Equals(right);

        public static bool operator !=(Ring left, Ring right)
            => !left.Equals(right);
    }
}
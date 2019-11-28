using System;
using MaxImDL;

namespace Playground
{
    internal struct CoordDesc : IEquatable<CoordDesc>
    {
        public (short X, short Y) FirstPosition { get; }
        public (short X, short Y) SecondPosition { get; }
        public Ring Aperture { get; }

        public CoordDesc(
            (short X, short Y) posFirst, 
            (short X, short Y) posSecond,
            Ring aperture)
        {
            FirstPosition = posFirst;
            SecondPosition = posSecond;
            Aperture = aperture;
        }

        public bool Equals(CoordDesc other)
            => FirstPosition.Equals(other.FirstPosition) && SecondPosition.Equals(other.SecondPosition) && Aperture.Equals(other.Aperture);

        public override bool Equals(object? obj) 
            => obj is CoordDesc other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FirstPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ SecondPosition.GetHashCode();
                hashCode = (hashCode * 397) ^ Aperture.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator==(CoordDesc left, CoordDesc right)
            => left.Equals(right);

        public static bool operator !=(CoordDesc left, CoordDesc right)
            => !left.Equals(right);

    }
}

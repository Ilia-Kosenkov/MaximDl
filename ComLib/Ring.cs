public enum ImagePlaneType : ushort
{
    MonochromeOrAverage = 0,
    Red = 1,
    Green = 2,
    Blue = 3
}

public readonly struct Ring
{
    public ushort Aperture {get;}
    public ushort Gap {get;}
    public ushort Annulus {get;}
    public ImagePlaneType Type {get;}

    public Ring(ushort aperture, ushort gap, ushort annulus, ImagePlaneType type = ImagePlaneType.MonochromeOrAverage)
    {
        Aperture = aperture;
        Gap = gap;
        Annulus = annulus;
        Type = type;
    }

    internal readonly int[] MarshalAsArray()
        => new int[] {Aperture, Gap, Annulus, (ushort)Type};
    

    public static implicit operator int[](Ring r)
        => r.MarshalAsArray();
}
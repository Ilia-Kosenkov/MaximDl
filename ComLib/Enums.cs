// ReSharper disable InconsistentNaming

using System;

public enum BinType : byte
{
    NoBin = 0,
    Bin2x2 = 2,
    Bin3x3 = 3
}

public enum PixelDataFormat : byte
{
    I8Bit = 1,
    I16Bit = 2,
    I32Bit = 3,
    F32Bit = 4
}

public enum ImageType : byte
{
    SBIGT3 = 0,
    SBIGST4 = 1,
    PCLynxx = 2,
    Fits = 3,
    Raw = 4,
    Tiff = 5,
    Jpeg = 6,
    Png = 7,
    Bmp = 8
}
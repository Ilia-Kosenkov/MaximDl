using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaximDl
{
    public sealed class MaxImDlDoc : ComType
    {
        //private static MaxImDlDoc? _instance;

        internal MaxImDlDoc() 
            : base (@"MaxIm.Document")
        {
            ComInstance = Activator.CreateInstance(Type)
                          // ReSharper disable once ConstantNullCoalescingCondition
                          ?? throw new InvalidOperationException("Failed to acquire look on the ComObject.");
        }

        internal MaxImDlDoc(object comInstance) : base (@"MaxIm.Document") => ComInstance = comInstance;

        public bool MouseUp => FromGetter<bool>();
        // It seems the property is not defined or the name is incorrect
        public bool MouseDown => !MouseUp;//FromGetter<bool>();
        public short MouseNewClick => FromGetter<short>();
        public short MouseX => FromGetter<short>();
        public short MouseY => FromGetter<short>();
        public short MouseRadius => FromGetter<short>();
        public short MouseGapWidth => FromGetter<short>();
        public short MouseAnnulusWidth => FromGetter<short>();
        public string DisplayName => FromGetter<string>();
        public (short X, short Y) MousePosition => (MouseX, MouseY);

        public ObjectInfo CalcInformation(short x, short y, Ring r) 
            => FromMethodInvoke<object[]>(nameof(CalcInformation), x, y, r.MarshalAsArray());

        public void OpenFile(string path)
            => InvokeMethod(nameof(OpenFile), path);

        public void Calibrate()
            => InvokeMethod(nameof(Calibrate));
        
        public bool BringToTop() 
            => FromMethodInvoke<bool>();

        public bool Close() 
            => FromMethodInvoke<bool>();

        // ReSharper disable once InconsistentNaming
        public object? GetFITSKey(string keyName)
            => FromMethodInvoke<object>(args: keyName);

        public void Bin(BinType bin)
        {
            if (bin == BinType.NoBin)
                throw new ArgumentException($"Cannot bin image using {bin} regime", nameof(bin));
            
            InvokeMethod(nameof(Bin), (ushort)bin);
        }
        public void SaveFile(string path, ImageType format = ImageType.Fits, bool stretch = false, PixelDataFormat sizeFormat = PixelDataFormat.I16Bit, short compression = 0)
            => InvokeMethod(nameof(SaveFile), path, (byte)format, stretch, (byte)sizeFormat, compression);


        public Task<bool> AwaitMouseNewClickEventAsync(TimeSpan timeout)
            => Task.Run(() => SpinWait.SpinUntil(() => MouseNewClick != 0, timeout));

        public Task AwaitMouseNewClickEventAsync()
            => Task.Run(() => SpinWait.SpinUntil(() => MouseNewClick != 0));
    }

}
using System;

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

        public short MouseNewClick => FromGetter<short>();
        public short MouseX => FromGetter<short>();
        public short MouseY => FromGetter<short>();
        public string DisplayName => FromGetter<string>();

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

        public void Bin(BinType bin)
        {
            if (bin == BinType.NoBin)
                throw new ArgumentException($"Cannot bin image using {bin} regime", nameof(bin));
            
            InvokeMethod(nameof(Bin), (ushort)bin);
        }
        public void SaveFile(string path, byte format, bool stretch = false, byte sizeFormat = 1, short compression = 0)
            => InvokeMethod(nameof(SaveFile), path, format, stretch, sizeFormat, compression);
    }

}
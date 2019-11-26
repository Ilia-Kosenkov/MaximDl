using System;

namespace MaximDl
{
    public sealed class MaxImDlDoc : ComType
    {
        private static MaxImDlDoc _instance;

        private MaxImDlDoc() 
            : base (@"MaxIm.Document")
        {
            ComInstance = Activator.CreateInstance(Type)
                ?? throw new InvalidOperationException("Failed to acquire look on the ComObject.");
        }

        public short MouseNewClick => (short)InvokeGetter(nameof(MouseNewClick));

        public short MouseX => (short)InvokeGetter(nameof(MouseX));
        public short MouseY => (short)InvokeGetter(nameof(MouseY));

        public ObjectInfo CalcInformation(short x, short y, Ring r)
            => (object[])InvokeMethod(nameof(CalcInformation), x, y, r.MarshalAsArray());

        public void OpenFile(string path)
            => InvokeMethod(nameof(OpenFile), path);

        public void Calibrate()
            => InvokeMethod(nameof(Calibrate));

        public void Bin(BinType bin)
        {
            if (bin == BinType.NoBin)
                throw new ArgumentException($"Cannot bin image using {bin} regime", nameof(bin));
            
            InvokeMethod(nameof(Bin), (ushort)bin);
        }
        public void SaveFile(string path, byte format, bool stretch = false, byte sizeFormat = 1, short compression = 0)
            => InvokeMethod(nameof(SaveFile), new object[] {path, format, stretch, sizeFormat, compression});
        
        public static MaxImDlDoc Acquire()
        {
            if(_instance is null)
            {
                _instance = new MaxImDlDoc();
                return _instance;    
            }
            
            throw new InvalidOperationException("Cannot acquire instance of the ComObject because it is alread in use.");
        }

        protected override void Dispose(bool disposing)
        {
            if(!IsDisposed && disposing)
            {
                if(ReferenceEquals(this, _instance))
                    _instance = null;
            }

            base.Dispose(disposing);
        }
    }

}
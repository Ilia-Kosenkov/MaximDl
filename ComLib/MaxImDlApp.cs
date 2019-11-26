using System;

namespace MaximDl
{
    public sealed class MaxImDlApp : ComType
    {
        private static MaxImDlApp? _instance;

        public bool CalMedianBias
        {
            get => FromGetter<bool>();
            set => FromSetter(value);
        }

        public bool CalMedianDark 
        {
            get => FromGetter<bool>();
            set => FromSetter(value);
        }


        private MaxImDlApp() 
            : base (@"MaxIm.Application")
        {
            ComInstance = Activator.CreateInstance(Type)
                ?? throw new InvalidOperationException("Failed to acquire look on the ComObject.");
        }

        public void CalClear()
            => InvokeMethod(nameof(CalClear), Array.Empty<object>());

        public bool CalAddBias(string path) => FromMethodInvoke<bool>(args: path);
        public bool CalAddDark(string path) => FromMethodInvoke<bool>(args: path);

        public bool CalSet() => FromMethodInvoke<bool>();

        public static MaxImDlApp Acquire()
        {
            if(_instance is null)
            {
                _instance = new MaxImDlApp();
                return _instance;    
            }
            
            throw new InvalidOperationException("Cannot acquire instance of the ComObject because it is already in use.");
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
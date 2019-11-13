using System;
using System.Runtime.InteropServices;

namespace MaximDl
{
    public sealed class MaxImDlApp : ComType
    {
        private static MaxImDlApp _instance;

        public bool CalMedianBias 
        {
            get => (bool)InvokeGetter(nameof(CalMedianBias));
            set => InvokeSetter(nameof(CalMedianBias), value);
        }

        public bool CalMedianDark 
        {
            get => (bool)InvokeGetter(nameof(CalMedianDark));
            set => InvokeSetter(nameof(CalMedianDark), value);
        }


        private MaxImDlApp() 
            : base (@"MaxIm.Application")
        {
            _comInstance = Activator.CreateInstance(Type)
                ?? throw new InvalidOperationException("Failed to acquire look on the ComObject.");
        }

        public void CalClear()
            => InvokeMethod(nameof(CalClear), Array.Empty<object>());
        public bool CalAddBias(string path)
            => (bool)InvokeMethod(nameof(CalAddBias), path);
        public bool CalAddDark(string path)
            => (bool)InvokeMethod(nameof(CalAddBias), path);

        public bool CalSet()
            => (bool)InvokeMethod(nameof(CalSet));

        public static MaxImDlApp Acquire()
        {
            if(_instance is null)
            {
                _instance = new MaxImDlApp();
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
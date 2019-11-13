using System;
using System.Runtime.InteropServices;

namespace MaximDl
{
    public sealed class MaxImDlApp : ComType
    {
        private static MaxImDlApp _instance;

        private MaxImDlApp() 
            : base (@"MaxIm.Application")
        {
            _comInstance = Activator.CreateInstance(Type)
                ?? throw new InvalidOperationException("Failed to acquire look on the ComObject.");
        }

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
using System;
using System.Runtime.InteropServices;

public abstract class ComType : IDisposable
{
    protected static Type Type { get; private set;}
    private bool _isDisposed;
    protected object _comInstance;

    public bool IsDisposed => _isDisposed;

    protected ComType(string protoType)
    {
        if(Type is null)
            Type = Type.GetTypeFromProgID(protoType)
                ?? throw new InvalidOperationException("ComType is not supported.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Dispose(true);
    }

    protected void CheckDisosed()
    {
        if(_isDisposed)
            throw new ObjectDisposedException(@"Object already disposed");
    }

    protected virtual void Dispose(bool disposing)
    {
        if(_isDisposed)
            return;
        
        if(disposing)
        {
        }

        if(!(_comInstance is null))
            Marshal.ReleaseComObject(_comInstance);
        
        _isDisposed = true;
    }

    ~ComType()
    {
        Dispose(false);
    }

}
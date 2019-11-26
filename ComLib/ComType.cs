using System;
using System.Runtime.InteropServices;

public abstract class ComType : IDisposable
{
    protected Type Type { get; private set;}
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

    protected void InvokeSetter(string name, object arg)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Setter name cannot be empty.", nameof(name));

        if(Type is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        var result = Type.InvokeMember(name, System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance, null, _comInstance, new object[] { arg });
    }

    protected object InvokeGetter(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Getter name cannot be empty.", nameof(name));

        if(Type is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        return Type.InvokeMember(name, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance, null, _comInstance, Array.Empty<object>());
    }

    protected object InvokeMethod(string name, params object[] args)
    {
        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Method name cannot be empty.", nameof(name));

        if(Type is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        return Type.InvokeMember(name, 
           System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, 
           null, _comInstance, args);

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
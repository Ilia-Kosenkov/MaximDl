using System;
using System.Runtime.InteropServices;

public abstract class ComType : IDisposable
{
    protected Type Type { get; }

    protected object? ComInstance;

    public bool IsDisposed { get; private set; }

    protected ComType(string protoType)
        => Type = Type.GetTypeFromProgID(protoType) ?? throw new InvalidOperationException("ComType is not supported.");

    protected ComType(Type type)
        => Type = type.IsCOMObject ? type : throw new ArgumentException("Provided type is not COM type.", nameof(type));

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void InvokeSetter(string name, object arg)
    {
        CheckDisposed();

        if(string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Setter name cannot be empty.", nameof(name));

        if(Type is null || ComInstance is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        Type.InvokeMember(name, System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance, null, ComInstance, new[] { arg });
    }

    protected object? InvokeGetter(string name)
    {
        CheckDisposed();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Getter name cannot be empty.", nameof(name));

        if(Type is null || ComInstance is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        return Type.InvokeMember(name, System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance, null, ComInstance, Array.Empty<object>());
    }

    protected object? InvokeMethod(string name, params object[] args)
    {
        CheckDisposed();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(@"Method name cannot be empty.", nameof(name));

        if(Type is null || ComInstance is null)        
            throw new InvalidOperationException("Object state is corrupted.");

        return Type.InvokeMember(name, 
           System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, 
           null, ComInstance, args);

    }

    protected void CheckDisposed()
    {
        if(IsDisposed || ComInstance is null)
            throw new ObjectDisposedException(@"Object already disposed");
    }

    protected virtual void Dispose(bool disposing)
    {
        if(IsDisposed)
            return;
        
        if(disposing)
        {
        }

        if (!(ComInstance is null))
        {
            Marshal.ReleaseComObject(ComInstance);
            ComInstance = null;
        }

        IsDisposed = true;
    }

    protected T FromGetter<T>([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
        => (T)(InvokeGetter(prop) ?? throw new InvalidOperationException($"Unexpected property ({prop}) return value."));

    protected void FromSetter<T>(T value, [System.Runtime.CompilerServices.CallerMemberName]string prop = "")
        => InvokeSetter(prop, value!);

    protected T FromMethodInvoke<T>([System.Runtime.CompilerServices.CallerMemberName]
        string method = "", params object[] args)
        => (T) (InvokeMethod(method, args) ??
                throw new InvalidOperationException($"Unexpected method ({method}) return value."));

    ~ComType()
    {
        Dispose(false);
    }

}
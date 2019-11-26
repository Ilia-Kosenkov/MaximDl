using System;

namespace MaximDl
{
    public sealed class MaxImDlDocCollection : ComType
    {
        
        public int Count => FromGetter<int>();
        public MaxImDlDoc this[int i] => 
            new MaxImDlDoc(Type.InvokeMember(@"Item", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance, null, ComInstance, new object[] {i + 1}));

        internal MaxImDlDocCollection(object comInstance) : base (comInstance.GetType()) 
        {
            ComInstance = comInstance;
        }


    }

}
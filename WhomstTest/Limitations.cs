using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WhomstTest
{
    public class Memory
    {
        public static int SizeOf<T>() where T : struct
        {
            var t = typeof(T);
            return t.IsEnum ? Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T))) : Marshal.SizeOf(t);
        }
    }

    public abstract partial class RawArray<T> : IDisposable, IList<T>
        where T : struct
    {
        protected RawArray(int length)
        {
            _ptr = Marshal.AllocHGlobal(length * Memory.SizeOf<T>());
            _length = length;
        }

        private IntPtr _ptr;
        private int _length;

        public IntPtr Ptr
        {
            get { return _ptr; }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (array == null
                || arrayIndex < 0
                || (arrayIndex + _length > array.Length))
            {
                throw new ArgumentException();
            }

            for (int i = 0; i < this.Count; i++)
            {
                array[arrayIndex + i] = this[i];
            }
        }

        public int Count { get { return _length; } }
        public bool IsReadOnly { get { return false; } }

        public T this[int index]
        {
            get
            {
                T val = default(T);
                ReadPtr(PtrAt(index), ref val);
                return val;
            }
            set
            {
                WritePtr(PtrAt(index), ref value);
            }
        }

        public abstract void ReadPtr(IntPtr ptr, ref T val);
        public abstract void WritePtr(IntPtr ptr, ref T val);

        /*public void ReadPtr_NOPE(IntPtr ptr, ref T val)
        {
            val = *(T*)ptr;
        }

        public void WritePtr_NOPE(IntPtr ptr, ref T val)
        {
            *(T*)ptr = val;
        }*/

        public IntPtr PtrAt(int i)
        {
            return (IntPtr)((long)Ptr + i * Memory.SizeOf<T>());
        }

        private void ReleaseUnmanagedResources()
        {
            Marshal.FreeHGlobal(_ptr);
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {

            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RawArray()
        {
            Dispose(false);
        }
    }

    /*{{
        var template = @"
    public class RawArray_TEMPLATE : RawArray<TEMPLATE>
    {
        public RawArray_TEMPLATE(int length) : base(length) { }
    
        public override unsafe void ReadPtr(IntPtr ptr, ref TEMPLATE val)
        {
            val = *(TEMPLATE*)ptr;
        }
    
        public override unsafe void WritePtr(IntPtr ptr, ref TEMPLATE val)
        {
            *(TEMPLATE*)ptr = val;
        }
    }";
             
        var rawArrayTypes = new[] {"RTSEnemy", "int", "float"};
        
        foreach (var t in rawArrayTypes)
        {
            WhomstOut.WriteLine(template.Replace("TEMPLATE", t));
        }
    }}*/
    
    public class RawArray_RTSEnemy : RawArray<RTSEnemy>
    {
        public RawArray_RTSEnemy(int length) : base(length) { }
    
        public override unsafe void ReadPtr(IntPtr ptr, ref RTSEnemy val)
        {
            val = *(RTSEnemy*)ptr;
        }
    
        public override unsafe void WritePtr(IntPtr ptr, ref RTSEnemy val)
        {
            *(RTSEnemy*)ptr = val;
        }
    }
    
    public class RawArray_int : RawArray<int>
    {
        public RawArray_int(int length) : base(length) { }
    
        public override unsafe void ReadPtr(IntPtr ptr, ref int val)
        {
            val = *(int*)ptr;
        }
    
        public override unsafe void WritePtr(IntPtr ptr, ref int val)
        {
            *(int*)ptr = val;
        }
    }
    
    public class RawArray_float : RawArray<float>
    {
        public RawArray_float(int length) : base(length) { }
    
        public override unsafe void ReadPtr(IntPtr ptr, ref float val)
        {
            val = *(float*)ptr;
        }
    
        public override unsafe void WritePtr(IntPtr ptr, ref float val)
        {
            *(float*)ptr = val;
        }
    }
    //{} HASH: 9EF434582AB8A6C0783FB43B8A4CF12C
    //!! Other examples: multidimensional arrays, arithmetic, vectors

    public class RawArrayUser
    {
        RawArray<int> IntArray = new RawArray_int(100);
    }

    partial class RawArray<T>
    {
        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}

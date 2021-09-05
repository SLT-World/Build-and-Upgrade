using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public class Packet
{
    List<byte> Bytes;
    int Index;

    public Packet()
        => Initialize();
    public Packet(byte[] _Bytes)
        => Initialize(_Bytes);
    public Packet(List<byte> _Bytes)
        => Initialize(_Bytes);

    public void Initialize()
    {
        Bytes = new List<byte>();
        Index = 0;
    }
    public void Initialize(byte[] _Bytes)
    {
        if (_Bytes != null && _Bytes.Length > 0)
        {
            Bytes = _Bytes.ToList();
            Index = 0;
        }
        else
            Initialize();
    }
    public void Initialize(List<byte> _Bytes)
    {
        if (_Bytes != null && _Bytes.Count > 0)
        {
            Bytes = _Bytes;
            Index = 0;
        }
        else
            Initialize();
    }
    /*public Packet(List<byte> _Bytes)
        => Bytes = _Bytes;*/
    /*public Packet(System.Object _Object)
    {
        foreach (var _PropertyInfo in _Object.GetType().GetFields())
        {
            Debug.Log(_PropertyInfo.GetValue(_Object));
            AddByte(_PropertyInfo.GetValue(_Object));//_PropertyInfo.GetValue(_Object)
        }
    }*/
    public void SetBytes(byte[] _Bytes)
    {
        Reset();
        Write(_Bytes);
    }

    public List<byte> ToList()
    {
        return Bytes;
    }

    public byte[] ToArray()
    {
        return Bytes.ToArray();
    }

    public int Length { get { return Bytes.Count; } }

    public int UnreadLength()
    {
        return Length - Index;
    }

    public void Reset()
    {
        Bytes.Clear();
        Index = 0;
    }

    public void Write(byte i)
    {
        Bytes.Add(i);
    }
    public void Write(byte[] i)
    {
        Bytes.AddRange(i);
    }
    public void Write(object i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(bool i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(string i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(short i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(int i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(long i)
    {
        Bytes.Add(Convert.ToByte(i));
    }
    public void Write(float i)
    {
        Bytes.Add(Convert.ToByte(i));
    }

    public byte ReadByte()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return _Byte;
        }
        else
            throw new Exception("Could not read value of type 'byte'!");
    }
    public bool ReadBool()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToBoolean(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'bool'!");
    }
    public string ReadString()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToString(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'string'!");
    }
    public short ReadShort()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToInt16(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'short'!");
    }
    public int ReadInt()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToInt32(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'int'!");
    }
    public long ReadLong()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToInt64(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'long'!");
    }
    public float ReadFloat()
    {
        if (Index < Bytes.Count)
        {
            byte _Byte = Bytes[Index];
            Index++;
            return Convert.ToSingle(_Byte);
        }
        else
            throw new Exception("Could not read value of type 'float'!");
    }

    private bool Disposed = false;

    protected virtual void Dispose(bool _Disposing)
    {
        if (!Disposed)
        {
            if (_Disposing)
            {
                Bytes = null;
                Index = 0;
            }
            Disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

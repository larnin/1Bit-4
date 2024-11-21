using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public static class Cast
{
    //public static unsafe TDest Reinterpret<TSource, TDest>(TSource source)
    //{
    //    var sourceRef = __makeref(source);
    //    var dest = default(TDest);
    //    var destRef = __makeref(dest);
    //    *(IntPtr*)&destRef = *(IntPtr*)&sourceRef;
    //    return __refvalue(destRef, TDest);
    //}

    public static ulong ReinterpretToUlong(float source)
    {
        Span<byte> data = stackalloc byte[4];
        BitConverter.TryWriteBytes(data, source);
        return BitConverter.ToUInt64(data);
    }

    public static int HashString(string str)
    {
        MD5 md5Hasher = MD5.Create();
        var hashed = md5Hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
        return BitConverter.ToInt32(hashed, 0);
    }
}

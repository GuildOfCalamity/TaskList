using System;
using static System.Net.Mime.MediaTypeNames;

namespace Task_List_App.Helpers;

/// <summary><example><code>
///     byte test1 = 0xFF; // 1111 1111
///     int  test2 = 0x80; // 1000 0000
///     var bt = test1.SetBit(1);
///     Debug.WriteLine($"{bt.ToBinaryString()}");
///     bt = test1.ClearBit(1);
///     Debug.WriteLine($"{bt.ToBinaryString()}");
///     bt = test1.ToggleBit(1);
///     Debug.WriteLine($"{bt.ToBinaryString()}");
///     var isIt = test2.IsBitSet(7);
///     Debug.WriteLine($"IsBitSet: {isIt}");
/// </code></example></summary>
public static class ByteExtensions
{
    public static bool IsBitSet(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b & (1 << pos)) != 0;
    }

    public static bool IsBitSet(this int b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b & (1 << pos)) != 0;
    }

    public static byte SetBit(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (byte)(b | (1 << pos));
    }

    public static int SetBit(this int b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b | (1 << pos));
    }

    public static byte ClearBit(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (byte)(b & ~(1 << pos));
    }

    public static int ClearBit(this int b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b & ~(1 << pos));
    }

    public static byte ToggleBit(this byte b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (byte)(b ^ (1 << pos));
    }

    public static int ToggleBit(this int b, int pos)
    {
        if (pos < 0 || pos > 7)
            throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");

        return (b ^ (1 << pos));
    }

    public static string ToBinaryString(this byte b)
    {
        return Convert.ToString(b, 2).PadLeft(8, '0');
    }

    public static bool BitsAreSet(this int number, params int[] positions)
    {
        var mask = 0;

        foreach (var position in positions)
            mask |= (1 << position);

        return (number & mask) == mask;
    }

    public static string BinaryToHex(this string strBinary)
    {
        try
        {
            string strHex = Convert.ToInt32(strBinary, 2).ToString("X");
            return strHex;
        }
        catch (Exception)
        {
            Debug.WriteLine($"[WARNING] Failed to convert '{strBinary}'");
            return string.Empty;
        }
    }

    public static string HexToBinary(this string strHex, int padding = 8)
    {
        try
        {
            return String.Join(String.Empty, strHex.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(padding, '0')));
        }
        catch (Exception)
        {
            Debug.WriteLine($"[WARNING] Failed to convert '{strHex}'");
            return 0.ToString().PadLeft(padding, '0');
        }
    }
}

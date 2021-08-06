using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using LiteNetLib.Utils;

namespace BeatTogether.Extensions
{
    public static class NetDataReaderExtensions
    {
        public static ulong GetVarULong(this NetDataReader reader)
        {
            var value = 0UL;
            var shift = 0;
            var b = reader.GetByte();
            while ((b & 128UL) != 0UL)
            {
                value |= (b & 127UL) << shift;
                shift += 7;
                b = reader.GetByte();
            }
            return value | (ulong)b << shift;
        }

        public static long GetVarLong(this NetDataReader reader)
        {
            var varULong = (long)reader.GetVarULong();
            if ((varULong & 1L) != 1L)
                return varULong >> 1;
            return -(varULong >> 1) + 1L;
        }

        public static uint GetVarUInt(this NetDataReader reader) =>
            (uint)reader.GetVarULong();

        public static int GetVarInt(this NetDataReader reader) =>
            (int)reader.GetVarLong();

        public static bool TryGetVarULong(this NetDataReader reader, [MaybeNullWhen(false)] out ulong value)
        {
            value = 0UL;
            var shift = 0;
            while (shift <= 63 && reader.AvailableBytes >= 1)
            {
                var b = reader.GetByte();
                value |= (ulong)(b & 127) << shift;
                shift += 7;
                if ((b & 128) == 0)
                    return true;
            }

            value = 0UL;
            return false;
        }

        public static bool TryGetVarUInt(this NetDataReader reader, [MaybeNullWhen(false)] out uint value)
        {
            ulong num;
            if (reader.TryGetVarULong(out num) && (num >> 32) == 0UL)
            {
                value = (uint)num;
                return true;
            }

            value = 0U;
            return false;
        }

        public static Color GetColor(this NetDataReader reader)
        {
            var r = reader.GetByte();
            var g = reader.GetByte();
            var b = reader.GetByte();
            var a = reader.GetByte();
            return Color.FromArgb(a, r, g, b);
        }
    }
}

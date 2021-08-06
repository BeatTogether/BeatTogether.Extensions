using System.Drawing;
using LiteNetLib.Utils;

namespace BeatTogether.Extensions
{
    public static class NetDataWriterExtensions
    {
        public static void PutVarULong(this NetDataWriter writer, ulong value)
        {
            do
            {
                var b = (byte)(value & 127UL);
                value >>= 7;
                if (value != 0UL)
                    b |= 128;
                writer.Put(b);
            } while (value != 0UL);
        }

        public static void PutVarLong(this NetDataWriter writer, long value) =>
            writer.PutVarULong((value < 0L ? (ulong)((-value << 1) - 1L) : (ulong)(value << 1)));

        public static void PutVarUInt(this NetDataWriter writer, uint value) =>
            writer.PutVarULong(value);

        public static void PutVarInt(this NetDataWriter writer, int value) =>
            writer.PutVarLong(value);

        public static void Put(this NetDataWriter writer, Color value)
        {
            writer.Put(value.R);
            writer.Put(value.G);
            writer.Put(value.B);
            writer.Put(value.A);
        }
    }
}

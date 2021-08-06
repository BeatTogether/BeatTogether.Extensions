using System;
using System.Drawing;
using System.Net;
using System.Text;
using BinaryRecords;

namespace BeatTogether.Extensions
{
    public static class BinaryBufferWriterExtensions
    {
        public static void WriteVarULong(this ref BinaryBufferWriter bufferWriter, ulong value)
        {
            do
            {
                var b = (byte)(value & 127UL);
                value >>= 7;
                if (value != 0UL)
                    b |= 128;
                bufferWriter.WriteUInt8(b);
            } while (value != 0UL);
        }

        public static void WriteVarLong(this ref BinaryBufferWriter bufferWriter, long value) =>
            bufferWriter.WriteVarULong((value < 0L ? (ulong)((-value << 1) - 1L) : (ulong)(value << 1)));

        public static void WriteVarUInt(this ref BinaryBufferWriter bufferWriter, uint value) =>
            bufferWriter.WriteVarULong(value);

        public static void WriteVarInt(this ref BinaryBufferWriter bufferWriter, int value) =>
            bufferWriter.WriteVarLong(value);

        public static void WriteVarBytes(this ref BinaryBufferWriter bufferWriter, ReadOnlySpan<byte> value)
        {
            bufferWriter.WriteVarUInt((uint)value.Length);
            bufferWriter.WriteBytes(value);
        }

        public static void WriteString(this ref BinaryBufferWriter bufferWriter, string value)
        {
            bufferWriter.WriteInt32(Encoding.UTF8.GetByteCount(value));
            bufferWriter.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteIPEndPoint(this ref BinaryBufferWriter bufferWriter, IPEndPoint value)
        {
            bufferWriter.WriteString(value.Address.ToString());
            bufferWriter.WriteInt32(value.Port);
        }

        public static void WriteColor(this ref BinaryBufferWriter bufferWriter, Color value)
        {
            bufferWriter.WriteUInt8(value.R);
            bufferWriter.WriteUInt8(value.G);
            bufferWriter.WriteUInt8(value.B);
            bufferWriter.WriteUInt8(value.A);
        }
    }
}

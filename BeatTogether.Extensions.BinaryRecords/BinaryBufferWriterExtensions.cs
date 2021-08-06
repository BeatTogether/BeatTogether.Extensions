using System;
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

        public static void WriteVarLong(this ref BinaryBufferWriter bufferWriter, long value)
            => bufferWriter.WriteVarULong((value < 0L ? (ulong)((-value << 1) - 1L) : (ulong)(value << 1)));

        public static void WriteVarUInt(this ref BinaryBufferWriter bufferWriter, uint value)
            => bufferWriter.WriteVarULong(value);

        public static void WriteVarInt(this ref BinaryBufferWriter bufferWriter, int value)
            => bufferWriter.WriteVarLong(value);

        public static void WriteVarBytes(this ref BinaryBufferWriter bufferWriter, ReadOnlySpan<byte> bytes)
        {
            bufferWriter.WriteVarUInt((uint)bytes.Length);
            bufferWriter.WriteBytes(bytes);
        }

        public static void WriteString(this ref BinaryBufferWriter bufferWriter, string value)
        {
            bufferWriter.WriteInt32(Encoding.UTF8.GetByteCount(value));
            bufferWriter.WriteBytes(Encoding.UTF8.GetBytes(value));
        }

        public static void WriteIPEndPoint(this ref BinaryBufferWriter bufferWriter, IPEndPoint ipEndPoint)
        {
            bufferWriter.WriteString(ipEndPoint.Address.ToString());
            bufferWriter.WriteInt32(ipEndPoint.Port);
        }
    }
}

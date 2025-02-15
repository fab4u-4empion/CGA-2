using System.Numerics;

namespace CGA2.Utils
{
    public  class SpanReader
    {
        public static Vector3 ReadVector3(Span<byte> buffer, int offset)
        {
            return new(
                    ReadFloat(buffer, offset),
                    ReadFloat(buffer, offset + 4),
                    ReadFloat(buffer, offset + 8)
                );
        }

        public static Vector2 ReadVector2(Span<byte> buffer, int offset)
        {
            return new(
                    ReadFloat(buffer, offset),
                    ReadFloat(buffer, offset + 4)
                );
        }

        public static float ReadFloat(Span<byte> buffer, int offset)
        {
            return BitConverter.ToSingle(buffer.Slice(offset, 4));
        }

        public static ushort ReadUShort(Span<byte> buffer, int offset)
        {
            return BitConverter.ToUInt16(buffer.Slice(offset, 2));
        }
    }
}

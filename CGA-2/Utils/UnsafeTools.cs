namespace CGA2.Utils
{
    public class UnsafeTools
    {
        public static unsafe float ChangeEndianness(float f)
        {
            uint i = *(uint*)&f;

            i = (i & 0x000000FFU) << 24 | (i & 0x0000FF00U) << 8 |
                (i & 0x00FF0000U) >> 8 | (i & 0xFF000000U) >> 24;

            return *(float*)&i;
        }
    }
}

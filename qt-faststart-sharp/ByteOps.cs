namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// 大端字节序读写辅助方法 (对应原 C 版本的 BE_32/BE_64/AV_WB32/AV_WB64 宏)
internal static class ByteOps
{
    public static uint ReadBe32(byte[] buffer, int offset) =>
        (uint)((buffer[offset] << 24) |
               (buffer[offset + 1] << 16) |
               (buffer[offset + 2] << 8) |
               buffer[offset + 3]);

    public static ulong ReadBe64(byte[] buffer, int offset)
    {
        ulong hi = ReadBe32(buffer, offset);
        ulong lo = ReadBe32(buffer, offset + 4);
        return (hi << 32) | lo;
    }

    public static void WriteBe32(byte[] buffer, int offset, uint value)
    {
        buffer[offset] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)value;
    }

    public static void WriteBe64(byte[] buffer, int offset, ulong value)
    {
        WriteBe32(buffer, offset, (uint)(value >> 32));
        WriteBe32(buffer, offset + 4, (uint)value);
    }
}

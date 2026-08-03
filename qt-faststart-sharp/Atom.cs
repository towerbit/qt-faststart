namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// 描述一个已解析的 atom：
// - Type: FourCC 类型
// - HeaderSize: atom 头部长度 (8 或 16 字节，取决于是否使用 64 位扩展 size)
// - Size: 负载(payload)长度，不包含头部
// - DataOffset: 负载在所属 byte[] 缓冲区中的起始偏移量
internal readonly record struct Atom(uint Type, int HeaderSize, long Size, int DataOffset)
{
    public int HeaderOffset => DataOffset - HeaderSize;
}

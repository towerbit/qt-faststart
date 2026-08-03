namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// QuickTime/MP4 顶层及嵌套 atom 的 FourCC 常量
// (等价于原 C 版本中的 QT_ATOM(...) 宏定义)
internal static class AtomTypes
{
    public const uint Free = 0x66726565; // 'free'
    public const uint Junk = 0x6A756E6B; // 'junk'
    public const uint Mdat = 0x6D646174; // 'mdat'
    public const uint Moov = 0x6D6F6F76; // 'moov'
    public const uint Pnot = 0x706E6F74; // 'pnot'
    public const uint Skip = 0x736B6970; // 'skip'
    public const uint Wide = 0x77696465; // 'wide'
    public const uint Pict = 0x50494354; // 'PICT'
    public const uint Ftyp = 0x66747970; // 'ftyp'
    public const uint Uuid = 0x75756964; // 'uuid'

    public const uint Cmov = 0x636D6F76; // 'cmov'
    public const uint Trak = 0x7472616B; // 'trak'
    public const uint Mdia = 0x6D646961; // 'mdia'
    public const uint Minf = 0x6D696E66; // 'minf'
    public const uint Stbl = 0x7374626C; // 'stbl'
    public const uint Stco = 0x7374636F; // 'stco'
    public const uint Co64 = 0x636F3634; // 'co64'
}

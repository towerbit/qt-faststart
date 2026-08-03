namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// 对应原 C 版本的 parse_atoms() + parse_atoms_callback_t
// 在给定的缓冲区区间内顺序解析 atom，并对每个 atom 调用 callback。
internal static class AtomParser
{
    private const int AtomPreambleSize = 8;

    public delegate void AtomCallback(byte[] buffer, in Atom atom);

    public static void ParseAtoms(byte[] buffer, int offset, long size, AtomCallback callback)
    {
        int pos = offset;
        long end = offset + size;

        while (end - pos >= AtomPreambleSize)
        {
            long atomSize = ByteOps.ReadBe32(buffer, pos);
            uint atomType = ByteOps.ReadBe32(buffer, pos + 4);
            pos += AtomPreambleSize;
            int headerSize = AtomPreambleSize;

            if (atomSize == 1)
            {
                if (end - pos < 8)
                {
                    throw new FastStartException("not enough room for 64 bit atom size");
                }

                atomSize = (long)ByteOps.ReadBe64(buffer, pos);
                pos += 8;
                headerSize = AtomPreambleSize + 8;
            }
            else if (atomSize == 0)
            {
                atomSize = AtomPreambleSize + (end - pos);
            }

            if (atomSize < headerSize)
            {
                throw new FastStartException($"atom size {atomSize} too small");
            }

            atomSize -= headerSize;

            if (atomSize > end - pos)
            {
                throw new FastStartException($"atom size {atomSize} too big");
            }

            var atom = new Atom(atomType, headerSize, atomSize, pos);
            callback(buffer, in atom);

            pos += (int)atomSize;
        }
    }
}

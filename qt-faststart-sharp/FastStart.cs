namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// 对应原 C 版本 main() 函数的主体逻辑：
//   1. 顺序扫描顶层 atom，确认最后一个 atom 是 moov
//   2. 读取整个 moov atom 到内存，检测是否为不支持的压缩 moov (cmov)
//   3. 调用 MoovUpdater 修正 chunk offset (并在必要时把 stco 升级为 co64)
//   4. 写出新文件: ftyp (若存在) + 新 moov + 原文件中 moov 之前的其余数据
internal static class FastStart
{
    private const int AtomPreambleSize = 8;
    private const long CopyBufferSize = 33554432; // 对应原 COPY_BUFFER_SIZE (32MB)
    private const long MaxFtypAtomSize = 1048576; // 对应原 MAX_FTYP_ATOM_SIZE (1MB)

    public static int Run(string inputPath, string outputPath)
    {
        byte[]? ftypAtom = null;
        long ftypAtomSize = 0;
        long startOffset = 0;

        uint atomType = 0;
        long atomSize = 0;
        long moovSize = 0;
        long freeSize = 0;
        ulong atomOffset = 0;

        using (var infile = OpenRead(inputPath))
        {
            var atomBytes = new byte[AtomPreambleSize];

            while (true)
            {
                if (ReadFully(infile, atomBytes, AtomPreambleSize) != AtomPreambleSize)
                {
                    break;
                }

                atomSize = ByteOps.ReadBe32(atomBytes, 0);
                atomType = ByteOps.ReadBe32(atomBytes, 4);

                if (atomType == AtomTypes.Ftyp)
                {
                    if (atomSize > MaxFtypAtomSize)
                    {
                        throw new FastStartException($"ftyp atom size {atomSize} too big");
                    }

                    ftypAtomSize = atomSize;
                    ftypAtom = new byte[ftypAtomSize];
                    infile.Seek(-AtomPreambleSize, SeekOrigin.Current);
                    if (ReadFully(infile, ftypAtom, (int)ftypAtomSize) != ftypAtomSize)
                    {
                        throw new FastStartException($"{inputPath}: unexpected end of file while reading ftyp atom");
                    }

                    startOffset = infile.Position;
                }
                else if (atomSize == 1)
                {
                    // 64 位扩展 size 特例
                    if (ReadFully(infile, atomBytes, AtomPreambleSize) != AtomPreambleSize)
                    {
                        break;
                    }

                    atomSize = (long)ByteOps.ReadBe64(atomBytes, 0);
                    infile.Seek(atomSize - AtomPreambleSize * 2, SeekOrigin.Current);
                }
                else
                {
                    infile.Seek(atomSize - AtomPreambleSize, SeekOrigin.Current);
                }

                Console.WriteLine($"{FourCcToString(atomType)} {atomOffset,10} {atomSize}");

                if (atomType != AtomTypes.Free &&
                    atomType != AtomTypes.Junk &&
                    atomType != AtomTypes.Mdat &&
                    atomType != AtomTypes.Moov &&
                    atomType != AtomTypes.Pnot &&
                    atomType != AtomTypes.Skip &&
                    atomType != AtomTypes.Wide &&
                    atomType != AtomTypes.Pict &&
                    atomType != AtomTypes.Uuid &&
                    atomType != AtomTypes.Ftyp)
                {
                    Console.Error.WriteLine("encountered non-QT top-level atom (is this a QuickTime file?)");
                    break;
                }

                atomOffset += (ulong)atomSize;

                // atom 头部至少 8 字节，若 atom_size 小于这个值，后续无法继续扫描
                if (atomSize < 8)
                {
                    break;
                }

                if (atomType == AtomTypes.Moov)
                {
                    moovSize = atomSize;
                }

                if (moovSize != 0 && atomType == AtomTypes.Free)
                {
                    freeSize += atomSize;
                    atomType = AtomTypes.Moov;
                    atomSize = moovSize;
                }
            }
        }

        if (atomType != AtomTypes.Moov)
        {
            Console.WriteLine("last atom in file was not a moov atom");
            return 0;
        }

        if (atomSize < 16)
        {
            throw new FastStartException("bad moov atom size");
        }

        if (atomSize > int.MaxValue)
        {
            throw new FastStartException($"moov atom size {atomSize} too big to load into memory");
        }

        byte[] moovAtom;
        long lastOffset;

        using (var infile = OpenRead(inputPath))
        {
            infile.Seek(-(atomSize + freeSize), SeekOrigin.End);
            lastOffset = infile.Position;

            moovAtom = new byte[atomSize];
            if (ReadFully(infile, moovAtom, (int)atomSize) != atomSize)
            {
                throw new FastStartException($"{inputPath}: unexpected end of file while reading moov atom");
            }
        }

        // 目前不支持压缩 moov atom (cmov)
        if (ByteOps.ReadBe32(moovAtom, 12) == AtomTypes.Cmov)
        {
            throw new FastStartException("this utility does not support compressed moov atoms yet");
        }

        moovAtom = MoovUpdater.UpdateMoovAtom(moovAtom);

        using var outfile = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        using (var infile = OpenRead(inputPath))
        {
            if (startOffset > 0)
            {
                // 跳过 ftyp atom
                infile.Seek(startOffset, SeekOrigin.Begin);
                lastOffset -= startOffset;
            }

            if (ftypAtomSize > 0)
            {
                Console.WriteLine(" writing ftyp atom...");
                outfile.Write(ftypAtom!, 0, (int)ftypAtomSize);
            }

            Console.WriteLine(" writing moov atom...");
            outfile.Write(moovAtom, 0, moovAtom.Length);

            Console.WriteLine(" copying rest of file...");
            CopyRemainder(infile, outfile, inputPath, lastOffset);
        }

        return 0;
    }

    private static void CopyRemainder(FileStream infile, FileStream outfile, string inputPath, long remaining)
    {
        int bufferSize = (int)Math.Min(CopyBufferSize, Math.Max(remaining, 0));
        if (bufferSize == 0)
        {
            return;
        }

        byte[] copyBuffer = new byte[bufferSize];

        while (remaining > 0)
        {
            int toCopy = (int)Math.Min(bufferSize, remaining);
            if (ReadFully(infile, copyBuffer, toCopy) != toCopy)
            {
                throw new FastStartException($"{inputPath}: unexpected end of file while copying data");
            }

            outfile.Write(copyBuffer, 0, toCopy);
            remaining -= toCopy;
        }
    }

    private static FileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    private static int ReadFully(Stream stream, byte[] buffer, int count)
    {
        int totalRead = 0;
        while (totalRead < count)
        {
            int read = stream.Read(buffer, totalRead, count - totalRead);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        return totalRead;
    }

    private static string FourCcToString(uint type) =>
        new(
        [
            (char)((type >> 24) & 0xFF),
            (char)((type >> 16) & 0xFF),
            (char)((type >> 8) & 0xFF),
            (char)(type & 0xFF),
        ]);
}

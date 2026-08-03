namespace QtFastStart;

// 移植自 FFmpeg tools/qt-faststart.c (原作者 Mike Melanson, public domain)。
// 详见 THIRD-PARTY-NOTICES.md。
//
// 对应原 C 版本中 update_moov_atom() 及其依赖的两遍处理逻辑：
//   第一遍: 递归遍历 moov，把所有 stco/co64 中的 chunk offset
//           都加上 moov atom 本身的大小 (因为 moov 被移到了文件开头)。
//           如果发现 stco (32 位偏移) 会溢出，则标记 overflow。
//   第二遍 (仅在溢出时执行): 重新构建整个 moov，把溢出的 stco 升级为 co64 (64 位偏移)。
internal static class MoovUpdater
{
    private sealed class UpdateChunkOffsetsContext
    {
        public ulong MoovAtomSize;
        public ulong StcoOffsetCount;
        public ulong StcoDataSize;
        public bool StcoOverflow;
        public int Depth;
    }

    private sealed class UpgradeStcoContext
    {
        public required byte[] Dest;
        public int DestPos;
        public required ulong OriginalMoovSize;
        public required ulong NewMoovSize;
    }

    // 输入 moovAtom 是完整的 moov atom 字节 (含头部)。
    // 返回值可能是原数组 (无溢出) 或新分配的、已升级为 co64 的数组。
    public static byte[] UpdateMoovAtom(byte[] moovAtom)
    {
        var context = new UpdateChunkOffsetsContext { MoovAtomSize = (ulong)moovAtom.Length };

        void UpdateChunkOffsetsCallback(byte[] buffer, in Atom atom)
        {
            switch (atom.Type)
            {
                case AtomTypes.Stco:
                    UpdateStcoOffsets(context, buffer, atom);
                    return;

                case AtomTypes.Co64:
                    UpdateCo64Offsets(context, buffer, atom);
                    return;

                case AtomTypes.Moov:
                case AtomTypes.Trak:
                case AtomTypes.Mdia:
                case AtomTypes.Minf:
                case AtomTypes.Stbl:
                    context.Depth++;
                    if (context.Depth > 10)
                    {
                        throw new FastStartException("atoms too deeply nested");
                    }

                    AtomParser.ParseAtoms(buffer, atom.DataOffset, atom.Size, UpdateChunkOffsetsCallback);
                    context.Depth--;
                    return;
            }
        }

        AtomParser.ParseAtoms(moovAtom, 0, moovAtom.Length, UpdateChunkOffsetsCallback);

        if (!context.StcoOverflow)
        {
            return moovAtom;
        }

        Console.WriteLine(" upgrading stco atoms to co64...");
        ulong newMoovSize = context.MoovAtomSize +
            context.StcoOffsetCount * 8 -
            context.StcoDataSize;

        byte[] newMoovAtom = new byte[newMoovSize];
        var upgradeContext = new UpgradeStcoContext
        {
            Dest = newMoovAtom,
            DestPos = 0,
            OriginalMoovSize = context.MoovAtomSize,
            NewMoovSize = newMoovSize,
        };

        void UpgradeStcoCallback(byte[] buffer, in Atom atom)
        {
            switch (atom.Type)
            {
                case AtomTypes.Stco:
                    UpgradeStcoAtom(upgradeContext, buffer, atom);
                    break;

                case AtomTypes.Moov:
                case AtomTypes.Trak:
                case AtomTypes.Mdia:
                case AtomTypes.Minf:
                case AtomTypes.Stbl:
                    // 先写入 atom 头部
                    Buffer.BlockCopy(buffer, atom.HeaderOffset, upgradeContext.Dest, upgradeContext.DestPos, atom.HeaderSize);
                    int startPos = upgradeContext.DestPos;
                    upgradeContext.DestPos += atom.HeaderSize;

                    // 递归处理内部 atom
                    AtomParser.ParseAtoms(buffer, atom.DataOffset, atom.Size, UpgradeStcoCallback);

                    // 回填该 atom 的实际大小
                    SetAtomSize(upgradeContext.Dest, startPos, atom.HeaderSize, (ulong)(upgradeContext.DestPos - startPos));
                    break;

                default:
                    long copySize = atom.HeaderSize + atom.Size;
                    Buffer.BlockCopy(buffer, atom.HeaderOffset, upgradeContext.Dest, upgradeContext.DestPos, (int)copySize);
                    upgradeContext.DestPos += (int)copySize;
                    break;
            }
        }

        AtomParser.ParseAtoms(moovAtom, 0, moovAtom.Length, UpgradeStcoCallback);

        if (upgradeContext.DestPos != newMoovAtom.Length)
        {
            throw new FastStartException("unexpected - wrong number of moov bytes written");
        }

        return newMoovAtom;
    }

    private static void UpdateStcoOffsets(UpdateChunkOffsetsContext context, byte[] buffer, in Atom atom)
    {
        Console.WriteLine(" patching stco atom...");
        if (atom.Size < 8)
        {
            throw new FastStartException($"stco atom size {atom.Size} too small");
        }

        uint offsetCount = ByteOps.ReadBe32(buffer, atom.DataOffset + 4);
        if (offsetCount > (ulong)(atom.Size - 8) / 4)
        {
            throw new FastStartException($"stco offset count {offsetCount} too big");
        }

        context.StcoOffsetCount += offsetCount;
        context.StcoDataSize += (ulong)(atom.Size - 8);

        int pos = atom.DataOffset + 8;
        int end = pos + (int)(offsetCount * 4);
        for (; pos < end; pos += 4)
        {
            uint currentOffset = ByteOps.ReadBe32(buffer, pos);
            if (currentOffset > uint.MaxValue - context.MoovAtomSize)
            {
                context.StcoOverflow = true;
            }

            currentOffset += (uint)context.MoovAtomSize;
            ByteOps.WriteBe32(buffer, pos, currentOffset);
        }
    }

    private static void UpdateCo64Offsets(UpdateChunkOffsetsContext context, byte[] buffer, in Atom atom)
    {
        Console.WriteLine(" patching co64 atom...");
        if (atom.Size < 8)
        {
            throw new FastStartException($"co64 atom size {atom.Size} too small");
        }

        uint offsetCount = ByteOps.ReadBe32(buffer, atom.DataOffset + 4);
        if (offsetCount > (ulong)(atom.Size - 8) / 8)
        {
            throw new FastStartException($"co64 offset count {offsetCount} too big");
        }

        int pos = atom.DataOffset + 8;
        int end = pos + (int)(offsetCount * 8);
        for (; pos < end; pos += 8)
        {
            ulong currentOffset = ByteOps.ReadBe64(buffer, pos);
            currentOffset += context.MoovAtomSize;
            ByteOps.WriteBe64(buffer, pos, currentOffset);
        }
    }

    private static void UpgradeStcoAtom(UpgradeStcoContext context, byte[] buffer, in Atom atom)
    {
        // 注意: 不再重复校验，第一遍已经校验过了
        uint offsetCount = ByteOps.ReadBe32(buffer, atom.DataOffset + 4);

        // 写入头部 (沿用原头部字节 + 前 8 字节数据，即 version/flags + offset_count)
        Buffer.BlockCopy(buffer, atom.HeaderOffset, context.Dest, context.DestPos, atom.HeaderSize + 8);
        ByteOps.WriteBe32(context.Dest, context.DestPos + 4, AtomTypes.Co64);
        SetAtomSize(context.Dest, context.DestPos, atom.HeaderSize, (ulong)(atom.HeaderSize + 8 + offsetCount * 8));
        context.DestPos += atom.HeaderSize + 8;

        // 写入数据: 将每个 32 位 offset 还原、再按新 moov 大小重新计算为 64 位 offset
        int pos = atom.DataOffset + 8;
        int end = pos + (int)(offsetCount * 4);
        for (; pos < end; pos += 4)
        {
            ulong rawDiff = unchecked((ulong)ByteOps.ReadBe32(buffer, pos) - context.OriginalMoovSize);
            uint originalOffset = unchecked((uint)rawDiff);
            ulong newOffset = (ulong)originalOffset + context.NewMoovSize;
            ByteOps.WriteBe64(context.Dest, context.DestPos, newOffset);
            context.DestPos += 8;
        }
    }

    private static void SetAtomSize(byte[] dest, int headerOffset, int headerSize, ulong size)
    {
        switch (headerSize)
        {
            case 8:
                ByteOps.WriteBe32(dest, headerOffset, (uint)size);
                break;

            case 16:
                ByteOps.WriteBe64(dest, headerOffset + 8, size);
                break;
        }
    }
}

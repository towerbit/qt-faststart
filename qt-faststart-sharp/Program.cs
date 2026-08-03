// qt-faststart (C# / .NET 10 port)
//
// 移植自 FFmpeg 的 tools/qt-faststart.c，原作者 Mike Melanson
// (melanson@pcisys.net)，原文件已声明置于公共领域 (public domain)。
// 本移植版权归本仓库贡献者所有，遵循仓库根目录 LICENSE (WTFPL)；
// 上游来源与许可证详情见 THIRD-PARTY-NOTICES.md。
//
// 功能: 将 MP4/QuickTime 文件的 moov atom 从文件尾部移动到文件头部，
// 便于网络流式播放 (等价于 ffmpeg 的 -movflags +faststart)。
//
// 用法: qt-faststart <infile.mp4> <outfile.mp4>

using System;
using System.IO;

namespace QtFastStart;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: qt-faststart <infile.mov> <outfile.mov>");
            Console.WriteLine("Note: alternatively you can use -movflags +faststart in ffmpeg");
            return 0;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        if (string.Equals(inputPath, outputPath, StringComparison.Ordinal))
        {
            Console.Error.WriteLine("input and output files need to be different");
            return 1;
        }

        try
        {
            return FastStart.Run(inputPath, outputPath);
        }
        catch (FastStartException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (IOException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}

// 用于携带"类似 perror"的错误信息（已经附带了文件路径上下文）。
internal sealed class FastStartException(string message) : Exception(message);

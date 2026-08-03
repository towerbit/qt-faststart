# qt-faststart

对 FFmpeg 附带的 `qt-faststart` 工具的两种独立实现，用于将 MP4/QuickTime 文件的
`moov` atom 从文件尾部移动到文件头部，使文件支持边下载边播放（等价于
`ffmpeg -movflags +faststart`）。

本仓库包含两个子项目：

| 子项目 | 语言 | 说明 |
| --- | --- | --- |
| [`qt-faststart-c`](qt-faststart-c) | C | 直接编译 FFmpeg 上游 `qt-faststart.c`，支持 CMake / MSVC |
| [`qt-faststart-sharp`](qt-faststart-sharp) | C# (.NET 10) | 移植版实现，支持 Native AOT 编译为单文件可执行程序 |

两者功能一致，用法相同

选择建议：只需要一个静态可执行文件、不想依赖 .NET 运行时，选 `qt-faststart-c`；
偏好 C#/.NET 生态或需要跨平台维护同一份可读性更高的代码，选 `qt-faststart-sharp`
（发布时用 Native AOT 编译，同样不依赖运行时）。

## 许可证与版权

本仓库自身代码（构建脚本、C# 移植代码、文档等）遵循根目录 [`LICENSE`](LICENSE)
（WTFPL）。

两个子项目均衍生自 FFmpeg 项目的 `tools/qt-faststart.c`，该文件由原作者
Mike Melanson 声明置于公共领域 (public domain)：

- `qt-faststart-c/qt-faststart.c` 是该上游文件的原样拷贝，文件头部的原始
  版权声明保持不变。
- `qt-faststart-sharp` 是对该文件的移植 (port)，各源文件头部注释均标注了
  来源。

完整的第三方版权说明见 [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md)。

---

## qt-faststart-c

FFmpeg `qt-faststart.c` 的独立 C 项目封装，支持 Windows 和 Linux。

### 文件

- `qt-faststart.c`：FFmpeg 上游源文件
- `CMakeLists.txt`：跨平台构建入口
- `CMakePresets.json`：可选的预设构建配置
- `qt-faststart.sln` / `qt-faststart.vcxproj`：MSVC 构建用的 Visual Studio 项目
- `docs/MSVC.md`：详细的 MSVC 构建说明
- `.github/workflows/build.yml`：Windows / Linux 的 GitHub Actions 构建流程

### 依赖要求

- CMake 3.20 及以上
- C 编译器
  - Windows：MSVC 或 MinGW-w64
  - Linux：GCC 或 Clang
- 如需使用内置 CMake 预设，还需要 Ninja

### Windows 构建

**方式一：MSVC + Visual Studio**

在 Visual Studio 2022 或更高版本中打开 `qt-faststart.sln`，选择 `Release`
和 `x64`，构建解决方案即可。详细的 IDE 和命令行步骤见 `docs/MSVC.md`。

命令行构建（MSBuild）：

```powershell
msbuild .\qt-faststart.sln /p:Configuration=Release /p:Platform=x64
```

输出路径：

```text
out\x64\Release\qt-faststart.exe
```

**方式二：CMake**

预设构建：

```powershell
cmake --preset windows-release
cmake --build --preset windows-release
```

直接构建：

```powershell
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
```

输出路径：

```text
build/windows-release/qt-faststart.exe
```

### Linux 构建

预设构建：

```bash
cmake --preset linux-release
cmake --build --preset linux-release
```

直接构建：

```bash
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build
```

输出路径：

```text
build/linux-release/qt-faststart
```

### 用法

```bash
qt-faststart input.mp4 output.mp4
```

```powershell
.\build\windows-release\qt-faststart.exe input.mp4 output.mp4
```

```bash
./build/linux-release/qt-faststart input.mp4 output.mp4
```

### CI

内置的 GitHub Actions 工作流会在 Windows 和 Linux 上分别构建
`qt-faststart.exe` 和 `qt-faststart`，并自动上传构建产物。

---

## qt-faststart-sharp

移植自 FFmpeg `tools/qt-faststart.c`（公共领域授权）的 C#/.NET 10 实现，
功能与 C 版本一致。发布时启用 `PublishAot`，可编译为不依赖 .NET 运行时的
原生可执行文件。

### 文件

- `Program.cs`：程序入口，处理命令行参数
- `FastStart.cs`：核心逻辑，对应原 C 版本 `main()` 的主体流程
- `AtomParser.cs` / `Atom.cs` / `AtomTypes.cs`：atom 解析与类型定义
- `MoovUpdater.cs`：修正 `moov` atom 中的 chunk offset（必要时将 `stco`
  升级为 `co64`）
- `ByteOps.cs`：大端字节读写辅助方法
- `qt-faststart-sharp.csproj` / `qt-faststart-sharp.slnx`：项目与解决方案文件
- `.github/workflows/build-aot.yml`：Windows / Linux 的 Native AOT 构建流程

### 依赖要求

- .NET 10 SDK

### 构建与运行

普通构建/运行（依赖 .NET 运行时）：

```bash
dotnet run --project qt-faststart-sharp -- input.mp4 output.mp4
```

发布为 Native AOT 单文件可执行程序（不依赖运行时）：

```bash
dotnet publish -r win-x64 -c Release   # Windows
dotnet publish -r linux-x64 -c Release # Linux
```

输出路径：

```text
bin/Release/net10.0/<rid>/publish/qt-faststart-sharp.exe   # Windows
bin/Release/net10.0/<rid>/publish/qt-faststart-sharp       # Linux
```

### 用法

```text
qt-faststart-sharp <infile.mov> <outfile.mov>
```

输入输出文件不能相同，否则会报错。也可以直接用 ffmpeg 的
`-movflags +faststart` 替代本工具。

### CI

内置的 GitHub Actions 工作流（`build-aot.yml`）会在 Windows (`win-x64`) 和
Linux (`linux-x64`) 上分别执行 `dotnet publish` 生成 Native AOT 可执行文件，
并自动上传构建产物。

### 版权与许可证

本项目是对 FFmpeg `tools/qt-faststart.c`（原作者 Mike Melanson，已声明置于
公共领域）的 C# 移植，各源文件头部注释均标注了对应关系。移植代码本身遵循
仓库根目录 `LICENSE`（WTFPL）。详见仓库根目录 `THIRD-PARTY-NOTICES.md`。

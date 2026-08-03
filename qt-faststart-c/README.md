# qt-faststart-c

Standalone C project for building FFmpeg's `qt-faststart.c` as a command-line
tool on Windows and Linux.

## Files

- `qt-faststart.c`: upstream source file from FFmpeg
- `CMakeLists.txt`: cross-platform build entrypoint
- `CMakePresets.json`: optional preset-based builds
- `qt-faststart.sln`: Visual Studio solution for MSVC builds
- `qt-faststart.vcxproj`: Visual Studio C project for MSVC
- `docs/MSVC.md`: detailed MSVC build guide
- `.github/workflows/build.yml`: GitHub Actions build for Windows and Linux

## Requirements

- CMake 3.20 or newer
- A C compiler
  - Windows: MSVC or MinGW-w64
  - Linux: GCC or Clang
- Ninja if you want to use the included CMake presets

## Build on Windows

### MSVC with Visual Studio

Open `qt-faststart.sln` in Visual Studio 2022 or newer, select `Release` and
`x64`, then build the solution.

For detailed IDE and command-line steps, see `docs/MSVC.md`.

Command-line build with MSBuild:

```powershell
msbuild .\qt-faststart.sln /p:Configuration=Release /p:Platform=x64
```

Typical output path:

```text
out\x64\Release\qt-faststart.exe
```

### CMake on Windows

Preset-based build:

```powershell
cmake --preset windows-release
cmake --build --preset windows-release
```

Direct CMake build:

```powershell
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build --config Release
```

Typical output path:

```text
build/windows-release/qt-faststart.exe
```

## Build on Linux

Preset-based build:

```bash
cmake --preset linux-release
cmake --build --preset linux-release
```

Direct CMake build:

```bash
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release
cmake --build build
```

Typical output path:

```text
build/linux-release/qt-faststart
```

## Usage

```bash
qt-faststart input.mp4 output.mp4
```

Windows example:

```powershell
.\build\windows-release\qt-faststart.exe input.mp4 output.mp4
```

Linux example:

```bash
./build/linux-release/qt-faststart input.mp4 output.mp4
```

## CI

The included GitHub Actions workflow builds:

- `qt-faststart.exe` on Windows
- `qt-faststart` on Linux

Build artifacts are uploaded automatically for each workflow run.

## License and copyright

`qt-faststart.c` is an unmodified copy of FFmpeg's upstream
`tools/qt-faststart.c`. Its author, Mike Melanson, has placed the file in
the public domain; the original copyright notice in the file header is
kept as-is. The build scripts added around it in this project
(`CMakeLists.txt`, `CMakePresets.json`, the `.sln`/`.vcxproj` files, etc.)
are licensed under the repository root `LICENSE` (WTFPL). See the
repository root `THIRD-PARTY-NOTICES.md` for details.

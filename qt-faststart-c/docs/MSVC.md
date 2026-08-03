# Build With MSVC

This project includes native Visual Studio project files so you can build
`qt-faststart.exe` with MSVC without generating anything first.

## Supported setup

- Visual Studio 2022 recommended
- Desktop development with C++ workload
- MSVC toolset
- Windows SDK

If Visual Studio asks to retarget the project when you open the solution,
accept the retarget prompt.

## Build in the Visual Studio IDE

1. Open `qt-faststart.sln`.
2. Select `Release` or `Debug`.
3. Select `x64` or `Win32`.
4. Build the solution with `Build > Build Solution`.

Output files are written to:

```text
out\<Platform>\<Configuration>\
```

Examples:

```text
out\x64\Release\qt-faststart.exe
out\Win32\Debug\qt-faststart.exe
```

## Build from the command line

Open one of these shells:

- `x64 Native Tools Command Prompt for VS 2022`
- `Developer PowerShell for VS 2022`

Then run:

```powershell
msbuild .\qt-faststart.sln /p:Configuration=Release /p:Platform=x64
```

Other examples:

```powershell
msbuild .\qt-faststart.sln /p:Configuration=Debug /p:Platform=x64
msbuild .\qt-faststart.sln /p:Configuration=Release /p:Platform=Win32
```

## Run the program

```powershell
.\out\x64\Release\qt-faststart.exe input.mp4 output.mp4
```

## Notes

- The project builds `qt-faststart.c` directly as a C source file.
- `_CRT_SECURE_NO_WARNINGS` and `_CRT_NONSTDC_NO_WARNINGS` are already set in
  the project configuration for MSVC compatibility.
- No external libraries are required.

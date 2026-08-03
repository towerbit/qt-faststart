cmake_minimum_required(VERSION 3.20)

project(qt-faststart
    VERSION 0.2
    DESCRIPTION "Cross-platform build for FFmpeg's qt-faststart utility"
    LANGUAGES C)

set(CMAKE_C_STANDARD 99)
set(CMAKE_C_STANDARD_REQUIRED ON)
set(CMAKE_C_EXTENSIONS OFF)

add_executable(qt-faststart qt-faststart.c)

if(MSVC)
    target_compile_definitions(qt-faststart PRIVATE _CRT_SECURE_NO_WARNINGS)
    target_compile_options(qt-faststart PRIVATE /W4)
else()
    target_compile_options(qt-faststart PRIVATE -Wall -Wextra -Wpedantic)
endif()

if(WIN32)
    target_compile_definitions(qt-faststart PRIVATE _CRT_NONSTDC_NO_WARNINGS)
else()
    target_compile_definitions(qt-faststart PRIVATE _FILE_OFFSET_BITS=64 _LARGEFILE_SOURCE)
    # Statically link glibc so the binary carries no runtime dependency on the
    # host's libc version, allowing it to run on older distros (e.g. Ubuntu
    # 22, openEuler 22) than the one it was built on.
    target_link_options(qt-faststart PRIVATE -static)
endif()

install(TARGETS qt-faststart RUNTIME DESTINATION bin)

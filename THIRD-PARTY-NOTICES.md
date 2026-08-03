# 第三方版权声明

本仓库的两个子项目均衍生自 FFmpeg 项目提供的 `tools/qt-faststart.c` 工具。
现将该文件的原始版权和许可证信息记录如下。

## qt-faststart.c (FFmpeg)

- 原作者：Mike Melanson (<melanson@pcisys.net>)
- 来源：FFmpeg 项目 `tools/qt-faststart.c`
- 许可证：作者已将该文件置于公共领域 (public domain)，允许以任何方式使用
  该程序（"This file is placed in the public domain. Use the program
  however you see fit."）
- 上游项目主页：<https://ffmpeg.org/>
- 上游仓库：<https://github.com/FFmpeg/FFmpeg>

原始版权声明摘录（完整文本见该文件头部注释）：

```text
qt-faststart.c, v0.2
by Mike Melanson (melanson@pcisys.net)
This file is placed in the public domain. Use the program however you
see fit.
```

## 各子项目中的处理方式

- **qt-faststart-c**：直接复用上游 `qt-faststart.c` 源码文件，文件头部的原始
  public domain 声明保持不变、未做任何修改。围绕该文件新增的构建脚本
  （`CMakeLists.txt`、`CMakePresets.json`、`.vcxproj` 等）由本仓库编写，
  遵循仓库根目录 `LICENSE`（WTFPL）。

- **qt-faststart-sharp**：是对上述 public domain 文件逐步移植 (port) 到
  C#/.NET 的衍生实现，逻辑对应关系已在各源文件头部注释中标注。移植代码本身
  由本仓库编写，遵循仓库根目录 `LICENSE`（WTFPL），但移植行为不改变原始
  算法/逻辑出自 FFmpeg `qt-faststart.c` 这一事实，故在此一并声明来源。

## 关于公共领域声明的说明

"Public domain"（公共领域）声明在美国法律体系下具有明确效力，但在部分司法
辖区（包括中国大陆）法律效力可能存在争议或不被直接承认。若在此类司法辖区
使用或分发本仓库代码，建议将上游文件视为"作者已明确放弃可主张的版权限制，
并允许自由使用"的等效声明对待；如需更保守的合规路径，可自行改用符合当地
法律的等效免费许可证条款。

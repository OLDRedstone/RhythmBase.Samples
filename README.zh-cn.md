# RhythmBase.Samples
基于 RhythmBase 的实用脚本集合。

[English](README.md) | **中文**

## 简介

本仓库用于收集和分享一些常用或可能实用的 RhythmBase 脚本，方便直接运行、学习参考或二次开发。

## 已收录脚本

- #### **autoplay.cs** by *obugs*
  从关卡文件中获取按拍时机，在播放关卡时模拟键盘操作自动按拍。
  > 不保证覆盖率，使用者自行承担使用后果。

- #### **counter.cs** by *obugs*
  统计关卡内事件数量、玩法信息等。

- #### **remove_vfx.cs** by *obugs*
  移除所有不影响游玩体验的事件。可用于减小游玩关卡时游戏对设备性能的需求。

- #### **check_font_glyphs.cs** by *obugs*
  检查字体文件是否能覆盖指定字符集合，用于筛选字体，避免缺字情况。

## 环境要求

- 推荐使用 .NET SDK：10.0.300+ 或 11.0.0+，以支持脚本文件引用等功能。

可通过以下命令检查当前 .NET SDK 版本：

```bash
dotnet --version
```

如果版本不符合要求，请先升级或安装对应版本的 .NET SDK。

## 使用方法

1. 克隆本仓库：

```bash
git clone <仓库地址>
cd RhythmBase.Samples
```

2. 运行脚本：

```bash
dotnet <script.cs>
```

将 `<script.cs>` 替换为实际要运行的脚本名称或项目路径。例如：

```bash
dotnet scripts/MyScript.cs
```

大多数脚本都可以直接通过上述方式运行。部分脚本可能需要额外参数或配置，具体请以对应脚本的说明为准。

由于 RhythmBase 积极迭代中，部分功能可能仅在最新版本中才能使用。\
执行之前，可以通过运行根目录下的脚本更改脚本依赖的 RhythmBase 版本。可以为脚本切换最新正式版、测试版，或者指定版本。
```bash
# 切换为最新正式版
dotnet .nuget_update_package.cs

# 切换为最新测试版
dotnet .nuget_update_package.cs -- -p
dotnet .nuget_update_package.cs -- --prerelease

# 将指定脚本切换为指定版本
dotnet .nuget_update_package.cs -- -t scripts/MyScript.cs -v 1.3.11
dotnet .nuget_update_package.cs -- --target scripts/MyScript.cs --version 1.3.11
```

> 提示：`--` 之后的参数会传递给脚本本身，而不是 `dotnet` 命令。

## 贡献

欢迎向本仓库贡献你自己的脚本，也欢迎对现有脚本进行修改、优化并提交。

你可以：

- 新增自己的脚本；
- 修复现有脚本的问题；
- 改进脚本功能或文档说明。

提交前请尽量确保脚本可以正常运行，并在 Pull Request 中简要说明脚本用途、使用方法和主要改动。

## 反馈

如果你在使用过程中遇到问题，或有新的想法和建议，欢迎提交 Issue 或 Pull Request。

## 致谢

感谢 RhythmBase 项目以及所有为本仓库做出贡献的开发者。
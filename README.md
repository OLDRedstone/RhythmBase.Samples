RhythmBase.Samples
A collection of practical scripts based on RhythmBase.

**English** | [中文](README.zh-cn.md)

## Introduction
This repository collects and shares commonly used or potentially useful RhythmBase scripts, making it easy to run them directly, learn from them, or build on them.

## Included Scripts
- #### **autoplay.cs** by *obugs*
  > [!WARNING]
  > Due to the potential for misuse, this feature is not shown for now.
  >
  Retrieves beat timings from a level file and simulates keyboard input to automatically press keys on the beat while playing the level.

- #### **counter.cs** by *obugs*
  Counts the number of events, gameplay information, etc. in a level.

- #### **remove_vfx.cs** by *obugs*
  Removes all events that do not affect the gameplay experience. Can be used to reduce the performance requirements on the device when playing a level.

## Requirements

- Recommended .NET SDK: 10.0.300+ or 11.0.0+, to support features such as script file references.

You can check the current .NET SDK version with:

```bash
dotnet --version
```

If the version does not meet the requirement, upgrade or install the corresponding .NET SDK first.

## Usage
1. Clone this repository:

```bash
git clone <repository-url>
cd RhythmBase.Samples
```

2. Run a script:

```bash
dotnet <script.cs>
```

Replace <script.cs> with the actual script name or project path. For example:

```bash
dotnet scripts/MyScript.cs
```

Most scripts can be run directly as shown above. Some scripts may require additional arguments or configuration; refer to the documentation for each script.

Because RhythmBase is under active development, some features may only be available in the latest version.\
Before running, you can change the RhythmBase version that a script depends on by running the script in the repository root. You can switch a script to the latest stable version, the latest prerelease version, or a specific version.

```bash
# Switch to the latest stable version
dotnet .nuget_update_package.cs

# Switch to the latest prerelease version
dotnet .nuget_update_package.cs -- -p
dotnet .nuget_update_package.cs -- --prerelease

# Switch the specified script to the specified version
dotnet .nuget_update_package.cs -- -t scripts/MyScript.cs -v 1.3.11
dotnet .nuget_update_package.cs -- --target scripts/MyScript.cs --version 1.3.11
```

> Tip: Arguments after `--` are passed to the script itself, not to the `dotnet` command.

## Contributing
You are welcome to contribute your own scripts, and to modify, improve, and submit changes to existing scripts.

You can:

- Add your own scripts;
- Fix issues in existing scripts;
- Improve script functionality or documentation.

Before submitting, please make sure the script runs correctly, and briefly describe the script's purpose, usage, and main changes in the Pull Request.

## Feedback
If you encounter any problems while using this repository, or have new ideas and suggestions, feel free to submit an Issue or Pull Request.

## Acknowledgments
Thanks to the RhythmBase project and all developers who have contributed to this repository.
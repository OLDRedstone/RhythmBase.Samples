/*
自动游玩脚本（目前可能还有些问题）
Autoplay script (may still have some issues)
*/

#:package RhythmBase.RhythmDoctor@1.3.11-alpha9-snapshot.20260920091616
#:package Spectre.Console@0.49.1
#:package SharpHook@8.0.0

using System.Diagnostics;
using RhythmBase.Global;
using RhythmBase.Global.Extensions;
using RhythmBase.Global.Serialization;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;
using SharpHook;
using SharpHook.Data;
using SharpHook.Simulation;
using Spectre.Console;

// 预设脚本常量
const float Threshold = 0.05f;
const float Offset = 0.760f;
const KeyCode Key = KeyCode.VcD;
string DirectoryPath = AnsiConsole.Ask<string>("请输入关卡目录路径：");

Config.CachePath = "temp";
Config.ClearCache();
LevelReadConfig rs = new()
{
	ZipProcessingMode = ZipProcessingMode.RootEntriesOnly,
	Strictness = JsonStrictness.Fallback,
};

string extension = Path.GetExtension(DirectoryPath);
using Level level = extension switch
{
	".zip" or ".rdzip" => Level.FromZip(DirectoryPath, rs),
	_ => Level.FromDirectory(DirectoryPath, rs),
};
List<Hit> hits = [];
foreach (BaseBeat b in level.MainChart.OfEvent<BaseBeat>())
{
	Hit[] beatHits;
	switch (b)
	{
		case AddClassicBeat acb:
			beatHits = [acb.Hit];
			break;
		case AddOneshotBeat aob:
			beatHits = aob.Hits;
			break;
		case AddFreeTimeBeat afb:
			if (afb.IsHittable)
				beatHits = [afb.Hit];
			beatHits = [];
			break;
		case PulseFreeTimeBeat pfb:
			if (pfb.IsHittable)
				beatHits = [pfb.Hit];
			beatHits = [];
			break;
		default:
			continue;
	}
	if (beatHits.Any(i => i.TickTime < i.Parent.TickTime))
		Console.WriteLine(b);
	hits.AddRange(beatHits);
}

hits.Sort((a, b) => a.TickTime.CompareTo(b.TickTime));
List<(TimeSpan Start, TimeSpan Duration)> press = [];

Hit hit = hits[0];
for (int i = 1; i < hits.Count; i++)
{
	var h1 = hits[i];
	if (hit.Merge(h1, out var merged))
		hit = merged;
	else
	{
		press.Add((hit.TickTime.TimeSpan, (hit.TickTime + hit.Hold).TimeSpan - hit.TickTime.TimeSpan));
		hit = h1;
	}
}

bool _isRunning = false;
object _lock = new();

AnsiConsole.MarkupLine("[green]脚本已启动，等待空格键开始...[/]");
using var hook = new EventLoopGlobalHook();
using var simulator = EventSimulator.Create("GameBot");

hook.KeyPressed += (sender, e) =>
{
	if (e.Data.KeyCode == KeyCode.VcSpace && !_isRunning)
	{
		lock (_lock)
		{
			if (_isRunning) return;
			_isRunning = true;
		}
		AnsiConsole.MarkupLine("[yellow]检测到空格键，开始执行脚本...[/]");
		Task.Run(() => RunScript(hook));
	}
};
hook.Run();

void RunScript(IGlobalHook hook)
{
	try
	{
		var stopwatch = Stopwatch.StartNew();
		var offsetSpan = TimeSpan.FromSeconds(Offset);

		foreach (var (start, duration) in press)
		{
			var targetTime = start + offsetSpan;
			var actualDuration = duration + TimeSpan.FromSeconds(Threshold);
			if (actualDuration < TimeSpan.Zero) actualDuration = TimeSpan.Zero;
			WaitUntil(stopwatch, targetTime);
			simulator.SimulateKeyPress(Key);
			var pressEnd = stopwatch.Elapsed + actualDuration;
			WaitUntil(stopwatch, pressEnd);
			simulator.SimulateKeyRelease(Key);
		}

		AnsiConsole.MarkupLine("[green]脚本执行完毕。[/]");
	}
	catch (Exception ex)
	{
		AnsiConsole.MarkupLine($"[red]脚本执行出错: {ex.Message}[/]");
	}
	finally
	{
		lock (_lock)
			_isRunning = false;
		AnsiConsole.MarkupLine("[grey]按空格键可再次开始。[/]");
	}
}
static void WaitUntil(Stopwatch stopwatch, TimeSpan target)
{
	while (stopwatch.Elapsed < target)
	{
		var remaining = target - stopwatch.Elapsed;
		if (remaining > TimeSpan.FromMilliseconds(1))
			Thread.Sleep(1);
		else
			Thread.SpinWait(100);
	}
}

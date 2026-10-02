#:package RhythmBase.RhythmDoctor@1.3.11-alpha9-snapshot.20260920091616
#:package Dumpify@0.7.0


using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Components.Conditions;
using RhythmBase.RhythmDoctor.Extensions;
using Dumpify;
using RhythmBase.RhythmDoctor;
using RhythmBase.Global.Serialization;
using System.Text.RegularExpressions;
using RhythmBase.Global.Extensions;
using System.Runtime;
using RhythmBase.Global;

Config.CachePath = "temp";
Config.ClearCache();

RhythmBase.Global.Serialization.LevelReadConfig rs = new()
{
	ZipProcessingMode = RhythmBase.Global.Serialization.ZipProcessingMode.RootEntriesOnly,
	Strictness = JsonStrictness.Fallback,
};

RhythmBase.Global.Serialization.LevelWriteConfig ws = new()
{
	LoadAssets = true,
	WriteIndented = false,
	WriteAligned = false,
	EnableUnsafeRelaxedJsonEscaping = false,
};

Chart chart = Chart.FromFile(@"D:\CSharp\Teto\Teto\bin\Debug\net8.0\Level\out.rdlevel");

List<Decoration> decos = [..chart.Decorations];
foreach (var deco in decos)
{
  chart.Decorations.Remove(deco);
}

chart.SaveToFile(@"D:\CSharp\Teto\Teto\bin\Debug\net8.0\Level\out2.rdlevel");
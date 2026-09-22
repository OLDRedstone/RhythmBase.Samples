/*
移除所有不影响游玩的事件
Remove all events that do not affect gameplay
*/

#:package RhythmBase.RhythmDoctor@1.3.11-alpha9-snapshot.20260920091616
#:package Spectre.Console@0.49.1

using System.Diagnostics;
using RhythmBase.Global;
using RhythmBase.Global.Components;
using RhythmBase.Global.Extensions;
using RhythmBase.Global.Serialization;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;
using Spectre.Console;


string DirectoryPath = AnsiConsole.Ask<string>("请输入关卡目录路径：");

Config.CachePath = "temp";
Config.ClearCache();
LevelReadConfig rs = new()
{
	ZipProcessingMode = ZipProcessingMode.RootEntriesOnly,
	Strictness = JsonStrictness.Fallback,
};


using Level level = Level.FromDirectory(DirectoryPath, rs);

ReadOnlyEnumCollection<EventType> eventTypes = [
		EventType.AddClassicBeat,
		EventType.AddOneshotBeat,
		EventType.AddFreeTimeBeat,
		EventType.PulseFreeTimeBeat,
		EventType.SetRowXs,
		EventType.CallCustomMethod,
		EventType.SetPlayStyle,
		EventType.Comment,
		EventType.ChangePlayersRows,
		EventType.FinishLevel,
		EventType.ForwardDecorationEvent,
		EventType.ForwardEvent,
		EventType.ForwardRowEvent,
		EventType.GoToLevel,
		EventType.LinkRows,
		EventType.PlaySong,
		EventType.PlaySound,
		EventType.SayReadyGetSetGo,
		EventType.SetBeatSound,
		EventType.SetBeatsPerMinute,
		EventType.SetClapSounds,
		EventType.SetCountingSound,
		EventType.SetCrotchetsPerBar,
		EventType.SetGameSound,
		EventType.SetSpeed,
		EventType.ShowDialogue,
];

List<IBaseEvent> eventsToRemove = [];
foreach (IBaseEvent e in level.MainChart)
	if (!eventTypes.Contains(e.Type))
		eventsToRemove.Add(e);
foreach (IBaseEvent e in eventsToRemove)
	level.MainChart.Remove(e);

level.MainChart.SaveToFile(Path.Combine(DirectoryPath, "without_vfx.rdlevel"), new LevelWriteConfig
{
	WriteIndented = true,
	WriteAligned = true,
});

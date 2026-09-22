/*
对关卡进行统计
To perform statistics on the level
*/

#:package RhythmBase.RhythmDoctor@1.3.11-alpha9-snapshot.20260920091616
#:package Spectre.Console@0.49.1
#:package Spectre.Console.Cli@0.49.1

using System.ComponentModel;
using RhythmBase.Global;
using RhythmBase.Global.Extensions;
using RhythmBase.Global.Serialization;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Events;
using RhythmBase.RhythmDoctor.Extensions;
using Spectre.Console;
using Spectre.Console.Cli;

Config.CachePath = "temp";
Config.ClearCache();

var app = new CommandApp<StatsCommand>();
app.Configure(cfg =>
{
	cfg.SetApplicationName("rhythm-stats");
	cfg.ValidateExamples();
});
return app.Run(args);

record class ChartData()
{
	public required string ChartName { get; init; }
	public required int TotalEvents { get; init; }
	public required int TotalDecorations { get; init; }
	public required int TotalHits { get; init; }
	public required int TotalPresses { get; init; }
	public required TimeSpan MinGap { get; init; }
}

enum ViewKind
{
	Summary,
	Chart,
	Exit,
}
enum ChartScale
{
	Linear,
	Log,
}

public sealed class StatsCommand : Command<StatsCommand.Settings>
{
	public sealed class Settings : CommandSettings
	{
		[CommandArgument(0, "<path>")]
		[Description("数据文件路径")]
		public string Path { get; init; } = "";

		[CommandOption("-t|--top <N>")]
		[Description("详情视图每页显示的行数")]
		[DefaultValue(15)]
		public int PageSize { get; init; }
	}

	public override int Execute(CommandContext context, Settings settings)
	{
		LevelReadConfig rs = new()
		{
			ZipProcessingMode = ZipProcessingMode.RootEntriesOnly,
			Strictness = JsonStrictness.Fallback,
		};
		using Level level = Level.FromDirectory(settings.Path, rs);
		var chartDataList = LoadChartData(level);
		var eventCounts = AggregateEventCounts(level);

		while (true)
		{
			AnsiConsole.Clear();

			var choice = AnsiConsole.Prompt(
					new SelectionPrompt<ViewKind>()
							.Title("[bold yellow]选择要查看的视图[/]")
							.PageSize(10)
							.UseConverter(v => v switch
							{
								ViewKind.Summary => "[#00FFFF on #002020]汇总表[/]",
								ViewKind.Chart => "[#00FFFF on #002020]事件图表[/]",
								ViewKind.Exit => "[red]退出[/]",
								_ => v.ToString(),
							})
							.AddChoices(
									ViewKind.Summary,
									ViewKind.Chart,
									ViewKind.Exit));

			AnsiConsole.Clear();

			switch (choice)
			{
				case ViewKind.Summary:
					RenderSummary(chartDataList);
					break;
				case ViewKind.Chart:
					var scale = AnsiConsole.Prompt(
						new SelectionPrompt<ChartScale>()
							.Title("[bold yellow]选择刻度类型[/]")
							.PageSize(5)
							.UseConverter(s => s switch
							{
								ChartScale.Linear => "线性刻度（真实数量）",
								ChartScale.Log => "对数刻度（数量级对比）",
								_ => s.ToString(),
							})
							.AddChoices(ChartScale.Linear, ChartScale.Log));

					AnsiConsole.Clear();
					RenderChart(eventCounts, scale);
					break;
				case ViewKind.Exit:
					return 0;
			}

			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine("[grey]按任意键返回菜单…[/]");
			Console.ReadKey(intercept: true);
		}
	}

	private static void RenderSummary(IReadOnlyList<ChartData> chartDataList)
	{
		var headers = new[] { "总事件数", "总装饰数", "总击拍数", "总按压数", "最小按拍间隔" };

		var summary = new Table()
				.Border(TableBorder.Rounded)
				.Expand()
				.AddColumn(new TableColumn("[green]图表[/]").LeftAligned())
				.AddColumns(headers
						.Select(h => new TableColumn($"[green]{h}[/]").RightAligned())
						.ToArray());

		foreach (var cd in chartDataList)
		{
			summary.AddRow(
			[
					Markup.Escape(cd.ChartName),
								cd.TotalEvents.ToString(),
								cd.TotalDecorations.ToString(),
								cd.TotalHits.ToString(),
								cd.TotalPresses.ToString(),
								cd.MinGap == TimeSpan.MaxValue
										? "N/A"
										: cd.MinGap.TotalMilliseconds.ToString("F2") + " ms",
						]);
		}

		AnsiConsole.Write(new Panel(summary)
				.Expand()
				.Header("[bold yellow]关卡事件统计[/]")
				.BorderColor(Spectre.Console.Color.Blue)
				.Padding(1, 1));
	}

	private static void RenderChart(IReadOnlyDictionary<EventType, int> eventCounts, ChartScale scale)
	{
		BarChart barchart;

		if (scale == ChartScale.Log)
		{
			var logToOriginal = new Dictionary<double, string>();
			var items = new List<BarChartItem>();

			foreach (var kv in eventCounts.Where(kv => kv.Value > 0))
			{
				var logValue = Math.Log10(kv.Value + 1);
				logToOriginal[logValue] = kv.Value.ToString("N0");
				items.Add(new BarChartItem(kv.Key.ToString(), logValue, Spectre.Console.Color.Green));
			}

			barchart = new BarChart()
					.Label("[green bold]事件类型[/] - [yellow bold]数量（对数刻度）[/]")
					.CenterLabel()
					.UseValueFormatter((value, _) =>
							logToOriginal.TryGetValue(value, out var original)
									? original
									: value.ToString("F2"))
					.AddItems(items);
		}
		else
		{
			barchart = new BarChart()
					.Label("[green bold]事件类型[/] - [yellow bold]数量（线性刻度）[/]")
					.CenterLabel()
					.AddItems(eventCounts
							.Where(kv => kv.Value > 0)
							.Select(kv => new BarChartItem(kv.Key.ToString(), kv.Value, Spectre.Console.Color.Green)));
		}

		AnsiConsole.Write(new Panel(barchart)
				.Expand()
				.Header("[bold yellow]事件类型分布[/]")
				.BorderColor(Spectre.Console.Color.Blue)
				.Padding(1, 1));
	}

	private static List<ChartData> LoadChartData(Level level)
	{
		List<ChartData> chartDataList = [];

		foreach (KeyValuePair<string, Chart> kvp in level.Charts)
		{
			Chart chart = kvp.Value;

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
			TimeSpan minGap = TimeSpan.MaxValue;
			for (int i = 1; i < hits.Count; i++)
			{
				var h1 = hits[i];
				if (hit.Merge(h1, out var merged))
					hit = merged;
				else
				{
					press.Add((hit.TickTime.TimeSpan, (hit.TickTime + hit.Hold).TimeSpan - hit.TickTime.TimeSpan));
					if (h1.TickTime.TimeSpan - (hit.TickTime + hit.Hold).TimeSpan < minGap)
						minGap = h1.TickTime.TimeSpan - hit.TickTime.TimeSpan;
					hit = h1;
				}
			}
			chartDataList.Add(new ChartData
			{
				ChartName = kvp.Key,
				TotalEvents = chart.Count,
				TotalDecorations = chart.Decorations.Count,
				TotalHits = hits.Count,
				TotalPresses = press.Count,
				MinGap = minGap,
			});
		}
		return chartDataList;
	}

	private static Dictionary<EventType, int> AggregateEventCounts(Level level)
	{

		Dictionary<EventType, int> eventCounts = [];
		foreach (KeyValuePair<string, Chart> kvp in level.Charts)
		{
			Chart chart = kvp.Value;
			foreach (EventType t in Enum.GetValues<EventType>())
			{
				int count = chart.OfEvents(t).Count();
				if (eventCounts.ContainsKey(t))
					eventCounts[t] += count;
				else
					eventCounts[t] = count;
			}
		}
		return eventCounts;
	}
}

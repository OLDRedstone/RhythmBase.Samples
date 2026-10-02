
#:package RhythmBase@1.3.12-snapshot.20260930032643
#:package Spectre.Console@0.49.1
#:package Spectre.Console.Cli@0.49.1

using RhythmBase.Global.Components;
using RhythmBase.Global.Components.Easing;
using RhythmBase.Global.Components.Vector;
using RhythmBase.Global.Serialization;
using RhythmBase.RhythmDoctor;
using RhythmBase.RhythmDoctor.Components;
using RhythmBase.RhythmDoctor.Components.Conditions;
using RhythmBase.RhythmDoctor.Events;

FileReference barhp = "barhp.png";
FileReference barbg = "barbg.png";
FileReference barfg = "barfg.png";
FileReference barmk = "barmk.png";
FileReference virusbarmk = "virusbarmk.png";
FileReference virusbarfg = "virusbarfg.png";
FileReference virusbarbg = "virusbarbg.png";


const int barhpwidth = 88;
const int virusbarhpwidth = 232;

const int targetBar = 10;
const int maxMiss = 10;
const int maxMissP1 = 10;
const int maxMissP2 = 10;
const int maxHit = 10;

const string tag_updateBar = "updateBar";
const string tag_updateBarInstant = "updateBarInstant";
const string tag_death = "death";
const string tag_updateVirusBar = "updateVirusBar";
const string tag_updateVirusBarInstant = "updateVirusBarInstant";
const string tag_virusDeath = "virusDeath";

const RoomIndex targetRoom = RoomIndex.Room1;

Chart chart = Chart.FromFile(@"E:/Download/Bullseye - KDrew by 9thCore/test222.rdlevel");


CustomCondition cond_alive = new() { Expression = $"numMistakes < {maxMiss}" };
CustomCondition cond_barelyAlive = new() { Expression = $"numMistakes + 1 > {maxMiss}" };
CustomCondition cond_p1_alive = new() { Expression = $"numMistakesP1 < {maxMissP1}" };
CustomCondition cond_p1_barelyAlive = new() { Expression = $"numMistakesP1 + 1 > {maxMissP1}" };
CustomCondition cond_p2_alive = new() { Expression = $"numMistakesP2 < {maxMissP2}" };
CustomCondition cond_p2_barelyAlive = new() { Expression = $"numMistakesP2 + 1 > {maxMissP2}" };
CustomCondition cond_virusAlive = new() { Expression = $"numPerfectHits < {maxHit}" };
CustomCondition cond_virusBarelyAlive = new() { Expression = $"numPerfectHits + 1 > {maxHit}" };
CustomCondition cond_inTargetBar = new() { Expression = $"barNumber > {targetBar} - 1" };

BarConfig barPatient1Config = new()
{
  InitPosition = (17, -10),
  Background = barbg,
  Foreground = barfg,
  Marker = barmk,
  Health = barhp,
  BarColor = 0xFF05DBDD,
  HealthBarWidth = barhpwidth,
  Height = 14,
  Room = targetRoom,
  UpdateBarTag = tag_updateBar,
  UpdateBarInstantTag = tag_updateBarInstant,
  AliveCondition = cond_alive,
  BarelyAliveCondition = cond_barelyAlive,
  MaxHealth = maxMiss,
  HealthVariable = "numMistakes",
};
BarConfig barPatient2Config = new()
{
  InitPosition = (8, -10),
  Background = barbg,
  Foreground = barfg,
  Marker = barmk,
  Health = barhp,
  BarColor = 0xFFDC0D21,
  HealthBarWidth = barhpwidth,
  Height = 14,
  Room = targetRoom,
  UpdateBarTag = tag_updateBar,
  UpdateBarInstantTag = tag_updateBarInstant,
  AliveCondition = cond_alive,
  BarelyAliveCondition = cond_barelyAlive,
  MaxHealth = maxMiss,
  HealthVariable = "numMistakes",
};
BarConfig barVirusConfig = new()
{
  InitPosition = (9, -10),
  Background = virusbarbg,
  Foreground = virusbarfg,
  Marker = virusbarmk,
  Health = barhp,
  BarColor = 0xFFBC2BEC,
  HealthBarWidth = virusbarhpwidth,
  Height = 14,
  Room = targetRoom,
  UpdateBarTag = tag_updateVirusBar,
  UpdateBarInstantTag = tag_updateVirusBarInstant,
  AliveCondition = cond_virusAlive,
  BarelyAliveCondition = cond_virusBarelyAlive,
  MaxHealth = maxHit,
  HealthVariable = "numPerfectHits",
};

#region init

Row targetRow = chart.Rows[0];

chart.Conditionals.Add(cond_alive);
chart.Conditionals.Add(cond_barelyAlive);
chart.Conditionals.Add(cond_virusAlive);
chart.Conditionals.Add(cond_virusBarelyAlive);
chart.Conditionals.Add(cond_inTargetBar);
chart.Conditionals.Add(cond_p1_alive);
chart.Conditionals.Add(cond_p1_barelyAlive);
chart.Conditionals.Add(cond_p2_alive);
chart.Conditionals.Add(cond_p2_barelyAlive);
chart.Add(new CallCustomMethod { MethodName = $"missesToCrackHeart = {maxMiss}" });
chart.Add(new TagAction { Tag = "[onMiss][onHeldPressMiss]miss", ActionTag = tag_updateBar, Condition = new() { [cond_alive] = true } });
chart.Add(new TagAction { Tag = "[onMiss][onHeldPressMiss]miss", ActionTag = tag_death, Condition = new() { [cond_alive] = false, [cond_inTargetBar] = false } });
chart.Add(new TagAction { Tag = "[onHit]hit", ActionTag = tag_updateVirusBar, Condition = new() { [cond_virusAlive] = true } });
chart.Add(new TagAction { Tag = "[onHit]hit", ActionTag = tag_virusDeath, Condition = new() { [cond_virusAlive] = false, [cond_inTargetBar] = false } });
chart.Add(new SetSpeed { Speed = 1f, Tag = tag_death });
chart.Add(new SetPlayStyle { PlayStyle = PlayStyleType.Immediately, IsRelative = false, NextBar = targetBar, Tag = tag_death });
chart.Add(new ShakeScreenCustom { ShakeType = ShakeType.Smooth, Duration = 1f, Frequency = 10, Amplitude = 3, Tag = tag_death });
chart.Add(new CustomFlash { StartColor = new(0xFFFFFFFF), StartOpacity = 30, EndColor = new(0xFFFFFFFF), EndOpacity = 0, Background = false, Duration = 1f, Tag = tag_death });
chart.Add(new CustomFlash { StartColor = new(0xFFFFFFFF), StartOpacity = 0, EndColor = new(0xFFFFFFFF), EndOpacity = 0, Background = true, Duration = 0f, Tag = tag_death });
chart.Add(new CallCustomMethod { MethodName = $"SetHeartMistakes({targetRow.Index}, missesToCrackHeart)", Tag = tag_death });
chart.Add(new ShowHands { Hand = PlayerHand.Right, Action = ShowHandsAction.Hide, ForceRaise = true, Tag = tag_death });
chart.Add(new MoveCamera { Position = (50, 50), Zoom = 100, Angle = 0, Duration = 0f, Tag = tag_death });
foreach (var vfx in new VfxPreset[]{
  VfxPreset.WavyRows,
  VfxPreset.BassDropOnHit,
  VfxPreset.ConfettiBurst,
  VfxPreset.Aberration,
  VfxPreset.JPEG,
  VfxPreset.Grain,
  VfxPreset.ScreenWaves,
  VfxPreset.Mosaic,
  VfxPreset.Noise,
  VfxPreset.Bloom,
  VfxPreset.Saturation,
  VfxPreset.Fisheye,
})
{
  chart.Add(new SetVfxPreset { Preset = vfx, Enable = false, Rooms = Room., Tag = tag_death });
}
chart.Add(new FlipScreen { FlipX = false, FlipY = false, Tag = tag_death });
chart.Add(new InvertColors { Enable = false, Tag = tag_death });
chart.Add(new CallCustomMethod { MethodName = "StopEverything()", Tag = tag_death });
chart.Add(new SetPlayStyle { PlayStyle = PlayStyleType.ProlongOneBar, IsRelative = false, NextBar = targetBar, Tag = tag_virusDeath });
chart.Add(new CallCustomMethod { TickTime = (targetBar, 4), MethodName = "CurrentSongVol(0, 1)" });
chart.Add(new CallCustomMethod { TickTime = (targetBar, 4), MethodName = $"ShowSpotlight({targetRow.Index}, true)" });
chart.Add(new CallCustomMethod { TickTime = (targetBar, 4), MethodName = "skipRankText = true" });
chart.Add(new CallCustomMethod { TickTime = (targetBar, 6), MethodName = "noHitFlashBorder = true" });
chart.Add(new CallCustomMethod { TickTime = (targetBar, 6), MethodName = "MistakeOrHealSilent(9999)" });
chart.Add(new SetVfxPreset { TickTime = (targetBar, 4), Preset = VfxPreset.CutsceneMode, Enable = true });



Bar barVirus = new(chart, barVirusConfig);
Bar barPatient1 = new(chart, barPatient1Config);
Bar barPatient2 = new(chart, barPatient2Config);
#endregion


barVirus.Move((1, 1), (8, 4), 1f, EaseType.OutExpo);
barPatient1.Move((1, 1), (17, 15), 1f, EaseType.OutExpo);
barPatient2.Move((1, 1), (9, 26), 1f, EaseType.OutExpo);
barVirus.SetHealth((1, 1), 1f, 6f, EaseType.OutExpo);
barPatient1.SetHealth((1, 1), 1f, 6f, EaseType.OutExpo);
barPatient2.SetHealth((1, 1), 1f, 6f, EaseType.OutExpo);


chart.SaveToFile(@"E:/Download/Bullseye - KDrew by 9thCore/test.rdlevel", new LevelWriteConfig
{
  WriteIndented = true,
  WriteAligned = true,
  EnableUnsafeRelaxedJsonEscaping = true,
});
record class BarConfig
{
  public required PointN InitPosition { get; init; }
  public required FileReference Background { get; init; }
  public required FileReference Foreground { get; init; }
  public required FileReference Marker { get; init; }
  public required FileReference Health { get; init; }
  public required Color BarColor { get; init; }
  public required int HealthBarWidth { get; init; }
  public required int Height { get; init; }
  public required RoomIndex Room { get; init; }
  public required string UpdateBarTag { get; init; }
  public required string UpdateBarInstantTag { get; init; }
  public required CustomCondition AliveCondition { get; init; }
  public required CustomCondition BarelyAliveCondition { get; init; }
  public required int MaxHealth { get; internal set; }
  public required string HealthVariable { get; internal set; }
}
class Bar
{
  private PointN _pivot = (0, 100);
  private SizeNI _hp_offset = (4, 4);
  private Decoration _bg;
  private Decoration _fg;
  private Decoration _mk;
  private Decoration _hp;
  private BarConfig _config;
  public Bar(Chart chart, BarConfig config)
  {
    _config = config;
    _bg = new Decoration { Character = config.Background.Path, Visible = false, Layer = LayerType.Foreground, Room = config.Room };
    _fg = new Decoration { Character = config.Foreground.Path, Visible = false, Layer = LayerType.Foreground, Room = config.Room };
    _mk = new Decoration { Character = config.Marker.Path, Visible = false, Layer = LayerType.Foreground, Room = config.Room };
    _hp = new Decoration { Character = config.Health.Path, Visible = false, Layer = LayerType.Foreground, Room = config.Room };
    chart.Decorations.Add(_mk);
    chart.Decorations.Add(_fg);
    chart.Decorations.Add(_hp);
    chart.Decorations.Add(_bg);
    _bg.Add(new SetVisible { Visible = true });
    _fg.Add(new SetVisible { Visible = true });
    _mk.Add(new SetVisible { Visible = true });
    _hp.Add(new SetVisible { Visible = true });
    _bg.Add(new Move { Position = config.InitPosition.ToScreenPosition(), Pivot = _pivot });
    _fg.Add(new Move { Position = config.InitPosition.ToScreenPosition(), Pivot = _pivot });
    _mk.Add(new Move { Position = config.InitPosition.ToScreenPosition(), Pivot = _pivot });
    _hp.Add(new Move { Position = (config.InitPosition + _hp_offset).ToScreenPosition(), Pivot = _pivot });
    _hp.Add(new Tint { IsTint = true, TintColor = config.BarColor });
    chart.Add(new CallCustomMethod { MethodName = $"ShakeSprite(\"{Path.GetFileNameWithoutExtension(_bg.Id)}\", 1, 1000, 1, true)", Tag = config.UpdateBarTag });
    chart.Add(new CallCustomMethod { MethodName = $"ShakeSprite(\"{Path.GetFileNameWithoutExtension(_fg.Id)}\", 1, 1000, 1, true)", Tag = config.UpdateBarTag });
    chart.Add(new CallCustomMethod { MethodName = $"ShakeSprite(\"{Path.GetFileNameWithoutExtension(_mk.Id)}\", 1, 1000, 1, true)", Tag = config.UpdateBarTag });
    chart.Add(new CallCustomMethod { MethodName = $"ShakeSprite(\"{Path.GetFileNameWithoutExtension(_hp.Id)}\", 1, 1000, 1, true)", Tag = config.UpdateBarTag });
    _hp.Add(new Move { Scale = (0, null), Ease = EaseType.OutExpo, Duration = 0.5f, Tag = config.UpdateBarTag, Condition = new() { [config.BarelyAliveCondition] = true } });
    _hp.Add(new Move { Scale = ($"({config.MaxHealth - 1} - {config.HealthVariable}) * {config.HealthBarWidth / (config.MaxHealth - 1f)}", null), Ease = EaseType.OutExpo, Duration = 0.5f, Tag = config.UpdateBarTag, Condition = new() { [config.BarelyAliveCondition] = false } });
    _hp.Add(new Tint { IsTint = true, TintColor = Color.White, Tag = config.UpdateBarTag });
    _hp.Add(new Tint { TickTime = (1, 1.5f), IsTint = true, TintColor = config.BarColor, Tag = config.UpdateBarTag });
  }
  public void Move(TickTime tickTime, PointN target, float duration, EaseType easing)
  {
    _bg.Add(new Move { TickTime = tickTime, Position = target.ToScreenPosition(), Duration = duration, Ease = easing });
    _fg.Add(new Move { TickTime = tickTime, Position = target.ToScreenPosition(), Duration = duration, Ease = easing });
    _mk.Add(new Move { TickTime = tickTime, Position = target.ToScreenPosition(), Duration = duration, Ease = easing });
    _hp.Add(new Move { TickTime = tickTime, Position = (target + _hp_offset).ToScreenPosition(), Duration = duration, Ease = easing });
  }
  public void SetHealth(TickTime tickTime, float health, float duration, EaseType easing = EaseType.Linear)
  {
    _hp.Add(new Move { TickTime = tickTime, Scale = (health * _config.HealthBarWidth, null), Duration = duration, Ease = easing });
  }
  public void SwitchRoom(RoomIndex room)
  {
    _bg.Add(new ReorderDecoration { NewRoom = room });
    _fg.Add(new ReorderDecoration { NewRoom = room });
    _mk.Add(new ReorderDecoration { NewRoom = room });
    _hp.Add(new ReorderDecoration { NewRoom = room });
  }
  public void SetVisible(TickTime tickTime, bool visible)
  {
    _bg.Add(new SetVisible { TickTime = tickTime, Visible = visible });
    _fg.Add(new SetVisible { TickTime = tickTime, Visible = visible });
    _mk.Add(new SetVisible { TickTime = tickTime, Visible = visible });
    _hp.Add(new SetVisible { TickTime = tickTime, Visible = visible });
  }
}
static class Tools
{
  private const float ScreenWidth = 352f;
  private const float ScreenHeight = 198f;
  extension(PointN p)
  {
    public PointN ToScreenPosition() => new(p.X / ScreenWidth * 100, (1 - p.Y / ScreenHeight) * 100);
  }
  extension(Point p)
  {
    public Point ToScreenPosition() => new(p.X / ScreenWidth * 100, (1 - p.Y / ScreenHeight) * 100);
  }
  extension(FileReference f)
  {
    public string Name => System.IO.Path.GetFileNameWithoutExtension(f);
  }
}
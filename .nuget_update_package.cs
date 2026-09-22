/*
这个脚本用于更新脚本中RhythmBase包的版本号
This script is used to update the version number of the RhythmBase package in the scripts
*/

#:package NuGet.Protocol@6.10.0
#:package Spectre.Console@0.49.1
#:package Spectre.Console.Cli@0.49.1
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Text.RegularExpressions;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

var config = new CommandApp<Command>();
config.Configure(cfg =>
{
	cfg.SetApplicationName("nuget-update-package");
	cfg.ValidateExamples();
});
return config.Run(args);


partial class Command : AsyncCommand<Config>
{
	[GeneratedRegex(@"^(#:package RhythmBase.*)@[\d\.]+")]
	private static partial Regex MyRegex();
	public async override Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Config settings)
	{
		var latestVersion =
		string.IsNullOrWhiteSpace(settings.Version)
			? await GetLatestPackageVersion("RhythmBase", includePrerelease: settings.IncludePrerelease)
			: NuGetVersion.Parse(settings.Version);
		var files =
			Directory.GetFiles("./scripts/", settings.TargetScript, SearchOption.AllDirectories);
		var regex = MyRegex();
		foreach (var file in files)
		{
			var content = File.ReadAllLines(file);
			for (int i = 0; i < content.Length; i++)
			{
				var match = regex.Match(content[i]);
				if (match.Success)
				{
					content[i] = $"{match.Groups[1].Value}@{latestVersion}";
				}
			}
			File.WriteAllLines(file, content);
		}
		return 0;
	}
	static async Task<NuGetVersion?> GetLatestPackageVersion(
			string packageId,
			bool includePrerelease = false,
			CancellationToken cancellationToken = default)
	{
		var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

		var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>();

		var packages = await metadataResource.GetMetadataAsync(
				packageId,
				includePrerelease: includePrerelease,
				includeUnlisted: false,
				new SourceCacheContext(),
				NullLogger.Instance,
				cancellationToken);

		return packages
				.OrderByDescending(p => p.Identity.Version)
				.FirstOrDefault()?.Identity.Version;
	}
}
class Config : CommandSettings
{
	[CommandOption("-p|--prerelease")]
	[Description("自动更新时包含预发布版本\nAutomatically include pre-release versions when updating")]
	public bool IncludePrerelease { get; set; }
	[CommandOption("-v|--version")]
	[Description("手动更新到的版本号\nManually specify the version number to update to")]
	public string Version { get; set; } = "";
	[CommandOption("-t|--target")]
	[Description("指定要更新的脚本文件\nSpecify the script files to update")]
	public string TargetScript { get; set; } = "*.cs";
}
using System;
using System.ComponentModel.Composition;

namespace Orange
{
	public static partial class Actions
	{

		public const string ConsoleCommandPassArguments = "--passargs";

		[Export(nameof(OrangePlugin.MenuItemsWithErrorDetails))]
		[ExportMetadata("Label", "Build & Run")]
		[ExportMetadata("Priority", 0)]
		public static string BuildAndRunAction()
		{
			AssetCooker.CookForActivePlatform();

			if (!BuildGame()) return "Can not BuildGame";

			The.UI.ScrollLogToEnd();
			RunGame();
			return null;
		}

		public static bool BuildGame()
		{
			return BuildGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
		}

		public static bool BuildGame(TargetPlatform platform, string customSolution = null)
		{
			var builder = new SolutionBuilder(platform, customSolution);
			if (The.Workspace.CleanBeforeBuild) {
				builder.Clean();
			}

			if (!builder.Build()) {
				UserInterface.Instance.ExitWithErrorIfPossible();
				return false;
			}
			return true;
		}

		public static void RunGame()
		{
			RunGame(The.Workspace.ActivePlatform, The.Workspace.CustomSolution);
		}

		public static bool RunGame(TargetPlatform platform, string customSolution = null)
		{
			var builder = new SolutionBuilder(platform, customSolution);
			string arguments = PluginLoader.GetCommandLineArguments();
			string passArguments = Toolbox.GetCommandLineArg(ConsoleCommandPassArguments);
			int exitCode = builder.Run(string.Join(" ", arguments, passArguments));
			if (exitCode != 0) {
				Console.WriteLine("Application terminated with exit code {0}", exitCode);
				return false;
			}
			return true;
		}
	}
}

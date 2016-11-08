﻿using System;
using System.IO;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class OpenProjectCommand : Command
	{
		public override string Text => "Open Project...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.OpenProject;

		public override void Execute()
		{
			var dlg = new FileDialog { AllowedFileTypes = new string[] { "citproj" }, Mode = FileDialogMode.Open };
			if (dlg.RunModal()) {
				if (Project.Current.Close()) {
					new Project(dlg.FileName).Open();
					var prefs = UserPreferences.Instance;
					prefs.RecentProjects.Remove(dlg.FileName);
					prefs.RecentProjects.Insert(0, dlg.FileName);
					prefs.Save();
				}
			}
		}
	}

	public class NewCommand : Command
	{
		public override string Text => "New";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.New;
		public override bool Enabled => Project.Current != null;

		public override void Execute()
		{
			Project.Current.NewDocument();
		}
	}

	public class OpenCommand : Command
	{
		public override string Text => "Open ...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.Open;
		public override bool Enabled => Project.Current != null;

		public override void Execute()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Open,
			};
			if (Document.Current != null) {
				dlg.InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path);
			}
			if (dlg.RunModal()) {
				string localPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out localPath)) {
					var alert = new AlertDialog("Tangerine", "Can't open a document outside the project directory", "Ok");
					alert.Show();
				} else {
					Project.Current.OpenDocument(localPath);
				}
			}
		}
	}

	public class SaveCommand : Command
	{
		public override string Text => "Save";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.Save;
		public override bool Enabled => Document.Current != null;

		static SaveCommand()
		{
			Document.PathSelector += SelectPath;
		}

		public override void Execute()
		{
			Document.Current.Save();
		}

		public static bool SelectPath(out string path)
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path)
			};
			path = null;
			if (!dlg.RunModal()) {
				return false;
			}
			if (!Project.Current.TryGetAssetPath(dlg.FileName, out path)) {
				var alert = new AlertDialog("Tangerine", "Can't save the document outside the project directory", "Ok");
				alert.Show();
				return false;
			}
			return true;
		}
	}

	public class SaveAsCommand : Command
	{
		public override string Text => "Save As...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.SaveAs;
		public override bool Enabled => Document.Current != null;

		public override void Execute()
		{
			SaveAs();
		}

		public static void SaveAs()
		{
			var dlg = new FileDialog {
				AllowedFileTypes = Document.GetSupportedFileTypes(),
				Mode = FileDialogMode.Save,
				InitialDirectory = Project.Current.GetSystemDirectory(Document.Current.Path)
			};
			if (dlg.RunModal()) {
				string assetPath;
				if (!Project.Current.TryGetAssetPath(dlg.FileName, out assetPath)) {
					var alert = new AlertDialog("Tangerine", "Can't save the document outside the project directory", "Ok");
					alert.Show();
				} else {
					Document.Current.SaveAs(assetPath);
				}
			}
		}
	}

	public class CloseDocumentCommand : Command
	{
		public override string Text => "Close Document...";
		public override Shortcut Shortcut => KeyBindings.GenericKeys.CloseDocument;

		public override void Execute()
		{
			if (Document.Current != null) {
				Project.Current.CloseDocument(Document.Current);
			}
		}
	}
}

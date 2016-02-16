﻿#if WIN
using System.Text;
using WinForms = System.Windows.Forms;

namespace Lime
{
	/// <summary>
	/// Brings access to files and folders through the dialog screen.
	/// </summary>
	public class FileDialog : IFileDialog
	{
		/// <summary>
		/// Gets or sets an array of allowed file types. Example { "txt", "png" }.
		/// </summary>
		public string[] AllowedFileTypes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether it is allowed to select more then one file in the open file dialog.
		/// </summary>
		public bool AllowsMultipleSelection { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the folder browser dialog has the "New Folder" button.
		/// </summary>
		public bool CanCreateDirectories { get; set; }

		/// <summary>
		/// Gets a string containing file or folder path selected in the dialog.
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets an array of strings containing paths of all selected files.
		/// </summary>
		public string[] FileNames { get; private set; }

		/// <summary>
		/// Gets or sets a mode indicating whether this dialog opens file, saves file or selects folder.
		/// </summary>
		public FileDialogMode Mode { get; set; }

		/// <summary>
		/// Shows dialog.
		/// </summary>
		/// <returns>return true if user clicks OK in the dialog.</returns>
		public bool RunModal()
		{
			FileName = null;
			FileNames = null;
			switch (Mode) {
				case FileDialogMode.Open:
					return ShowOpenFileDialog();
				case FileDialogMode.Save:
					return ShowSaveFileDialog();
				case FileDialogMode.SelectFolder:
					return ShowFolderBrowserDialog();
			}
			return false;
		}

		private void SetFilter(WinForms.FileDialog dialog)
		{
			if (AllowedFileTypes != null) {
				var result = new StringBuilder();
				var i = 0;
				foreach (var type in AllowedFileTypes) {
					i++;
					result.Append(type.ToUpper()).Append("|*.").Append(type);
					if (i < AllowedFileTypes.Length) {
						result.Append("|");
					}
				}
				dialog.Filter = result.ToString();
			}
		}

		private bool ShowFileDialog(WinForms.FileDialog dialog)
		{
			if (dialog.ShowDialog() == WinForms.DialogResult.OK) {
				FileName = dialog.FileName;
				FileNames = dialog.FileNames;
				return true;
			}
			return false;
		}

		private bool ShowFolderBrowserDialog()
		{
			using (var folderBrowserDialog = new WinForms.FolderBrowserDialog()) {
				folderBrowserDialog.ShowNewFolderButton = CanCreateDirectories;
				if (folderBrowserDialog.ShowDialog() == WinForms.DialogResult.OK) {
					FileName = folderBrowserDialog.SelectedPath;
					FileNames = new[] { FileName };
					return true;
				}
			}
			return false;
		}

		private bool ShowOpenFileDialog()
		{
			using (var openFileDialog = new WinForms.OpenFileDialog()) {
				SetFilter(openFileDialog);
				openFileDialog.RestoreDirectory = true;
				openFileDialog.Multiselect = AllowsMultipleSelection;
				return ShowFileDialog(openFileDialog);
			}
		}

		private bool ShowSaveFileDialog()
		{
			using (var saveFileDialog = new WinForms.SaveFileDialog()) {
				SetFilter(saveFileDialog);
				saveFileDialog.RestoreDirectory = true;
				return ShowFileDialog(saveFileDialog);
			}
		}
	}
}
#endif
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orange
{
	public abstract class UserInterface
	{
		public static UserInterface Instance;

		public static void CreateGUI()
		{
			Instance = new MainWindow();
			Instance.Initialize();
		}

		public static void CreateConsole()
		{
			Instance = new ConsoleUI();
			Instance.Initialize();
		}

		public virtual void Initialize() { }

		public virtual void ClearLog() { }

		public virtual void ScrollLogToEnd() { }

		public virtual void RefreshMenu() { }

		public abstract bool AskConfirmation(string text);

		public abstract bool AskChoice(string text, out bool yes);

		public abstract TargetPlatform GetActivePlatform();

		public virtual void ProcessPendingEvents() { }

		public virtual void OnWorkspaceOpened() { }

		public abstract bool DoesNeedSvnUpdate();
	}
}

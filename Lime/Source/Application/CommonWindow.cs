﻿using System;

namespace Lime
{
	public abstract class CommonWindow
	{
		public event Action Activated;
		public event Action Deactivated;
		public event ClosingDelegate Closing;
		public event Action Closed;
		public event Action Moved;
		public event ResizeDelegate Resized;
		public event UpdatingDelegate Updating;
		public event Action Rendering;
		public event VisibleChangingDelegate VisibleChanging;
		public object Tag { get; set; }

		public static IWindow Current { get; private set; }
		public IContext Context { get; set; }

		public event Action<System.Exception> UnhandledExceptionOnUpdate;

		protected CommonWindow()
		{
			if (Current == null) {
				Current = (IWindow)this;
			}
			Context = new Context(new Property(typeof(CommonWindow), nameof(Current)), this);
		}

		protected void RaiseActivated()
		{
			using (Context.Activate().Scoped()) {
				if (Activated != null) {
					Activated();
				}
			}
		}

		protected void RaiseDeactivated()
		{
			using (Context.Activate().Scoped()) {
				if (Deactivated != null) {
					Deactivated();
				}
			}
		}

		protected void RaiseClosed()
		{
			using (Context.Activate().Scoped()) {
				if (Closed != null) {
					Closed();
				}
			}
		}

		protected void RaiseRendering()
		{
			using (Context.Activate().Scoped()) {
				if (Rendering != null) {
					Rendering();
				}
			}
		}

		protected void RaiseUpdating(float delta)
		{
			using (Context.Activate().Scoped()) {
				try {
					if (Updating != null) {
						Updating(delta);
					}
					if (Current.Active) {
						Application.MainMenu?.Refresh();
						Application.RaiseActiveWindowUpdated(Current);
						Application.UpdateCounter++;
					}
				} catch (System.Exception e) {
					if (UnhandledExceptionOnUpdate != null) {
						UnhandledExceptionOnUpdate(e);
					} else {
						throw;
					}
				}
			}
		}

		protected bool RaiseClosing(CloseReason reason)
		{
			using (Context.Activate().Scoped()) {
				if (Closing != null) {
					return Closing(reason);
				}
			}
			return true;
		}

		protected void RaiseMoved()
		{
			using (Context.Activate().Scoped()) {
				if (Moved != null) {
					Moved();
				}
			}
		}

		protected void RaiseResized(bool deviceRotated)
		{
			using (Context.Activate().Scoped()) {
				if (Resized != null) {
					Resized(deviceRotated);
				}
			}
		}

		protected void RaiseVisibleChanging(bool value)
		{
			using (Context.Activate().Scoped()) {
				if (VisibleChanging != null) {
					VisibleChanging(value);
				}
			}
		}
	}
}

using System;
using Lime;
using ProtoBuf;
using System.IO;
using System.Collections.Generic;
using Kumquat;

namespace Lime
{
	[ProtoContract]
	public enum RenderTarget
	{
		[ProtoEnum]
		None,
		[ProtoEnum]
		A,
		[ProtoEnum]
		B,
		[ProtoEnum]
		C,
		[ProtoEnum]
		D,
	}
	
	public delegate void UpdateDelegate(float delta);

	public class KeyEventArgs : EventArgs 
	{
		public bool Consumed;
	}

	[ProtoContract]
	[ProtoInclude(100, typeof(Kumquat.Area))]
	public class Frame : Widget, IImageCombinerArg
	{
		RenderTarget renderTarget;
		ITexture renderTexture;

		public BareEventHandler Rendered;
		public BareEventHandler Clicked;

		[ProtoMember(1)]
		public RenderTarget RenderTarget {
			get { return renderTarget; }
			set {
				renderTarget = value;
				renderedToTexture = value != RenderTarget.None;
				switch(value) {
				case RenderTarget.A:
					renderTexture = new SerializableTexture("#a");
					break;
				case RenderTarget.B:
					renderTexture = new SerializableTexture("#b");
					break;
				case RenderTarget.C:
					renderTexture = new SerializableTexture("#c");
					break;
				case RenderTarget.D:
					renderTexture = new SerializableTexture("#d");
					break;
				default:
					renderTexture = null;
					break;
				}
			}
		}

		public float AnimationSpeed = 1;

		// In dialog mode frame acts like a modal dialog, all controls behind the dialog are frozen.
		// If dialog is being shown or hidden then all controls on dialog are frozen either.
		public bool DialogMode;

		public Frame() {}

		public Frame(Vector2 position)
		{
			this.Position = position;
		}

		void IImageCombinerArg.BypassRendering() {}

		ITexture IImageCombinerArg.GetTexture()
		{
			return renderTexture;
		}

		private void UpdateForDialogMode(int delta)
		{
			if (!World.Instance.IsTopDialogUpdated) {
				if (globallyVisible && Input.MouseVisible) {
					if (World.Instance.ActiveWidget != null && !World.Instance.ActiveWidget.ChildOf(this)) {
						// Discard active widget if it's not a child of the topmost dialog.
						World.Instance.ActiveWidget = null;
					}
				}
				if (globallyVisible && World.Instance.ActiveTextWidget != null && !World.Instance.ActiveTextWidget.ChildOf(this)) {
					// Discard active text widget if it's not a child of the topmost dialog.
					World.Instance.ActiveTextWidget = null;
				}
			}
			if (!IsRunning) {
				base.Update(delta);
			}
			if (globallyVisible) {
				// Consume all input events and drive mouse out of the screen.
				Input.ConsumeAllKeyEvents(true);
				Input.MouseVisible = false;
				Input.TextInput = null;
			}
			if (IsRunning) {
				base.Update(delta);
			}
			World.Instance.IsTopDialogUpdated = true;
		}

		public override void LateUpdate(int delta)
		{
			if (AnimationSpeed != 1) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			while (delta > MaxTimeDelta) {
				base.LateUpdate(MaxTimeDelta);
				delta -= MaxTimeDelta;
			}
			base.LateUpdate(delta);
		}

		private void UpdateHelper(int delta)
		{
			if (DialogMode) {
				UpdateForDialogMode(delta);
			} else {
				base.Update(delta);
			}
		}

		public override void Update(int delta)
		{
			if (AnimationSpeed != 1) {
				delta = MultiplyDeltaByAnimationSpeed(delta);
			}
			while (delta > MaxTimeDelta) {
				UpdateHelper(MaxTimeDelta);
				delta -= MaxTimeDelta;
			}
			UpdateHelper(delta);
			if (Clicked != null) {
				if (Input.WasMouseReleased() && HitTest(Input.MousePosition)) {
					Clicked();
				}
			}
		}

		private int MultiplyDeltaByAnimationSpeed(int delta)
		{
			delta = (int)(delta * AnimationSpeed);
			if (delta < 0) {
				throw new System.ArgumentOutOfRangeException("delta");
			}
			return delta;
		}

		public override void Render()
		{
			if (renderTexture != null) {
				RenderToTexture(renderTexture);
			}
			if (Rendered != null) {
				Renderer.Transform1 = GlobalMatrix;
				Rendered();
			}
		}

		public override void AddToRenderChain(RenderChain chain)
		{
			if (globallyVisible) {
				if (renderTexture != null)
					chain.Add(this);
				else
					base.AddToRenderChain(chain);
			}
		}

		private static HashSet<string> processingFiles = new HashSet<string>();

		public Frame(Node parent, string path)
		{
			LoadFromBundle(path);
			if (parent != null) {
				parent.PushNode(this);
			}
		}

		public Frame(string path)
		{
			LoadFromBundle(path);
		}

		private void LoadFromBundle(string path)
		{
			path = Path.ChangeExtension(path, "scene");
			if (processingFiles.Contains(path))
				throw new Lime.Exception("Cyclic dependency of scenes has detected: {0}", path);
			processingFiles.Add(path);
			try {
				using (Stream stream = AssetsBundle.Instance.OpenFileLocalized(path)) {
					Serialization.ReadObject<Frame>(path, stream, this);
				}
				LoadContent();
				Tag = path;
			} finally {
				processingFiles.Remove(path);
			}
		}

		public static Frame Create(string path)
		{
			return new Frame(path);
		}

		public void LoadContent()
		{
			if (!string.IsNullOrEmpty(ContentsPath))
				LoadContentHelper();
			else {
				foreach (Node node in Nodes.AsArray) {
					if (node is Frame) {
						(node as Frame).LoadContent();
					}
				}
			}
		}

		private void LoadContentHelper()
		{
			Nodes.Clear();
			Markers.Clear();
			var contentsPath = Path.ChangeExtension(ContentsPath, "scene");
			if (AssetsBundle.Instance.FileExists(contentsPath)) {
				Frame content = Frame.Create(ContentsPath);
				if (content.Widget != null && Widget != null) {
					content.Update(0);
					content.Widget.Size = Widget.Size;
					content.Update(0);
				}
				foreach (Marker marker in content.Markers)
					Markers.Add(marker);
				foreach (Node node in content.Nodes.AsArray) {
					node.Unlink();
					Nodes.Add(node);
				}
			}
		}

		public static Frame CreateSubframe(string path)
		{
			var frame = Create(path).Nodes[0] as Frame;
			frame.Parent = null;
			frame.Tag = path;
			return frame;
		}

		public static Frame CreateAndRun(Node parent, string path, string marker = null)
		{
			Frame frame = Create(path);
			frame.Tag = path;
			if (marker == null) {
				frame.IsRunning = true;
			} else {
				frame.RunAnimation(marker);
			}
			if (parent != null) {
				parent.Nodes.Insert(0, frame);
			}
			return frame;
		}

		public static Frame CreateSubframeAndRun(Node parent, string path, string marker = null)
		{
			Frame frame = Frame.Create(path).Nodes[0] as Frame;
			frame.Parent = null;
			frame.Tag = path;
			if (marker == null) {
				frame.IsRunning = true;
			} else {
				frame.RunAnimation(marker);
			}
			if (parent != null) {
				parent.Nodes.Insert(0, frame);
			}
			return frame;
		}
	}
}

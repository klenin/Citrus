#if WIN
using System;

namespace Lime
{
	public class WebBrowser : Widget
	{
		public static Func<IWebBrowserImplementation> BrowserFactory = () => new WinFormsWebBrowser();
		private IWebBrowserImplementation implementation;

		public Uri Url
		{
			get { return implementation.Url; }
			set { implementation.Url = value; }
		}

		public WebBrowser(): base()
		{
			implementation = BrowserFactory();
		}

		public WebBrowser(Widget parentWidget)
			: this()
		{
			AddToWidget(parentWidget);
		}

		public void AddToWidget(Widget parentWidget)
		{
			parentWidget.AddNode(this);
			Anchors = Anchors.LeftRightTopBottom;
			Size = parentWidget.Size;
		}

		public override void Render()
		{
			implementation.Render(this);
		}

		protected override void OnSizeChanged(Vector2 sizeDelta)
		{
			// OnSizeChanged() is called before WebBrowser constructor, where normal
			// web browser implementation is constructed
			if (implementation != null)
				implementation.OnSizeChanged(this, sizeDelta);
		}

		public override void Update(float delta)
		{
			base.Update(delta);
			implementation.Update(this, delta);
		}

		public override void Dispose()
		{
			base.Dispose();
			implementation.Dispose();
		}
	}
}
#endif
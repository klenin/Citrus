#if iOS
using ApiDefinition;
using System.Runtime.InteropServices;


#region --- License ---
/* Licensed under the MIT/X11 license.
 * Copyright (c) 2009 Novell, Inc.
 * Copyright 2013 Xamarin Inc
 * This notice may not be removed from any source distribution.
 * See license.txt for licensing detailed licensing details.
 */
// Copyright 2011 Xamarin Inc. All rights reserved.
#endregion

using System;
using System.ComponentModel;
using System.Drawing;

using CoreAnimation;
using CoreGraphics;
using Foundation;
using OpenGLES;
using UIKit;
using ObjCRuntime;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform;
using OpenTK.Platform.iPhoneOS;

using All  = OpenTK.Graphics.ES11.All;
using ES11 = OpenTK.Graphics.ES11;
using ES20 = OpenTK.Graphics.ES20;

#pragma warning disable 0618

namespace Lime.Xamarin
{
	public class FrameEventArgs
	{
		public double Time;
	}

    sealed class GLCalls {
        public delegate void glBindFramebuffer(All target, int framebuffer);
        public delegate void glBindRenderbuffer(All target, int renderbuffer);
        public delegate void glDeleteFramebuffers(int n, ref int framebuffers);
        public delegate void glDeleteRenderbuffers(int n, ref int renderbuffers);
        public delegate void glFramebufferRenderbuffer(All target, All attachment, All renderbuffertarget, int renderbuffer);
        public delegate void glGenFramebuffers(int n, ref int framebuffers);
        public delegate void glGenRenderbuffers(int n, ref int renderbuffers);
        public delegate void glGetInteger(All name, ref int value);
        public delegate void glScissor(int x, int y, int width, int height);
        public delegate void glViewport(int x, int y, int width, int height);
        public delegate void glGetRenderBufferParameter(All target, All pname, ref int p);
        public delegate void glPixelStore(All pname, int param);
        public delegate void glReadPixels(int x, int y, int width, int height, All format, All type, byte[] pixels);

        public glBindFramebuffer BindFramebuffer;
        public glBindRenderbuffer BindRenderbuffer;
        public glDeleteFramebuffers DeleteFramebuffers;
        public glDeleteRenderbuffers DeleteRenderbuffers;
        public glFramebufferRenderbuffer FramebufferRenderbuffer;
        public glGenFramebuffers GenFramebuffers;
        public glGenRenderbuffers GenRenderbuffers;
        public glGetInteger GetInteger;
        public glScissor Scissor;
        public glViewport Viewport;
        public glGetRenderBufferParameter GetRenderbufferParameter;
        public glPixelStore PixelStore;
        public glReadPixels ReadPixels;

        public static GLCalls GetGLCalls(EAGLRenderingAPI api)
        {
            switch (api) {
                case EAGLRenderingAPI.OpenGLES1: return CreateES1();
                case EAGLRenderingAPI.OpenGLES2: return CreateES2();
            }
            throw new ArgumentException("api");
        }

        static GLCalls CreateES1()
        {
            return new GLCalls() {
                BindFramebuffer         = (t, f)              => ES11.GL.Oes.BindFramebuffer(t, f),
                BindRenderbuffer        = (t, r)              => ES11.GL.Oes.BindRenderbuffer(t, r),
                DeleteFramebuffers      = (int n, ref int f)  => ES11.GL.Oes.DeleteFramebuffers(n, ref f),
                DeleteRenderbuffers     = (int n, ref int r)  => ES11.GL.Oes.DeleteRenderbuffers(n, ref r),
                FramebufferRenderbuffer = (t, a, rt, rb)      => ES11.GL.Oes.FramebufferRenderbuffer(t, a, rt, rb),
                GenFramebuffers         = (int n, ref int f)  => ES11.GL.Oes.GenFramebuffers(n, out f),
                GenRenderbuffers        = (int n, ref int r)  => ES11.GL.Oes.GenRenderbuffers(n, out r),
                GetInteger              = (All n, ref int v)  => ES11.GL.GetInteger(n, out v),
                Scissor                 = (x, y, w, h)        => ES11.GL.Scissor(x, y, w, h),
                Viewport                = (x, y, w, h)        => ES11.GL.Viewport(x, y, w, h),
                GetRenderbufferParameter= (All t, All p, ref int a) => ES11.GL.Oes.GetRenderbufferParameter (t, p, out a),
                PixelStore              = (n, p)                    => ES11.GL.PixelStore(n, p),
                ReadPixels              = (x, y, w, h, f, t, d)     => ES11.GL.ReadPixels(x, y, w, h, f, t, d),
            };
        }

        static GLCalls CreateES2()
        {
            return new GLCalls() {
				BindFramebuffer         = (t, f)              => ES20.GL.BindFramebuffer((ES20.All)t, f),
				BindRenderbuffer        = (t, r)              => ES20.GL.BindRenderbuffer((ES20.All) t, r),
                DeleteFramebuffers      = (int n, ref int f)  => ES20.GL.DeleteFramebuffers(n, ref f),
                DeleteRenderbuffers     = (int n, ref int r)  => ES20.GL.DeleteRenderbuffers(n, ref r),
				FramebufferRenderbuffer = (t, a, rt, rb)      => ES20.GL.FramebufferRenderbuffer((ES20.All) t, (ES20.All) a, (ES20.All) rt, rb),
                GenFramebuffers         = (int n, ref int f)  => ES20.GL.GenFramebuffers(n, out f),
                GenRenderbuffers        = (int n, ref int r)  => ES20.GL.GenRenderbuffers(n, out r),
				GetInteger              = (All n, ref int v)  => ES20.GL.GetInteger((ES20.All) n, ref v),
                Scissor                 = (x, y, w, h)        => ES20.GL.Scissor(x, y, w, h),
                Viewport                = (x, y, w, h)        => ES20.GL.Viewport(x, y, w, h),
				GetRenderbufferParameter= (All t, All p, ref int a) => ES20.GL.GetRenderbufferParameter ((ES20.All) t, (ES20.All) p, ref a),
				PixelStore              = (n, p)                    => ES20.GL.PixelStore((ES20.All) n, p),
				ReadPixels              = (x, y, w, h, f, t, d)     => ES20.GL.ReadPixels(x, y, w, h, (ES20.All) f, (ES20.All) t, d),
            };
        }
    }

    interface ITimeSource {
        void Suspend ();
        void Resume ();

        void Invalidate ();
    }

    [Register]
    class CADisplayLinkTimeSource : NSObject, ITimeSource {
        static Selector selRunIteration = new Selector ("runIteration");

        iPhoneOSGameView view;
        CADisplayLink displayLink;

        public CADisplayLinkTimeSource (iPhoneOSGameView view, int frameInterval)
        {
            this.view = view;

            if (displayLink != null)
                displayLink.Invalidate ();

            displayLink = CADisplayLink.Create (this, selRunIteration);
            displayLink.FrameInterval = frameInterval;
            displayLink.Paused = true;
        }

        public void Suspend ()
        {
            displayLink.Paused = true;
        }

        public void Resume ()
        {
            displayLink.Paused = false;
            displayLink.AddToRunLoop (NSRunLoop.Main, NSRunLoop.NSDefaultRunLoopMode);
        }

        public void Invalidate ()
        {
            if (displayLink != null) {
                displayLink.Invalidate ();
                displayLink = null;
            }
        }

        [Export ("runIteration")]
        [Preserve (Conditional = true)]
        void RunIteration ()
        {
            view.RunIteration (null);
        }
    }

    class NSTimerTimeSource : ITimeSource {

        TimeSpan timeout;
        NSTimer timer;

        iPhoneOSGameView view;

        public NSTimerTimeSource (iPhoneOSGameView view, double updatesPerSecond)
        {
            this.view = view;

            // Can't use TimeSpan.FromSeconds() as that only has 1ms
            // resolution, and we need better (e.g. 60fps doesn't fit nicely
            // in 1ms resolution, but does in ticks).
            timeout = new TimeSpan ((long) (((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));
        }

        public void Suspend ()
        {
            if (timer != null) {
                timer.Invalidate ();
                timer = null;
            }
        }

        public void Resume ()
        {
            if (timeout != new TimeSpan (-1))
                timer = NSTimer.CreateRepeatingScheduledTimer(timeout, view.RunIteration);
        }

        public void Invalidate ()
        {
            if (timer != null) {
                timer.Invalidate ();
                timer = null;
            }
        }
    }

    public class iPhoneOSGameView : UIView
    {
        bool suspended;
        bool disposed;

        int framebuffer, renderbuffer;

        GLCalls gl;

        ITimeSource timesource;
        System.Diagnostics.Stopwatch stopwatch;
        TimeSpan prevUpdateTime;
        TimeSpan prevRenderTime;


        [Export("initWithCoder:")]
        public iPhoneOSGameView(NSCoder coder)
            : base(coder)
        {
            stopwatch = new System.Diagnostics.Stopwatch ();
        }

        [Export("initWithFrame:")]
		public iPhoneOSGameView(CGRect frame)
            : base(frame)
        {
            stopwatch = new System.Diagnostics.Stopwatch ();
        }

        [Export ("layerClass")]
        public static Class GetLayerClass ()
        {
            return new Class (typeof (CAEAGLLayer));
        }

        void AssertValid()
        {
            if (disposed)
                throw new ObjectDisposedException("");
        }

        void AssertContext()
        {
            if (GraphicsContext == null)
                throw new InvalidOperationException("Operation requires a GraphicsContext, which hasn't been created yet.");
        }

        EAGLRenderingAPI api;
        public EAGLRenderingAPI ContextRenderingApi {
            get {
                AssertValid();
                return api;
            }
            set {
                AssertValid();
                if (GraphicsContext != null)
                    throw new NotSupportedException("Can't change RenderingApi after GraphicsContext is constructed.");
                this.api = value;
            }
        }

        public IGraphicsContext GraphicsContext {get; set;}
        public EAGLContext EAGLContext {
            get {
                AssertValid();
                if (GraphicsContext != null) {
                    iPhoneOSGraphicsContext c = GraphicsContext as iPhoneOSGraphicsContext;
                    if (c != null)
                        return c.EAGLContext;
                    var i = GraphicsContext as IGraphicsContextInternal;
                    IGraphicsContext ic = i == null ? null : i.Implementation;
                    c = ic as iPhoneOSGraphicsContext;
                    if (c != null)
                        return c.EAGLContext;
                }
                return null;
            }
        }

        bool retainedBacking;
        public bool LayerRetainsBacking {
            get {
                AssertValid();
                return retainedBacking;
            }
            set {
                AssertValid();
                if (GraphicsContext != null)
                    throw new NotSupportedException("Can't change LayerRetainsBacking after GraphicsContext is constructed.");
                retainedBacking = value;
            }
        }

        NSString colorFormat;
        public NSString LayerColorFormat {
            get {
                AssertValid();
                return colorFormat;
            }
            set {
                AssertValid();
                if (GraphicsContext != null)
                    throw new NotSupportedException("Can't change LayerColorFormat after GraphicsContext is constructed.");
                colorFormat = value;
            }
        }

        public int Framebuffer {
            get {return framebuffer;}
        }

        public int Renderbuffer {
            get {return renderbuffer;}
        }

        public bool AutoResize {get; set;}

        UIViewController GetViewController()
        {
            UIResponder r = this;
            while (r != null) {
                var c = r as UIViewController;
                if (c != null)
                    return c;
                r = r.NextResponder;
            }
            return null;
        }

        public virtual string Title {
            get {
                AssertValid();
                var c = GetViewController();
                if (c != null)
                    return c.Title;
                throw new NotSupportedException();
            }
            set {
                AssertValid();
                var c = GetViewController();
                if (c != null) {
                    if (c.Title != value) {
                        c.Title = value;
                        OnTitleChanged(EventArgs.Empty);
                    }
                }
                else
                    throw new NotSupportedException();
            }
        }

        protected virtual void OnTitleChanged(EventArgs e)
        {
            var h = TitleChanged;
            if (h != null)
                h (this, EventArgs.Empty);
        }

        public virtual bool Visible {
            get {
                AssertValid();
                return !base.Hidden;
            }
            set {
                AssertValid();
                if (base.Hidden != !value) {
                    base.Hidden = !value;
                    OnVisibleChanged(EventArgs.Empty);
                }
            }
        }

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            var h = VisibleChanged;
            if (h != null)
                h (this, EventArgs.Empty);
        }

        public virtual IWindowInfo WindowInfo {
            get {
                AssertValid();
                return null;
            }
        }

        public virtual WindowState WindowState {
            get {
                AssertValid();
                var c = GetViewController();
                if (c != null && c.WantsFullScreenLayout)
                    return WindowState.Fullscreen;
                return WindowState.Normal;
            }
            set {
                AssertValid();
                var c = GetViewController();
                if (c != null) {
                    if (c.WantsFullScreenLayout == (value == WindowState.Fullscreen)) {
                        c.WantsFullScreenLayout = value == WindowState.Fullscreen;
                        OnWindowStateChanged(EventArgs.Empty);
                    }
                }
            }
        }

        protected virtual void OnWindowStateChanged(EventArgs e)
        {
            var h = WindowStateChanged;
            if (h != null)
                h (this, EventArgs.Empty);
        }

        public virtual WindowBorder WindowBorder {
            get {
                AssertValid();
                return WindowBorder.Hidden;
            }
            set {}
        }

        Size size;
        public Size Size {
            get {
                AssertValid();
                return size;
            }
            set {
                AssertValid();
                if (size != value) {
                    size = value;
                    OnResize(EventArgs.Empty);
                }
            }
        }

        protected virtual void OnResize(EventArgs e)
        {
            var h = Resize;
            if (h != null)
                h (this, e);
        }

        protected virtual void CreateFrameBuffer()
        {
            AssertValid();
            if (LayerColorFormat == null)
                throw new InvalidOperationException("Set the LayerColorFormat property to an EAGLColorFormat value before calling Run().");

            CAEAGLLayer eaglLayer = (CAEAGLLayer) Layer;
            eaglLayer.DrawableProperties = NSDictionary.FromObjectsAndKeys (
                    new NSObject [] {NSNumber.FromBoolean(LayerRetainsBacking), LayerColorFormat},
                    new NSObject [] {EAGLDrawableProperty.RetainedBacking,      EAGLDrawableProperty.ColorFormat}
            );
            ConfigureLayer(eaglLayer);

            GraphicsContext = Utilities.CreateGraphicsContext(ContextRenderingApi);
            gl = GLCalls.GetGLCalls(ContextRenderingApi);

			gl.GenFramebuffers(1, ref framebuffer);
			gl.BindFramebuffer(All.FramebufferOes, framebuffer);

            gl.GenRenderbuffers(1, ref renderbuffer);
			gl.BindRenderbuffer(All.RenderbufferOes, renderbuffer);

			if (!EAGLContext.RenderBufferStorage((uint)All.RenderbufferOes, eaglLayer)) {
				throw new InvalidOperationException("Error with EAGLContext.RenderBufferStorage!");
            }
			gl.FramebufferRenderbuffer(All.FramebufferOes, All.ColorAttachment0Oes, All.RenderbufferOes, renderbuffer);
			
            Size newSize = new Size(
                    (int) Math.Round(eaglLayer.Bounds.Size.Width), 
                    (int) Math.Round(eaglLayer.Bounds.Size.Height));
            Size = newSize;

            gl.Viewport(0, 0, newSize.Width, newSize.Height);
            gl.Scissor(0, 0, newSize.Width, newSize.Height);

            frameBufferWindow = new WeakReference(Window);
            frameBufferLayer = new WeakReference(Layer);
        }

        protected virtual void ConfigureLayer(CAEAGLLayer eaglLayer)
        {
        }

        protected virtual void DestroyFrameBuffer()
        {
            AssertValid();
            AssertContext();
            EAGLContext oldContext = EAGLContext.CurrentContext;
            if (!GraphicsContext.IsCurrent)
                MakeCurrent();

			DeleteBuffers();

            if (oldContext != EAGLContext)
                EAGLContext.SetCurrentContext(oldContext);
            else
                EAGLContext.SetCurrentContext(null);

            GraphicsContext.Dispose();
            GraphicsContext = null;
            gl = null;
        }
        
        protected virtual void DeleteBuffers()
        {
			DetachRenderBufferStorage(EAGLContext, (uint)All.RenderbufferOes); 
			gl.DeleteFramebuffers(1, ref framebuffer);
			gl.DeleteRenderbuffers(1, ref renderbuffer);
			framebuffer = renderbuffer = 0;
        }
			
		[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
		private static extern bool bool_objc_msgSend_nuint_IntPtr (IntPtr receiver, IntPtr selector, nuint arg1, IntPtr arg2);

		static private bool DetachRenderBufferStorage(EAGLContext me, uint target)
		{
			// return me.RenderBufferStorage(target, (CAEAGLLayer)null);
			return bool_objc_msgSend_nuint_IntPtr(me.Handle, Selector.GetHandle("renderbufferStorage:fromDrawable:"), target, 
					(IntPtr)0);
		}

        public virtual void Close()
        {
            AssertValid();
            OnClosed(EventArgs.Empty);
        }

        protected virtual void OnClosed(EventArgs e)
        {
            var h = Closed;
            if (h != null)
                h (this, e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing) {
                if (timesource != null)
			        timesource.Invalidate ();
                timesource = null;
                if (stopwatch != null)
			        stopwatch.Stop();
                stopwatch = null;
                DestroyFrameBuffer();
            }
            base.Dispose (disposing);
            disposed = true;
            if (disposing)
                OnDisposed(EventArgs.Empty);
        }

        protected virtual void OnDisposed(EventArgs e)
        {
            var h = Disposed;
            if (h != null)
                h (this, e);
        }

        public override void LayoutSubviews()
        {
            if (GraphicsContext == null)
                return;

            var bounds = Bounds;
            if (AutoResize && (Math.Round(bounds.Width) != Size.Width ||
                        Math.Round(bounds.Height) != Size.Height)) {
                DestroyFrameBuffer();
                CreateFrameBuffer();
            }
        }

        public virtual void MakeCurrent()
        {
            AssertValid();
            AssertContext();
            GraphicsContext.MakeCurrent(WindowInfo);
        }

        public virtual void SwapBuffers()
        {
            AssertValid();
            AssertContext();
			gl.BindRenderbuffer(All.RenderbufferOes, renderbuffer);
            GraphicsContext.SwapBuffers();
        }

        WeakReference frameBufferWindow;
        WeakReference frameBufferLayer;

        public void Run()
        {
            RunWithFrameInterval (1);
        }

        public void Run (double updatesPerSecond)
        {
            if (updatesPerSecond < 0.0)
                throw new ArgumentException ("updatesPerSecond");
            
            if (updatesPerSecond == 0.0) {
                RunWithFrameInterval (1);
                return;
            }
            
	    if (timesource != null)
		    timesource.Invalidate ();

	    timesource = new NSTimerTimeSource (this, updatesPerSecond);

            CreateFrameBuffer ();
            OnLoad (EventArgs.Empty);
	    Start ();
        }

	[Obsolete ("Use either Run (float updatesPerSecond) or RunWithFrameInterval (int frameInterval)")]
	public void Run (int frameInterval)
	{
		RunWithFrameInterval (frameInterval);
	}

        public void RunWithFrameInterval (int frameInterval)
        {
            AssertValid ();
            
            if (frameInterval < 1)
                throw new ArgumentException ("frameInterval");

	    if (timesource != null)
		    timesource.Invalidate ();

	    timesource = new CADisplayLinkTimeSource (this, frameInterval);

            CreateFrameBuffer ();
            OnLoad (EventArgs.Empty);
            Start ();
        }

	void Start ()
	{
	    prevUpdateTime = TimeSpan.Zero;
	    prevRenderTime = TimeSpan.Zero;
	    Resume ();
	}

        public void Stop()
        {
            AssertValid();
            if (timesource != null) {
                timesource.Invalidate ();
                timesource = null;
            }
            suspended = false;
            OnUnload(EventArgs.Empty);
        }

        void Suspend ()
        {
            if (timesource != null)
                timesource.Suspend ();
            stopwatch.Stop();
            suspended = true;
        }

        void Resume ()
        {
            if (timesource != null)
                timesource.Resume ();
            stopwatch.Start();
            suspended = false;
        }

        public UIImage Capture ()
        {
            // This is from: https://developer.apple.com/library/ios/#qa/qa2010/qa1704.html

            int backingWidth = 0, backingHeight = 0;

            // Bind the color renderbuffer used to render the OpenGL ES view
            // If your application only creates a single color renderbuffer which is already bound at this point,
            // this call is redundant, but it is needed if you're dealing with multiple renderbuffers.
            // Note, replace "_colorRenderbuffer" with the actual name of the renderbuffer object defined in your class.
			gl.BindRenderbuffer (All.RenderbufferOes, Renderbuffer);

            // Get the size of the backing CAEAGLLayer
			gl.GetRenderbufferParameter (All.RenderbufferOes, All.RenderbufferWidthOes, ref backingWidth);
			gl.GetRenderbufferParameter (All.RenderbufferOes, All.RenderbufferHeightOes, ref backingHeight);

            int width = backingWidth, height = backingHeight;
            int dataLength = width * height * 4;
            byte[] data = new byte [dataLength];

            // Read pixel data from the framebuffer
            gl.PixelStore (All.PackAlignment, 4);
            gl.ReadPixels (0, 0, width, height, All.Rgba, All.UnsignedByte, data);

            // Create a CGImage with the pixel data
            // If your OpenGL ES content is opaque, use kCGImageAlphaNoneSkipLast to ignore the alpha channel
            // otherwise, use kCGImageAlphaPremultipliedLast
            using (var data_provider = new CGDataProvider (data, 0, data.Length)) {
                using (var colorspace = CGColorSpace.CreateDeviceRGB ()) {
                    using (var iref = new CGImage (width, height, 8, 32, width * 4, colorspace, 
                                    (CGImageAlphaInfo) ((int) CGBitmapFlags.ByteOrder32Big | (int) CGImageAlphaInfo.PremultipliedLast),
                                    data_provider, null, true, CGColorRenderingIntent.Default)) {

                        // OpenGL ES measures data in PIXELS
                        // Create a graphics context with the target size measured in POINTS
                        int widthInPoints, heightInPoints;
						float scale = (float)ContentScaleFactor;
                        widthInPoints = (int) (width / scale);
                        heightInPoints = (int) (height / scale);
                        UIGraphics.BeginImageContextWithOptions (new System.Drawing.SizeF (widthInPoints, heightInPoints), false, scale);

                        try {
                            var cgcontext = UIGraphics.GetCurrentContext ();

                            // UIKit coordinate system is upside down to GL/Quartz coordinate system
                            // Flip the CGImage by rendering it to the flipped bitmap context
                            // The size of the destination area is measured in POINTS
                            cgcontext.SetBlendMode (CGBlendMode.Copy);
                            cgcontext.DrawImage (new System.Drawing.RectangleF (0, 0, widthInPoints, heightInPoints), iref);

                            // Retrieve the UIImage from the current context
                            var image = UIGraphics.GetImageFromCurrentImageContext ();

                            return image;
                        } finally {
                            UIGraphics.EndImageContext ();
                        }
                    }
                }
            }
        }

        public override void WillMoveToWindow(UIWindow window)
        {
            if (window == null && !suspended)
                Suspend();
            else if (window != null && suspended) {
                if (frameBufferLayer != null && ((CALayer)frameBufferLayer.Target) != Layer ||
                    frameBufferWindow != null && ((UIWindow)frameBufferWindow.Target) != window) {

                    DestroyFrameBuffer();
                    CreateFrameBuffer ();
                }

                Resume ();
            }
        }

        FrameEventArgs updateEventArgs = new FrameEventArgs();
        FrameEventArgs renderEventArgs = new FrameEventArgs();

		internal void RunIteration (NSTimer timer)
        {
            var curUpdateTime = stopwatch.Elapsed;
            if (prevUpdateTime == TimeSpan.Zero)
                prevUpdateTime = curUpdateTime;
            var t = (curUpdateTime - prevUpdateTime).TotalSeconds;
            updateEventArgs.Time = t;
            OnUpdateFrame(updateEventArgs);
            prevUpdateTime = curUpdateTime;

			gl.BindFramebuffer(All.FramebufferOes, framebuffer);

            var curRenderTime = stopwatch.Elapsed;
            if (prevRenderTime == TimeSpan.Zero)
                prevRenderTime = curRenderTime;
            t = (curRenderTime - prevRenderTime).TotalSeconds;
            renderEventArgs.Time = t;
            OnRenderFrame(renderEventArgs);
            prevRenderTime = curRenderTime;
        }

        protected virtual void OnLoad(EventArgs e)
        {
            var h = Load;
            if (h != null)
                h (this, e);
        }

        protected virtual void OnUnload(EventArgs e)
        {
            var h = Unload;
            DestroyFrameBuffer();
            if (h != null)
                h (this, e);
        }

        protected virtual void OnUpdateFrame(FrameEventArgs e)
        {
            var h = UpdateFrame;
            if (h != null)
                h (this, e);
        }

        protected virtual void OnRenderFrame(FrameEventArgs e)
        {
            var h = RenderFrame;
            if (h != null)
                h (this, e);
        }

        public event EventHandler<EventArgs> Resize;
        public event EventHandler<EventArgs> Closed;
        public event EventHandler<EventArgs> Disposed;
        public event EventHandler<EventArgs> TitleChanged;
        public event EventHandler<EventArgs> VisibleChanged;
		public event EventHandler<EventArgs> WindowStateChanged;
        public event EventHandler<EventArgs> Load;
        public event EventHandler<EventArgs> Unload;
        public event EventHandler<FrameEventArgs> UpdateFrame;
        public event EventHandler<FrameEventArgs> RenderFrame;
    }
}

// vim: et ts=4 sw=4
#endif
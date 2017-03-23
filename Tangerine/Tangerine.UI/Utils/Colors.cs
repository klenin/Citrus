using System;
using Lime;

namespace Tangerine.UI
{
	public class ColorTheme
	{
		public class ToolbarColors
		{
			public Color4 ButtonHighlightBorder;
			public Color4 ButtonHighlightBackground;
			public Color4 ButtonPressBorder;
			public Color4 ButtonPressBackground;
			public Color4 ButtonCheckedBorder;
			public Color4 ButtonCheckedBackground;
			public Color4 ButtonDisabledColor;
			public Color4 Background;
			public Color4 Border;
		}

		public class SceneViewColors
		{
			public Color4 Selection;
			public Color4 LockedWidgetBorder;
			public Color4 ExposedItemInactiveBorder;
			public Color4 ExposedItemActiveBorder;
			public Color4 ExposedItemSelectedBorder;
			public Color4 Label;
			public Color4 MouseSelection;
			public Color4 ContainerOuterSpace;
			public Color4 ContainerInnerSpace;
			public Color4 ContainerBorder;
			public Color4 PointObject;
		}

		public class TimelineGridColors
		{
			public Color4 PropertyRowBackground;
			public Color4 Lines;
			public Color4 Selection;
			public Color4 Cursor;
			public Color4 RunningCursor;
		}

		public class TimelineRulerColors
		{
			public Color4 Notchings;
			public Color4 JumpMarker;
			public Color4 PlayMarker;
			public Color4 StopMarker;
			public Color4 UnknownMarker;
			public Color4 Cursor;
			public Color4 RunningCursor;
		}

		public class TimelineOverviewColors
		{
			public Color4 Veil;
			public Color4 Border;
		}

		public class TimelineRollColors
		{
			public Color4 Lines;
			public Color4 GrayedLabel;
			public Color4 DragCursor;
		}

		public class DockingColors
		{
			public Color4 DragRectagleOutline;
			public Color4 PanelTitleBackground;
			public Color4 PanelTitleSeparator;
		}

		public class InspectorColors
		{
			public Color4 BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder;
			public Color4 CategoryLabelBackground = Color4.Black.Lighten(0.13f);
		}

		public DesktopTheme.ColorTheme Basic;
		public ToolbarColors Toolbar;
		public SceneViewColors SceneView;
		public TimelineGridColors TimelineGrid;
		public TimelineRulerColors TimelineRuler;
		public TimelineOverviewColors TimelineOverview;
		public TimelineRollColors TimelineRoll;
		public DockingColors Docking;
		public InspectorColors Inspector;

		public static ColorTheme Current = CreateLightTheme();

		public static ColorTheme CreateDarkTheme()
		{
			var toolbuttonHighlightBorder = DesktopTheme.Colors.KeyboardFocusBorder.Lighten(0.2f);
			var toolbuttonHighlightBackground = DesktopTheme.Colors.KeyboardFocusBorder.Darken(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Lighten(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Lighten(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Lighten(0.1f),
				Background = DesktopTheme.Colors.GrayBackground,
				Border = DesktopTheme.Colors.SeparatorColor
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Blue
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = DesktopTheme.Colors.GrayBackground.Darken(0.5f),
				Lines = new Color4(45, 45, 48),
				Selection = Color4.Gray.Transparentify(0.5f),
				Cursor = new Color4(163, 0, 0).Darken(0.15f),
				RunningCursor = new Color4(0, 163, 0).Darken(0.15f)
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = new Color4(209, 206, 0),
				PlayMarker = new Color4(0, 163, 0),
				StopMarker = new Color4(163, 0, 0),
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.6f),
				Border = Color4.White.Darken(0.2f)
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = DesktopTheme.Colors.BlackText.Darken(0.5f),
				DragCursor = new Color4(254, 170, 24)
			};
			var docking = new DockingColors {
				DragRectagleOutline = new Color4(51, 51, 255),
				PanelTitleBackground = DesktopTheme.Colors.GrayBackground.Lighten(0.1f),
				PanelTitleSeparator = DesktopTheme.Colors.GrayBackground.Lighten(0.15f)
			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder,
				CategoryLabelBackground = Color4.Black.Lighten(0.13f)
			};
			return new ColorTheme {
				Basic = DesktopTheme.Colors,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector
			};
		}

		public static ColorTheme CreateLightTheme()
		{
			var toolbuttonHighlightBorder = DesktopTheme.Colors.KeyboardFocusBorder.Darken(0.2f);
			var toolbuttonHighlightBackground = DesktopTheme.Colors.KeyboardFocusBorder.Lighten(0.3f);
			var toolbar = new ToolbarColors {
				ButtonHighlightBorder = toolbuttonHighlightBorder,
				ButtonHighlightBackground = toolbuttonHighlightBackground,
				ButtonPressBorder = toolbuttonHighlightBorder,
				ButtonPressBackground = toolbuttonHighlightBackground.Darken(0.1f),
				ButtonCheckedBorder = toolbuttonHighlightBorder.Darken(0.1f),
				ButtonCheckedBackground = toolbuttonHighlightBackground.Transparentify(0.5f),
				ButtonDisabledColor = Color4.Gray.Darken(0.1f),
				Background = DesktopTheme.Colors.GrayBackground,
				Border = DesktopTheme.Colors.SeparatorColor
			};
			var sceneView = new SceneViewColors {
				Selection = Color4.Green,
				LockedWidgetBorder = Color4.FromFloats(0, 1, 1),
				ExposedItemInactiveBorder = Color4.Gray,
				ExposedItemActiveBorder = Color4.White,
				ExposedItemSelectedBorder = Color4.Green,
				Label = Color4.Green,
				MouseSelection = Color4.Yellow,
				ContainerOuterSpace = Color4.Gray,
				ContainerInnerSpace = Color4.White,
				ContainerBorder = Color4.Blue,
				PointObject = Color4.Blue
			};
			var timelineGrid = new TimelineGridColors {
				PropertyRowBackground = DesktopTheme.Colors.GrayBackground.Lighten(0.5f),
				Lines = Color4.White.Darken(0.25f),
				Selection = Color4.Gray.Transparentify(0.5f),
				Cursor = Color4.Red.Lighten(0.4f),
				RunningCursor = Color4.Green.Lighten(0.4f)
			};
			var timelineRuler = new TimelineRulerColors {
				Notchings = timelineGrid.Lines,
				JumpMarker = Color4.Yellow,
				PlayMarker = Color4.Green,
				StopMarker = Color4.Red,
				UnknownMarker = Color4.Black,
				Cursor = timelineGrid.Cursor,
				RunningCursor = timelineGrid.RunningCursor
			};
			var timelineOverview = new TimelineOverviewColors {
				Veil = Color4.White.Darken(0.2f).Transparentify(0.3f),
				Border = Color4.White.Darken(0.2f)
			};
			var timelineRoll = new TimelineRollColors {
				Lines = timelineGrid.Lines,
				GrayedLabel = DesktopTheme.Colors.BlackText.Lighten(0.5f),
				DragCursor = Color4.Black
			};
			var docking = new DockingColors {
				DragRectagleOutline = Color4.FromFloats(0.2f, 0.2f, 1f),
				PanelTitleBackground = DesktopTheme.Colors.GrayBackground.Darken(0.1f),
				PanelTitleSeparator = DesktopTheme.Colors.GrayBackground.Darken(0.15f)
			};
			var inspector = new InspectorColors {
				BorderAroundKeyframeColorbox = DesktopTheme.Colors.ControlBorder,
				CategoryLabelBackground = Color4.White.Darken(0.13f)
			};
			return new ColorTheme {
				Basic = DesktopTheme.Colors,
				Toolbar = toolbar,
				SceneView = sceneView,
				TimelineGrid = timelineGrid,
				TimelineRuler = timelineRuler,
				TimelineOverview = timelineOverview,
				TimelineRoll = timelineRoll,
				Docking = docking,
				Inspector = inspector
			};
		}
	}
}

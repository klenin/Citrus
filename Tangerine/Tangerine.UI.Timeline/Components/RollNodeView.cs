using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;

namespace Tangerine.UI.Timeline.Components
{
	public class RollNodeView : IRollWidget
	{
		protected readonly Row row;
		protected readonly NodeRow nodeData;
		protected readonly SimpleText label;
		protected readonly EditBox editBox;
		protected readonly Image nodeIcon;
		protected readonly Widget widget;
		protected readonly ToolbarButton enterButton;
		protected readonly ToolbarButton expandButton;
		protected readonly ToolbarButton eyeButton;
		protected readonly ToolbarButton lockButton;
		readonly Widget spacer;

		public RollNodeView(Row row)
		{
			this.row = row;
			nodeData = row.Components.Get<NodeRow>();
			label = new SimpleText();
			editBox = new EditBox { LayoutCell = new LayoutCell(Alignment.Center, stretchX: float.MaxValue) };
			nodeIcon = new Image(NodeIconPool.GetTexture(nodeData.Node.GetType())) {
				HitTestTarget = true,
				MinMaxSize = new Vector2(16)
			};
			expandButton = CreateExpandButton();
			var expandButtonContainer = new Widget {
				Layout = new StackLayout { IgnoreHidden = false },
				LayoutCell = new LayoutCell(Alignment.Center),
				Nodes = { expandButton }
			};
			expandButtonContainer.Updating += delta => 
				expandButtonContainer.Visible = nodeData.Node.Animators.Count > 0;
			enterButton = (ClassAttributes<TangerineClassAttribute>.Get(nodeData.Node.GetType())?.
				AllowChildren ?? false) ? CreateEnterButton() : null;
			eyeButton = CreateEyeButton();
			lockButton = CreateLockButton();
			widget = new Widget {
				Padding = new Thickness { Left = 4, Right = 2 },
				MinHeight = TimelineMetrics.DefaultRowHeight,
				Layout = new HBoxLayout { CellDefaults = new LayoutCell(Alignment.Center) },
				HitTestTarget = true,
				Nodes = {
					(spacer = new Widget()),
					expandButtonContainer,
					nodeIcon,
					new HSpacer(3),
					label,
					editBox,
					new Widget(),
					(Widget)enterButton ?? (Widget)new HSpacer(ToolbarButton.DefaultSize.X),
					eyeButton,
					lockButton,
				},
			};
			label.AddChangeWatcher(() => nodeData.Node.Id, s => RefreshLabel());
			label.AddChangeWatcher(() => IsGrayedLabel(nodeData.Node), s => RefreshLabel());
			label.AddChangeWatcher(() => nodeData.Node.ContentsPath, s => RefreshLabel());
			widget.CompoundPresenter.Push(new DelegatePresenter<Widget>(RenderBackground));
			editBox.Visible = false;
			widget.Tasks.Add(HandleDobleClickTask());
		}

		Widget IRollWidget.Widget => widget;
		float IRollWidget.Indentation { set { spacer.MinMaxWidth = value; } }

		bool IsGrayedLabel(Node node) => node.AsWidget?.Visible ?? true;

		void RefreshLabel()
		{
			var node = nodeData.Node;
			if (!string.IsNullOrEmpty(node.ContentsPath)) {
				label.Text = $"{node.Id} [{node.ContentsPath}]";
			} else {
				label.Text = node.Id;
			}
			label.Color = IsGrayedLabel(node) ? DesktopTheme.Colors.BlackText : TimelineRollColors.GrayedLabel;
		}
		
		ToolbarButton CreateEnterButton()
		{
			var button = new ToolbarButton {
				Texture = IconPool.GetTexture("Timeline.EnterContainer"),
				Highlightable = false
			};
			button.Clicked += () => Core.Operations.EnterNode.Perform(nodeData.Node);
			return button;
		}

		ToolbarButton CreateEyeButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(() => nodeData.Visibility, i => {
				var texture = "Timeline.Dot";
				if (i == NodeVisibility.Shown) {
					texture = "Timeline.Eye";
				} else if (i == NodeVisibility.Hidden) {
					texture = "Timeline.Cross";
				}
				button.Texture = IconPool.GetTexture(texture);
			});
			button.Clicked += () => {
				Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Visibility), (NodeVisibility)(((int)nodeData.Visibility + 1) % 3));
			};
			return button;
		}

		ToolbarButton CreateLockButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Locked, 
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Lock" : "Timeline.Dot")
			);
			button.Clicked += () => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Locked), !nodeData.Locked);
			return button;
		}

		ToolbarButton CreateExpandButton()
		{
			var button = new ToolbarButton { Highlightable = false };
			button.AddChangeWatcher(
				() => nodeData.Expanded,
				i => button.Texture = IconPool.GetTexture(i ? "Timeline.Expanded" : "Timeline.Collapsed")
			);
			button.Clicked += () => Core.Operations.SetProperty.Perform(nodeData, nameof(NodeRow.Expanded), !nodeData.Expanded);
			return button;
		}

		void RenderBackground(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, row.Selected ? Colors.SelectedBackground : Colors.WhiteBackground);
		}

		IEnumerator<object> HandleDobleClickTask()
		{
			while (true) {
				if (nodeIcon.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.ClearRowSelection.Perform();
					Core.Operations.SelectRow.Perform(row);
					label.Visible = false;
					editBox.Visible = true;
					editBox.Text = nodeData.Node.Id;
					editBox.SetFocus();
					editBox.Tasks.Add(EditNodeIdTask());
				} else if (widget.Input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					Core.Operations.EnterNode.Perform(nodeData.Node);
				}
				yield return null;
			}
		}

		IEnumerator<object> EditNodeIdTask()
		{
			var initialText = editBox.Text;
			while (editBox.IsFocused()) {
				yield return null;
				if (!row.Selected) {
					editBox.RevokeFocus();
				}
			}
			editBox.Visible = false;
			label.Visible = true;
			if (editBox.Text != initialText) {
				Core.Operations.SetProperty.Perform(nodeData.Node, "Id", editBox.Text);
			}
		}
	}
}

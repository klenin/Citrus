﻿using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreatePointObjectProcessor : ITaskProvider
	{
		SceneView sv => SceneView.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				Type nodeType;
				if (CreateNodeRequestComponent.Consume<PointObject>(sv.Components, out nodeType)) {
					yield return CreatePointObjectTask(nodeType);
				}
				yield return null;
			}
		}

		IEnumerator<object> CreatePointObjectTask(Type nodeType)
		{
			while (true) {
				if (sv.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(sv.Components);
				if (sv.Input.WasMousePressed()) {
					try {
						var currentPoint = (PointObject)Core.Operations.CreateNode.Perform(nodeType, aboveSelected: nodeType != typeof(SplinePoint));
						var container = (Widget)Document.Current.Container;
						var t = sv.Scene.CalcTransitionToSpaceOf(container);
						var pos = Vector2.Zero;
						if (container.Width.Abs() > Mathf.ZeroTolerance && container.Height.Abs() > Mathf.ZeroTolerance) {
							pos = sv.MousePosition * t / container.Size;
						}
						Core.Operations.SetProperty.Perform(currentPoint, nameof(PointObject.Position), pos);
					} catch (InvalidOperationException e) {
						AlertDialog.Show(e.Message);
						break;
					}
				}
				if (sv.Input.WasMousePressed(1)) {
					break;
				}
				yield return null;
			}
		}
	}
}
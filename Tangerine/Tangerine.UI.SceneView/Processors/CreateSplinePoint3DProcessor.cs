﻿using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.SceneView
{
	public class CreateSplinePoint3DProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			while (true) {
				if (CreateNodeRequestComponent.Consume<SplinePoint3D>(SceneView.Instance.Components)) {
					yield return CreateSplinePoint3DTask();
				}
				yield return null;
			}
		}

		IEnumerator<object> CreateSplinePoint3DTask()
		{
			while (true) {
				if (SceneView.Instance.InputArea.IsMouseOver()) {
					Utils.ChangeCursorIfDefault(MouseCursor.Hand);
				}
				CreateNodeRequestComponent.Consume<Node>(SceneView.Instance.Components);
				if (SceneView.Instance.Input.WasMousePressed()) {
					var spline = (Spline3D)Document.Current.Container;
					var vp = spline.GetViewport();
					var ray = vp.ScreenPointToRay(SceneView.Instance.Input.MousePosition);
					var xyPlane = new Plane(new Vector3(0, 0, 1), 0).Transform(spline.GlobalTransform);
					var d = ray.Intersects(xyPlane);
					if (d.HasValue) {
						var pos = (ray.Position + ray.Direction * d.Value) * spline.GlobalTransform.CalcInverted();
						var point = (SplinePoint3D)Core.Operations.CreateNode.Perform(typeof(SplinePoint3D));
						Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.Position), pos);
						Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentA), new Vector3(1, 0, 0));
						Core.Operations.SetProperty.Perform(point, nameof(SplinePoint3D.TangentB), new Vector3(-1, 0, 0));
					}
				}
				if (SceneView.Instance.Input.WasMousePressed(1)) {
					break;
				}
				yield return null;
			}
			Utils.ChangeCursorIfDefault(MouseCursor.Default);
		}
	}
}

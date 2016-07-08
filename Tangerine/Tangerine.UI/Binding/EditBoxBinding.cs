using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI
{
	public class EditBoxBinding : IProcessor
	{
		readonly EditBox editBox;
		readonly IDataflowProvider<string> source;

		public EditBoxBinding(EditBox editBox, IDataflowProvider<string> source)
		{
			this.source = source;
			this.editBox = editBox;
		}

		public IEnumerator<object> Loop()
		{
			var dataflow = source.GetDataflow();
			while (true) {
				dataflow.Poll();
				if (!editBox.IsFocused()) {
					editBox.Text = dataflow.Value;
				}
				yield return null;
			}
		}
	}
}
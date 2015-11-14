﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lime
{
	public class CommonTaskProfiler: ITaskProfiler
	{
		private Dictionary<Type, ProfileEntry> profile = new Dictionary<Type, ProfileEntry>();
		private MemoryStopwatch memoryStopwatch = new MemoryStopwatch();
		public long TotalTasksUpdated;

		public void RegisterTask(IEnumerator<object> enumerator)
		{
			var type = enumerator.GetType();
			ProfileEntry pe;
			profile.TryGetValue(type, out pe);
			pe.TaskCount++;
			profile[type] = pe;
		}

		public void BeforeAdvance(IEnumerator<object> enumerator)
		{
			TotalTasksUpdated++;
			memoryStopwatch.Start();
		}

		public void AfterAdvance(IEnumerator<object> enumerator)
		{
			memoryStopwatch.Stop();
			var type = enumerator.GetType();
			ProfileEntry pe;
			profile.TryGetValue(type, out pe);
			pe.CallCount++;
			if (memoryStopwatch.MemoryAllocated > 0) {
				pe.MemoryAllocated += memoryStopwatch.MemoryAllocated;
			}
			profile[type] = pe;
		}

		public void DumpProfile(TextWriter writer)
		{
			var items = profile.Select(p => new {
				Method = p.Key.ToString(),
				Memory = p.Value.MemoryAllocated,
				CallCount = p.Value.CallCount,
				TaskCount = p.Value.TaskCount,
			}).OrderByDescending(a => a.Memory);
			writer.WriteLine("Memory allocated\tCall count\tMethod Name");
			writer.WriteLine("===================================================================================================");
			foreach (var i in items) {
				writer.WriteLine("{0:N0}\t\t\t{1:N0}\t\t{2}\t\t{3}", i.Memory, i.CallCount, i.TaskCount, i.Method);
			}
		}

		public bool IsNull
		{
			get { return false; }
		}

		private struct ProfileEntry
		{
			public long MemoryAllocated;
			public int CallCount;
			public int TaskCount;
		}

		private class MemoryStopwatch
		{
			private long startingMemoryAllocation;
			public long MemoryAllocated { get; private set; }
			public bool Stopped { get; private set; }

			public MemoryStopwatch()
			{
				Stopped = true;
			}

			public void Start()
			{
				Stopped = false;
				startingMemoryAllocation = GC.GetTotalMemory(false);
			}

			public void Stop()
			{
				if (Stopped) {
					throw new InvalidOperationException("Cannot call Stop() before Start() or twice in a row.");
				}
				MemoryAllocated = GC.GetTotalMemory(false) - startingMemoryAllocation;
				Stopped = true;
			}
		}
	}
}
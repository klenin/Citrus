using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public sealed class TriggerAttribute : Attribute {}

	public sealed class AnimatorCollection : ICollection<IAnimator>, IDisposable
	{
		private Node owner;

		public IAnimator First { get; private set; }
		public int Count { get; private set; }
		public int Version { get; private set; }

		public AnimatorCollection(Node owner)
		{
			this.owner = owner;
		}

		public void Dispose()
		{
			foreach (var a in this) {
				a.Dispose();
			}
			Clear();
		}

		public void Add(IAnimator item)
		{
			if (item.Owner != null) {
				throw new InvalidOperationException();
			}
			item.Owner = owner;
			if (First == null) {
				First = item;
			} else {
				var a = First;
				while (a.Next != null) {
					a = a.Next;
				}
				a.Next = item;
			}
			item.Bind(owner);
			Animation animation;
			if (owner.Animations.TryFind(item.AnimationId, out animation)) {
				animation.NextMarkerOrTriggerTime = null;
			}
			Version++;
			Count++;
		}

		public void AddRange(IEnumerable<IAnimator> collection)
		{
			foreach (var a in collection) {
				Add(a);
			}
		}

		public void Clear()
		{
			for (var a = First; a != null; ) {
				var t = a.Next;
				a.Owner = null;
				a.Next = null;
				a = t;
			}
			First = null;
			Count = 0;
			Version++;
		}

		internal static AnimatorCollection SharedClone(Node owner, AnimatorCollection source)
		{
			var result = new AnimatorCollection(owner);
			foreach (var animator in source) {
				result.Add(animator.Clone());
			}
			return result;
		}

		public bool TryFind(string propertyName, out IAnimator animator, string animationId = null)
		{
			for (var a = First; a != null; a = a.Next) {
				if (a.TargetProperty == propertyName && a.AnimationId == animationId) {
					animator = a;
					return true;
				}
			}
			animator = null;
			return false;
		}

		public bool TryFind<T>(string propertyName, out Animator<T> animator, string animationId = null)
		{
			IAnimator a;
			TryFind(propertyName, out a, animationId);
			animator = a as Animator<T>;
			return animator != null;
		}

		public IAnimator this[string propertyName, string animationId = null]
		{
			get
			{
				IAnimator animator;
				if (TryFind(propertyName, out animator, animationId)) {
					return animator;
				}
				PropertyInfo pi = owner.GetType().GetProperty(propertyName);
				if (pi == null) {
					throw new Lime.Exception("Unknown property {0} in {1}", propertyName, owner.GetType().Name);
				}
				animator = AnimatorRegistry.Instance.CreateAnimator(pi.PropertyType);
				animator.TargetProperty = propertyName;
				animator.AnimationId = animationId;
				Add(animator);
				Version++;
				return animator;
			}
		}

		public bool Contains(IAnimator item)
		{
			for (var a = First; a != null; a = a.Next) {
				if (a == item) {
					return true;
				}
			}
			return false;
		}

		void ICollection<IAnimator>.CopyTo(IAnimator[] array, int index)
		{
			for (var a = First; a != null; a = a.Next) {
				array[index++] = a;
			}
		}

		bool ICollection<IAnimator>.IsReadOnly => false;

		public bool Remove(IAnimator item)
		{
			if (item.Owner != owner) {
				throw new InvalidOperationException();
			}
			IAnimator prev = null;
			for (var a = First; a != null; a = a.Next) {
				if (a == item) {
					if (prev == null) {
						First = a.Next;
					} else {
						prev.Next = a.Next;
					}
					a.Owner = null;
					a.Next = null;
					Count--;
					Version++;
					return true;
				}
				prev = a;
			}
			return false;
		}

		public bool Remove(string propertyName, string animationId = null)
		{
			IAnimator animator;
			if (TryFind(propertyName, out animator, animationId)) {
				return Remove(animator);
			}
			return false;
		}

		public int GetOverallDuration()
		{
			int val = 0;
			for (var a = First; a != null; a = a.Next) {
				val = Math.Max(val, a.Duration);
			}
			return val;
		}

		public void Apply(double time, string animationId = null)
		{
			for (var a = First; a != null; a = a.Next) {
				if (a.AnimationId == animationId) {
					a.Apply(time);
				}
			}
		}

		public void InvokeTriggers(int frame, double animationTimeCorrection = 0)
		{
			for (var a = First; a != null; a = a.Next) {
				if (a.IsTriggerable) {
					a.InvokeTrigger(frame, animationTimeCorrection);
				}
			}
		}

		IEnumerator<IAnimator> IEnumerable<IAnimator>.GetEnumerator() => new Enumerator(First);
		IEnumerator IEnumerable.GetEnumerator() => new Enumerator(First);

		public Enumerator GetEnumerator() => new Enumerator(First);

		public struct Enumerator : IEnumerator<IAnimator>
		{
			private IAnimator first;
			private IAnimator current;

			public Enumerator(IAnimator first)
			{
				this.first = first;
				current = null;
			}

			object IEnumerator.Current => current;

			public bool MoveNext()
			{
				if (current == null) {
					current = first;
				} else {
					current = current.Next;
				}
				return current != null;
			}

			public void Reset() => current = null;

			public IAnimator Current => current;

			public void Dispose() { }
		}
	}
}

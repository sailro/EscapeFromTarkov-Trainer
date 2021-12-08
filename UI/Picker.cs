namespace EFT.Trainer.UI
{
	public abstract class Picker<T>
	{
		protected T _value;
		public T Value => _value;
		public abstract bool IsSelected { get; protected set; }

		protected Picker(T value)
		{
			_value = value;
		}

		public abstract void SetWindowPosition(float x, float y);
		public abstract void DrawWindow(int id, string title);
	}
}

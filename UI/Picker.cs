namespace EFT.Trainer.UI;

public interface IPicker
{
	public bool IsSelected { get; set; }
	public object RawValue { get; set; }

	public void SetWindowPosition(float x, float y);
	public void DrawWindow(int id, string title);
}

public abstract class Picker<T>(T value) : IPicker
{
	public object RawValue { get; set; } = value!;
	public T Value
	{
		get => (T)RawValue;
		set => RawValue = value!;
	}

	public abstract bool IsSelected { get; set; }
	public abstract void SetWindowPosition(float x, float y);
	public abstract void DrawWindow(int id, string title);
}

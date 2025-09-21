using Godot;

[Tool]
public partial class OptionsCategory : Container
{
	private string _categoryName;

    [Export]
    public string CategoryName
    {
        get => _categoryName;
		set
		{
			_categoryName = value;
			RefreshCategoryName();
        }
    }
	private bool _ready;
	public override void _Ready()
	{
		_ready = true;
		RefreshCategoryName();
    }

	private void RefreshCategoryName()
	{
		if (!_ready) return;
		var c = GetChild<Control>(0);
		if (c != null)
		{
			var l = c.GetChild<Label>(1);
			if (l != null) l.Text = _categoryName;
		}
	}
}

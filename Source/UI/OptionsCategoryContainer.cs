using Godot;

public partial class OptionsCategoryContainer : ScrollContainer
{
	Control _lastFocused = null;
	public override void _Process(double delta)
	{
		if (!IsVisibleInTree()) return;
		Container container = GetChild<Container>(0);
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != _lastFocused)
		{
			_lastFocused = focused;
			if (focused == null) return;
			if (focused.GetParent() == container) FollowElement(focused);
			else
			{
				do focused = focused.GetParent<Control>();
				while (focused.GetParent() != container || focused.GetParent() == null);
				if (focused.GetParent() == container) FollowElement(focused);
			}
		}
	}
	private void FollowElement(Control e)
	{
		//
		EnsureControlVisible(e);
	}
}

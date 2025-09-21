using System;
using System.Security.AccessControl;
using Godot;

[Tool]
public partial class OptionEntry : Button
{
	private string _entryName;
	[Export]
	private string EntryName
	{
		get { return _entryName; }
		set
		{
			_entryName = value;
			RefreshEntryName();
		}
	}
	private int _value;
	[Export]
	public int Value
	{
		get { return _value; }
		set
		{
			_value = value;
			ClampValue();
			BuildValueString();
		}
	}
	private int _minValue;
	[Export]
	private int MinValue
	{
		get { return _minValue; }
		set
		{
			_minValue = value;
			ClampValue();
			BuildValueString();
		}
	}
	private int _maxValue;
	[Export]
	private int MaxValue
	{
		get { return _maxValue; }
		set
		{
			_maxValue = value;
			ClampValue();
			BuildValueString();
		}
	}
	private string[] _options;
	[Export]
	private string[] Options
	{
		get { return _options; }
		set
		{
			_options = value;
			ClampValue();
			BuildValueString();
		}
	}
	private string _icon;
	[Export]
	private string ValueIcon
	{
		get { return _icon; }
		set
		{
			_icon = value;
			BuildValueString();
		}
	}
	private string _numberFormat = " {0}";
	[Export]
	private string NumberFormat
	{
		get { return _numberFormat; }
		set { _numberFormat = value; BuildValueString(); }
	}
	//
	private void ClampValue()
	{
		if (_options != null && _options.Length > 0)
		{
			_value = Math.Clamp(_value, 0, _options.Length - 1);
		}
		else if (_minValue != _maxValue)
		{
			_value = Math.Clamp(_value, _minValue, _maxValue);
		}
	}
	private void BuildValueString()
	{
		if (!_ready) return;
		var rl = GetChild<RichTextLabel>(1);
		if (rl != null)
		{
			var s = "[right]";
			if (_options != null && _options.Length > 0)
			{
				for (int i = 0; i < _options.Length; i++)
				{
					if (i != 0) s += " ";
					if (i == _value) s += $"[color=fff]{_options[i]}";
					else s += $"[color=333]{_options[i]}";
				}
			}
			else // Simple value numeric display
			{
				if (_icon != null && _icon.Length > 0 && _maxValue != _minValue)
				{
					for (int i = _minValue; i < _maxValue; i++)
					{
						if (i == _value) s += "[color=333]";
						s += _icon;
					}
				}
				s += string.Format(_numberFormat, _value);
			}
			rl.Text = s;
		}
	}
	public void ChangeValue(int v)
	{
		Value += v;
		SetValue();
	}
	private void GetValueType()
	{
		_propertyType = Main.Settings.GetValueType(Name);
	}
	private void GetValue()
	{
		if (_propertyType == null) return;
		var v = Main.Settings.GetValue(Name);
		if (_propertyType == typeof(int))
		{
			Value = (int)v;
		}
		else if (_propertyType == typeof(bool))
		{
			Value = (bool)v ? 0 : 1;
		}
		else if (_propertyType == typeof(string))
		{
			var str = (string)v;
			if (Name == "Language")
			{
				var langs = TranslationServer.GetLoadedLocales();
				var idx = 0;
				for (int i = 0; i < langs.Length; i++)
				{
					if (str == langs[i])
					{
						idx = i;
						break;
					}
				}
				Value = idx;
			}
		}
	}
	private void SetValue()
	{
		if (_propertyType == typeof(int))
		{
			Main.Settings.SetValue(Name, Value);
		}
		else if (_propertyType == typeof(bool))
		{
			Main.Settings.SetValue(Name, Value == 0);
		}
		else if (_propertyType == typeof(string))
		{
			if (Name == "Language")
			{
				var langs = TranslationServer.GetLoadedLocales();
				Main.Settings.SetValue(Name, langs[Value]);
			}
		}
	}
	private void OnPress()
	{
		var v = Value;
		Value += 1;
		if (Value == v) Value = 0;
		SetValue();
	}
	private Type _propertyType;
	private bool _ready;
	public override void _Ready()
	{
		_ready = true;
		RefreshAll();
		if (Engine.IsEditorHint()) return;
		Setup();
	}
	void RefreshAll()
	{
		RefreshEntryName();
		BuildValueString();
	}
	private void RefreshEntryName()
	{
		if (!_ready) return;
		var l = GetChild<Label>(0);
		if (l != null) l.Text = _entryName;
	}
	public async void Setup()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		// load value
		GetValueType();
		if (_propertyType == null) return;
		GetValue();
		Pressed += OnPress;
	}
}

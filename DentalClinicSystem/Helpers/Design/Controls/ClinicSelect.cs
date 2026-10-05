using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DentalClinicSystem.Helpers.Design.Controls;

// AntdUI displays SelectItems; the clinic forms continue to read their original
// model objects and ID values. Rebinding preserves the selection by object/ID.
public sealed class ClinicSelect : AntdUI.Select
{
    private object? _source;
    private string _displayMember = "", _valueMember = "";
    private bool _binding;
    private object? _lastSelection;
    public ClinicSelect()
    {
        Items = new ChoiceCollection(this);
        List = true; Font = Typography.Body; Radius = Metrics.ControlRadius;
        Height = Metrics.ControlHeight; Width = Metrics.FormWidth;
        base.SelectedIndexChanged += (_, _) => NotifySelection();
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new ChoiceCollection Items { get; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? DataSource
    {
        get => _source;
        set
        {
            _source = value;
            var selected = SelectedItem;
            _binding = true;
            try { Items.Replace(value is IEnumerable rows ? rows.Cast<object>().ToArray() : []); }
            finally { _binding = false; }
            Rebuild(selected, selectFirst: true);
        }
    }
    [DefaultValue("")]
    public string DisplayMember { get => _displayMember; set { _displayMember = value; Rebuild(SelectedItem); } }
    [DefaultValue("")]
    public string ValueMember { get => _valueMember; set { _valueMember = value; Rebuild(SelectedItem); } }
    [DefaultValue(ComboBoxStyle.DropDownList)]
    public ComboBoxStyle DropDownStyle { get => List ? ComboBoxStyle.DropDownList : ComboBoxStyle.DropDown; set => List = value == ComboBoxStyle.DropDownList; }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public object? SelectedItem
    {
        get => base.SelectedIndex >= 0 && base.SelectedIndex < Items.Count ? Items[base.SelectedIndex] : null;
        set => base.SelectedIndex = Items.IndexOf(value!);
    }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public new object? SelectedValue
    {
        get => Member(SelectedItem, ValueMember);
        set => base.SelectedIndex = Items.ToList().FindIndex(item => Equals(Member(item, ValueMember), value));
    }
    public new event EventHandler? SelectedIndexChanged;
    public new event EventHandler? SelectedValueChanged;
    private static object? Member(object? item, string member) => item is null || member.Length == 0 ? item : TypeDescriptor.GetProperties(item)[member]?.GetValue(item);
    private void NotifySelection()
    {
        if (_binding || ReferenceEquals(_lastSelection, SelectedItem)) return;
        _lastSelection = SelectedItem;
        SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        SelectedValueChanged?.Invoke(this, EventArgs.Empty);
    }
    private void Rebuild(object? selected = null, bool selectFirst = false)
    {
        if (_binding) return;
        var current = selected ?? SelectedItem;
        _binding = true;
        try
        {
            // AntdUI caches the selected label. Reset the index so changing
            // DisplayMember after DataSource also refreshes the visible text.
            base.SelectedIndex = -1;
            base.Items.Clear();
            base.Items.AddRange(Items.Select(item => (object)new AntdUI.SelectItem(Member(item, DisplayMember)?.ToString() ?? item.ToString() ?? "", item)).ToArray());
            var index = Items.IndexOf(current!);
            if (index < 0 && current is not null && ValueMember.Length > 0)
                index = Items.ToList().FindIndex(item => Equals(Member(item, ValueMember), Member(current, ValueMember)));
            base.SelectedIndex = index >= 0 ? index : selectFirst && Items.Count > 0 ? 0 : -1;
            if (base.SelectedIndex < 0) Text = "";
        }
        finally { _binding = false; }
        NotifySelection();
    }
    public sealed class ChoiceCollection(ClinicSelect owner) : Collection<object>
    {
        public void AddRange(object[] items) { foreach (var item in items) Items.Add(item); owner.Rebuild(); }
        internal void Replace(object[] items) { Items.Clear(); foreach (var item in items) Items.Add(item); }
        protected override void InsertItem(int index, object item) { base.InsertItem(index, item); owner.Rebuild(); }
        protected override void RemoveItem(int index) { base.RemoveItem(index); owner.Rebuild(); }
        protected override void SetItem(int index, object item) { base.SetItem(index, item); owner.Rebuild(); }
        protected override void ClearItems() { base.ClearItems(); owner.Rebuild(); }
    }
}

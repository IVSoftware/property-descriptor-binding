using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace variable_column_dgv
{
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            dataGridView.DataSource = Days;
            dataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            var numCol = dataGridView.Columns[0];
            numCol.Frozen = true;
            numCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            }
            // Fill column(cosmetic)
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            });
            Days.Add(new Day { Number = 1 });            
            Days.Add(new Day { Number = 2 });            
            Days.Add(new Day { Number = 3 });            
            Days.Add(new Day { Number = 4 });

            Days[3].Number = 44;
            Days[0].Hours[9].Value = 99;
        }
        DayList Days { get; } = new DayList();
    }
    class PropertyInfoDescriptor : PropertyDescriptor
    {
        public PropertyInfoDescriptor(string name) : base(name, new Attribute[0]) { }
        public override Type ComponentType => throw new NotImplementedException();
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(int);
        public override bool CanResetValue(object component) => true;
        public override void ResetValue(object component)
        {
            if(component is Day day)
            {

            }
        }
        Regex _getIndex = new Regex(@"\[(\d+)\]");
        public override object? GetValue(object? component)
        {
            if(component is Day day)
            {
                switch (Name)
                {
                    case nameof(Day.Number): return day.Number;
                    default:
                        var index = 
                            Convert.ToInt32(_getIndex.Match(Name).Groups[1].Value);
                        return day.Hours[index].Value;
                }
            }
            return default;
        }
        public override void SetValue(object? component, object? value)
        {
            if(component is Day day)
            {
                switch (Name)
                {
                    case nameof(Day.Number): day.Number = Convert.ToInt32(value); break;
                    default:
                        var index =
                            Convert.ToInt32(_getIndex.Match(Name).Groups[1].Value);
                        day.Hours[index].Value = Convert.ToInt32(value);
                        break;
                }
            }
        }
        public override bool ShouldSerializeValue(object component) => true;
    }

    class DayList : BindingList<Day>, ITypedList
    {
        public DayList()
        {
            Columns = new BindingList<PropertyInfoDescriptor>();
            Columns.Add(new PropertyInfoDescriptor(nameof(Day.Number)));

            for (int i = 0; i < 24; i++)
            {
                var name = $"Hour[{i:d2}]";
                Columns.Add(new PropertyInfoDescriptor(name));
            }
        }
        private BindingList<PropertyInfoDescriptor> Columns { get; } 
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[]? listAccessors)
        {
            if (listAccessors is PropertyDescriptor[] accessors && accessors.Any())
            {
                var builder = new List<PropertyInfoDescriptor>();
                foreach (var item in accessors)
                {
                    if(builder.FirstOrDefault(_=>_.Name == item.Name) is PropertyInfoDescriptor pid)
                    {
                        builder.Add(pid);
                    }
                }
                return new PropertyDescriptorCollection(builder.ToArray());
            }
            else
            {
                return new PropertyDescriptorCollection(Columns.ToArray());
            }
        }
        public string GetListName(PropertyDescriptor[]? listAccessors) => _name;
        static int _instanceCount = 0;
        string _name = $"List{_instanceCount++}";
    }


    public class Hour : INotifyPropertyChanged
    {
        public Hour(string propertyName) => PropertyName = propertyName;
        public int Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _value = value;
                    OnPropertyChanged(PropertyName);
                }
            }
        }
        public string PropertyName { get; }

        int _value = default;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Day : INotifyPropertyChanged
    {
        private static Random _rando = new Random(1);
        public Day()
        {
            var builder = new List<Hour>();
            for (int i = 0; i < 24; i++)
            {
                var hour = new Hour($"Hour[{i:d2}]");
                builder.Add(hour);
            }
            Hours = builder.ToArray();
        }
        public int Number
        {
            get => _number;
            set
            {
                if (!Equals(_number, value))
                {
                    _number = value;
                    OnPropertyChanged();
                }
            }
        }
        int _number = default;

        public Hour[] Hours { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

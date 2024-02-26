# Property Descriptor Binding

Based on the go-ahead in your comment, here's a snippet that partially answers your question. A good place to start might be to finish out the implementation of your `ListItemDescriptor` class so that it maps column names like "Hour[00]" to the array object `Day.Hour[0]` in both read and write directions. 

[![poc screenshot][1]][1]

It could look something like this:

```csharp

class ListItemDescriptor : PropertyDescriptor
{
    public ListItemDescriptor(string name) : base(name, new Attribute[0]) { }
    public override Type ComponentType => typeof(Day);
    public override bool IsReadOnly => false;
    public override Type PropertyType => typeof(int);
    public override bool CanResetValue(object component) => true;
    public override void ResetValue(object component)
    {
        if(component is Day day)
        {
            switch (Name)
            {
                case nameof(Day.Number): 
                    day.Number = default; 
                    break;
                default:
                    var index =
                        Convert.ToInt32(_getIndex.Match(Name).Groups[1].Value);
                    day.Hours[index].Value = default; 
                    break;
            }
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
```

In this POC I tried the two test cases  but it will take more if columns are going to come and go. One thing I am  wondering is, since there are always 24 hours in a day, why not let DGV have a complete set of columns and modify the _visibility_ of the columns instead? Do you have reasons other than making columns hidden or shown where you want to dynamically add and remove them?


  [1]: https://i.stack.imgur.com/Ag8np.png
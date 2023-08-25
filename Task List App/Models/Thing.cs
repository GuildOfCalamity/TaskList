using System;
using System.ComponentModel;
using Windows.Data.Json;

namespace Task_List_App.Models;

/// <summary>
/// R.F.U.
/// </summary>
public class Thing : INotifyPropertyChanged
{
    public string CustomerID { get; set; }
    public int EmployeeID { get; set; }
    public DateTime OrderDate { get; set; }
    public int OrderID { get; set; }
    public decimal OrderTotal { get; set; }

    public Thing()
    {
        // Set up default customer and employee IDs for demo
        CustomerID = "ICS";
        EmployeeID = 1;
        OrderDate = DateTime.Today;
    }

    public string ToJson()
    {
        JsonObject obj = new JsonObject();
        obj.SetNamedValue("CustomerID", JsonValue.CreateStringValue(CustomerID));
        obj.SetNamedValue("EmployeeID", JsonValue.CreateNumberValue(EmployeeID));
        obj.SetNamedValue("OrderDateString", JsonValue.CreateStringValue(OrderDate.ToString()));
        var jsonString = obj.Stringify();
        return jsonString;
    }

    public void LoadFromJson(string orderJson)
    {
        JsonObject orderObj = JsonObject.Parse(orderJson);
        CustomerID = orderObj.GetNamedString("CustomerID");
        EmployeeID = (int)orderObj.GetNamedNumber("EmployeeID");
        OrderDate = DateTime.Parse(orderObj.GetNamedString("OrderDateString"));

        JsonArray detailArray = orderObj.GetNamedArray("OrderDetails");
        foreach (IJsonValue jsonValue in detailArray)
        {
            if (jsonValue.ValueType == JsonValueType.Object)
            {
                JsonObject detailObj = jsonValue.GetObject();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ThingHeader : INotifyPropertyChanged
{
    public int FrontID { get; set; }
    public int BackID { get; set; }

    public ThingHeader() { }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

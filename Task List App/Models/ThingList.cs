using System;
using System.Collections.ObjectModel;
using Windows.Data.Json;

namespace Task_List_App.Models;

/// <summary>
/// R.F.U.
/// </summary>
public class ThingList : ObservableCollection<ThingHeader>
{
    public ThingList() { }

    public bool LoadFromJsonString(string json)
    {
        ThingList orders = new ThingList();
        
        JsonArray result = JsonArray.Parse(json);
        
        foreach (var jsonOrder in result)
        {
            JsonObject jsonOrderObject = jsonOrder.GetObject();
            var order = new ThingHeader();
            order.FrontID = Convert.ToInt32(jsonOrderObject.GetNamedNumber("FontID"));
            order.BackID = Convert.ToInt32(jsonOrderObject.GetNamedNumber("BackID"));
            orders.Add(order);
        }
        
        foreach (ThingHeader o in orders)
        {
            this.Items.Add(o);
        }

        return (Items.Count > 0);
    }
}

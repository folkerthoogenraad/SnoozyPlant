using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnoozyPlants.App.Model;

internal static class DateFormatter
{
    public static string FormatRelativeDate(DateTime date)
    {
        var now = DateTime.Now.Date;

        var diff = now - date;

        if(diff.Days == 1)
        {
            return "Yesterday";
        }

        if(diff.Days == -1)
        {
            return "Tomorrow";
        }

        if(diff.Days > 0)
        {
            return $"{diff.Days} days ago";
        }
        else if (diff.Days < 0)
        {
            return $"In {-diff.Days} days";
        }
        else
        {
            return "Today";
        }
    }

    public static string FormatInterval(int days)
    {
        if(days == 1)
        {
            return "Every day";
        }

        return $"Every {days} days";
    }
}

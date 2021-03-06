using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    // http://stackoverflow.com/questions/13099834/how-to-get-the-display-name-attribute-of-an-enum-member-via-mvc-razor-code

    public static string GetDisplayName(this Enum enumValue)
    {
        string displayAttribute = string.Empty;

        if (enumValue.GetType().GetMember(enumValue.ToString()).First().GetCustomAttribute<DisplayAttribute>() == null)
        {
            displayAttribute = enumValue.ToString();
        }
        else
        {
            displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                .GetName();
        }

        return displayAttribute;
    }
} // EnumExtensions
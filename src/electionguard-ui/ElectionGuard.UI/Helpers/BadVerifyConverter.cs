﻿using System.Globalization;

namespace ElectionGuard.UI.Helpers;

internal class BadVerifyConverter : IValueConverter
{

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "badverify.png" : "verify.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class BadVerifyTooltipConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var badVerify = (bool)value;
        return badVerify ? $"{AppResources.BadVerified}\n{AppResources.ContactAdmin}" : AppResources.Verified;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

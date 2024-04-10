using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace Task_List_App;

/// <summary>
/// For use with the <see cref="CommunityToolkit.Mvvm.Input.AsyncRelayCommand{T}"/>, e.g.
/// Text="{x:Bind ViewModel.SomeAsyncRelayCommand.ExecutionTask, Converter={StaticResource TaskResultConverter}"
/// Custom version of the converter from the Toolkit, with a bug fix. This should be moved over to the Toolkit in the next release.
/// </summary>
public class TaskResultConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type? targetType, object? parameter, string? language)
    {
        if (value is Task task)
        {
            return task.GetResultOrDefault(); // helper from our extensions class
        }

        return null;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type? targetType, object? parameter, string? language)
    {
        return null;
    }
}

/// <summary>
/// For use with the <see cref="CommunityToolkit.Mvvm.Input.AsyncRelayCommand{T}"/>, e.g.
/// Text="{x:Bind ViewModel.SomeAsyncRelayCommand.ExecutionTask.Exception, Converter={StaticResource TaskExceptionConverter}"
/// </summary>
public class TaskExceptionConverter : IValueConverter
{
    /// <inheritdoc/>
    public object? Convert(object? value, Type? targetType, object? parameter, string? language)
    {
        var result = string.Empty;

        if (value is AggregateException aex)
        {
            aex?.Flatten().Handle(ex =>
            {
                result = $"{ex.Message}";
                return true;
            });
        }
        else if (value is Exception ex)
        {
            result = $"{ex.Message}";
        }

        return result;
    }

    /// <inheritdoc/>
    public object? ConvertBack(object? value, Type? targetType, object? parameter, string? language)
    {
        return null;
    }
}

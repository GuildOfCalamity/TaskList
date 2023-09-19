namespace Task_List_App.Core.Contracts.Services;

public interface IMessageService
{
    void ShowMessageBox(IntPtr windowHandle, string title, string message, string okText, string cancelText, Action? okAction, Action? cancelAction);
    Task ShowMessageBoxAsync(IntPtr windowHandle, string title, string message, string okText, string cancelText, Action? okAction, Action? cancelAction);
}

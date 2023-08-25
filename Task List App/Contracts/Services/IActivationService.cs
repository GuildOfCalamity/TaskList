namespace Task_List_App.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}

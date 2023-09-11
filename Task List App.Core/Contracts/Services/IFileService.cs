namespace Task_List_App.Core.Contracts.Services;

public interface IFileService
{
    int DaysUntilBackupReplaced { get; set; }
    T Read<T>(string folderPath, string fileName);
    void Save<T>(string folderPath, string fileName, T content);
    bool Restore(string folderPath, string fileName);
    void Delete(string folderPath, string fileName);
}

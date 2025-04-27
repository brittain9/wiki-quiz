namespace WikiQuizGenerator.Core.Interfaces;

public interface IAiServiceManager
{
    void SelectAiService(string aiService, string model);
    string[] GetAvailableAiServices();
    string[] GetModels(string aiServiceId);
}

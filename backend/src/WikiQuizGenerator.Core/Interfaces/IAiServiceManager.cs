namespace WikiQuizGenerator.Core.Interfaces;

public interface IAiServiceManager
{
    void SelectAiService(int aiService, int model);
    Dictionary<int, string> GetAvailableAiServices();
    object GetModels(int? aiServiceId);
}

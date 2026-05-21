namespace TBRPicker.Jobs;

public record ImportJob(string UserId, List<string> Shelves);
namespace DriveTrack.Api.Data.Dto;

public record CreateCategoryRequest(string Name, Guid? OwnerUserId = null);

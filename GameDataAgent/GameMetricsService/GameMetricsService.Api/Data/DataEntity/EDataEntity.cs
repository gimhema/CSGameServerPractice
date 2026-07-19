namespace GameMetricsService.Api.Data.DataEntity;

public abstract class EDataEntity
{
    public long Id { get; init; }
    public long PlayerId { get; init; }
    public DateTime OccurredAt { get; init; }
}

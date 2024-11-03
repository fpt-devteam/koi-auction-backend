public class BreederStatisticDto
{
    public int BreederId { get; set; }
    public string FarmName { get; set; } = string.Empty;
    public double PercentUnsold { get; set; }
    public double PercentCancelledSoldLot { get; set; }
    public double PercentSuccess { get; set; }
    public double PercentUnsuccess { get; set; }
} 
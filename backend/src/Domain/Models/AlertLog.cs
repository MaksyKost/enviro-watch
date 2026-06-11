namespace EnviroWatch.Domain.Models;

public class AlertLog
{
    public Guid Id { get; set; }

    public Guid AlertId { get; set; }

    public double Value { get; set; }

    public double Threshold { get; set; }

    public DateTime TriggeredAt { get; set; }

    public bool EmailSent { get; set; }

    public Alert? Alert { get; set; }
}

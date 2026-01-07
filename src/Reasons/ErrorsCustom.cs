namespace REslava.Result;

public class StartDateIsAfterEndDateError : Error
{
    public StartDateIsAfterEndDateError (DateTime startDate, DateTime endDate)
    {
        base.Message = ($"The start date {startDate} is after the end date {endDate}");
        Tags.Add ("ErrorCode", "12");
    }
}

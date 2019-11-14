public class IronSourceError
{
    private int code;
    private string description;

    public IronSourceError(int errorCode, string errorDescription)
    {
        code = errorCode;
        description = errorDescription;
    }

    public int getErrorCode()
    {
        return code;
    }

    public string getDescription()
    {
        return description;
    }

    public int getCode()
    {
        return code;
    }

    public override string ToString()
    {
        return code + " : " + description;
    }
}
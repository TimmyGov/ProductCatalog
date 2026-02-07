namespace ProductCatalog.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
    {
    }

    public EntityNotFoundException(string message)
        : base(message)
    {
    }

    public EntityNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public EntityNotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key \"{key}\" was not found.")
    {
    }
}

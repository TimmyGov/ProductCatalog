namespace ProductCatalog.Domain.Exceptions;

public class DuplicateEntityException : Exception
{
    public DuplicateEntityException()
    {
    }

    public DuplicateEntityException(string message)
        : base(message)
    {
    }

    public DuplicateEntityException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DuplicateEntityException(string entityName, object key)
        : base($"Entity \"{entityName}\" with key \"{key}\" already exists.")
    {
    }
}

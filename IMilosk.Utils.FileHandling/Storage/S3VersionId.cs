namespace IMilosk.Utils.FileHandling.Storage;

public class S3VersionId : IEquatable<S3VersionId>
{
    public static S3VersionId Empty => string.Empty;
    private string Value { get; }

    private S3VersionId(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
    }

    private static S3VersionId Create(string value)
    {
        return new S3VersionId(value);
    }

    public bool Equals(S3VersionId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((S3VersionId)obj);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Value);
    }

    public static implicit operator string(S3VersionId s3VersionId) => s3VersionId.Value;
    public static implicit operator S3VersionId(string value) => Create(value);
}
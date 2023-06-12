namespace Api.Domain;

public static class DefaultLengths
{
    public const int Text = 256;

    public const int MetadataText = Text;

    public const int CreatedBy = MetadataText;
    public const int ChangedBy = MetadataText;
    public const int ConcurrencyToken = Text;
    public const int RowVersion = Text;
}
namespace BusinessObjects.Entities
{
    public interface IEntity<TKey>
    {
        TKey Id { get; }
    }
}

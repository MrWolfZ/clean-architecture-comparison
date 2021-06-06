namespace CAC.Core.Domain
{
    public abstract record Entity;
    
    public abstract record Entity<T, TId>(TId Id) : Entity
        where T : Entity<T, TId>
        where TId : EntityId<T>;
}

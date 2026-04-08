namespace Astrarium.Plugins.Journal.Database.Entities
{
    /// <summary>
    /// Common interface for all DB entities
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Unique ID of the entity
        /// </summary>
        string Id { get; set; }
    }
}

namespace TTPService.Configuration
{
    public class RepositoryOptions
    {
        public virtual string ConnectionString { get; set; }

        public virtual string Database { get; set; }

        public virtual string Collection { get; set; }

        public virtual bool ShouldSeedEmptyRepository { get; set; }
    }
}
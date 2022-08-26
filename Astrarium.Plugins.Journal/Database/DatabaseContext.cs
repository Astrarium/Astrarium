using Astrarium.Plugins.Journal.Database.Entities;
using Astrarium.Types;
using SQLite.CodeFirst;
using System.Data.Entity;

namespace Astrarium.Plugins.Journal.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base()
        {
            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;

            //Database.Log = s => System.Diagnostics.Debug.Write($"Observations DB: {s}");
        }

        public DbSet<SessionDB> Sessions { get; set; }
        public DbSet<SiteDB> Sites { get; set; }
        public DbSet<ObserverDB> Observers { get; set; }
        public DbSet<OpticsDB> Optics { get; set; }
        public DbSet<EyepieceDB> Eyepieces { get; set; }
        public DbSet<LensDB> Lenses { get; set; }
        public DbSet<FilterDB> Filters { get; set; }
        public DbSet<CameraDB> Cameras { get; set; }
        public DbSet<TargetDB> Targets { get; set; }
        public DbSet<ObservationDB> Observations { get; set; }
        public DbSet<AttachmentDB> Attachments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            System.Data.Entity.Database.SetInitializer(new SQLiteInitializer(modelBuilder));

            modelBuilder.Entity<ObserverDB>()
                .ToTable("Observers")
                .HasKey(x => x.Id);

            modelBuilder.Entity<SiteDB>()
                .ToTable("Sites")
                .HasKey(x => x.Id);

            modelBuilder.Entity<OpticsDB>()
               .ToTable("Optics")
               .HasKey(x => x.Id);

            modelBuilder.Entity<EyepieceDB>()
               .ToTable("Eyepieces")
               .HasKey(x => x.Id);

            modelBuilder.Entity<LensDB>()
               .ToTable("Lenses")
               .HasKey(x => x.Id);

            modelBuilder.Entity<FilterDB>()
               .ToTable("Filters")
               .HasKey(x => x.Id);

            modelBuilder.Entity<CameraDB>()
               .ToTable("Cameras")
               .HasKey(x => x.Id);

            modelBuilder.Entity<TargetDB>()
                .ToTable("Targets")
                .HasKey(x => x.Id);

            modelBuilder.Entity<AttachmentDB>()
                .ToTable("Attachments")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ObservationDB>()
                .ToTable("Observations")
                .HasKey(x => x.Id);

            modelBuilder.Entity<ObservationDB>()
                .HasRequired(x => x.Target)
                .WithMany()
                .HasForeignKey(x => x.TargetId);

            modelBuilder.Entity<ObservationDB>()
                .HasOptional(x => x.Scope)
                .WithMany()
                .HasForeignKey(x => x.ScopeId);

            modelBuilder.Entity<ObservationDB>()
                .HasOptional(x => x.Eyepiece)
                .WithMany()
                .HasForeignKey(x => x.EyepieceId);

            modelBuilder.Entity<ObservationDB>()
                .HasOptional(x => x.Lens)
                .WithMany()
                .HasForeignKey(x => x.LensId);

            modelBuilder.Entity<ObservationDB>()
                .HasOptional(x => x.Filter)
                .WithMany()
                .HasForeignKey(x => x.FilterId);

            modelBuilder.Entity<ObservationDB>()
                .HasOptional(x => x.Camera)
                .WithMany()
                .HasForeignKey(x => x.ImagerId);

            modelBuilder.Entity<SessionDB>()
                .ToTable("Sessions")
                .HasKey(x => x.Id);

            modelBuilder.Entity<SessionDB>()
                .HasMany(r => r.CoObservers)
                .WithMany() // No navigation property here
                .Map(m =>
                {
                    m.MapLeftKey("SessionId");
                    m.MapRightKey("ObserverId");
                    m.ToTable("CoObservers");
                });

            modelBuilder.Entity<SessionDB>()
                .HasMany(r => r.Attachments)
                .WithMany() // No navigation property here
                .Map(m =>
                {
                    m.MapLeftKey("SessionId");
                    m.MapRightKey("AttachmentId");
                    m.ToTable("SessionAttachments");
                });

            modelBuilder.Entity<ObservationDB>()
                .HasMany(r => r.Attachments)
                .WithMany() // No navigation property here
                .Map(m =>
                {
                    m.MapLeftKey("ObservationId");
                    m.MapRightKey("AttachmentId");
                    m.ToTable("ObservationAttachments");
                });

            modelBuilder.Entity<SessionDB>()
                .HasMany(x => x.Observations)
                .WithRequired()
                .HasForeignKey(x => x.SessionId);
        }

        private class SQLiteInitializer : SqliteCreateDatabaseIfNotExists<DatabaseContext>
        {
            public SQLiteInitializer(DbModelBuilder modelBuilder) : base(modelBuilder) { }

            protected override void Seed(DatabaseContext context)
            {
                base.Seed(context);
                context.Database.ExecuteSqlCommand($"PRAGMA user_version = 1");
            }
        }
    }
}

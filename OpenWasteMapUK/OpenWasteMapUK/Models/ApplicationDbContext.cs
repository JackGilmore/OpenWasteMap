using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace OpenWasteMapUK.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OsmElement>().HasKey(k => k.Id);
            builder.Entity<OsmElement>().Property(p => p.Id).ValueGeneratedNever();

            var nodesValueComparer = new ValueComparer<List<long>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList()
            );

            builder.Entity<OsmElement>()
                .Property(e => e.Nodes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList()
                )
                .Metadata
                .SetValueComparer(nodesValueComparer);

            var tagsValueComparer = new ValueComparer<Dictionary<string, string>>(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c
            );

            builder.Entity<OsmElement>()
                .Property(e => e.Tags)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => v == null
                        ? new Dictionary<string, string>()
                        : JsonConvert.DeserializeObject<Dictionary<string, string>>(v)
                )
                .HasColumnType("nvarchar(max)")
                .Metadata
                .SetValueComparer(tagsValueComparer);
        }


        public DbSet<OsmElement> OsmElements { get; set; }
    }
}

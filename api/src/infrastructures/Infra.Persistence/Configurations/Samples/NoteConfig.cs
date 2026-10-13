using Core.Domain.Entities.Samples.Notes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.Persistence.Configurations.Samples;

internal sealed class NoteConfig : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.Property(c => c.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Content)
            .HasMaxLength(5000);

        builder.Property(c => c.IsPinned)
            .IsRequired();

        builder.Property(c => c.IsDeleted)
            .IsRequired();

        builder.HasQueryFilter(c => !c.IsDeleted);

        builder.HasIndex(c => new { c.IsPinned, c.Title })
            .HasDatabaseName("IX_Note_IsPinned_Title");
    }
}

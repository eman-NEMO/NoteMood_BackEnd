using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NoteMoodUOW.Core.Interfaces;
using NoteMoodUOW.Core.Models;
using System;

namespace NoteMoodUOW.EF
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Entry> Entries { get; set; }
        public DbSet<DailySentiment> DailySentiments { get; set; }
        public DbSet<Aspect> Aspects { get; set; }
        public DbSet<Entity> Entities { get; set; }
        public DbSet<Sentiment> Sentiments { get; set; }
        public DbSet<EntitySentiment> EntitySentiments { get; set; }
        public DbSet<TopicSentiment> TopicSentiments { get; set; }
        public DbSet<Topic> Topics { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DailySentiment>()
                .HasKey(uds => new { uds.ApplicationUserId, uds.Date });

            modelBuilder.Entity<DailySentiment>()
                .HasOne(uds => uds.User)
                .WithMany(u => u.DailySentiments)
                .HasForeignKey(uds => uds.ApplicationUserId);

            modelBuilder.Entity<Aspect>()
                .HasMany(a => a.Entities)
                .WithOne(e => e.Aspect)
                .HasForeignKey(e => e.AspectId);

            modelBuilder.Entity<Entity>()
                .HasMany(e => e.EntitySentiments)
                .WithOne(es => es.Entity)
                .HasForeignKey(es => es.EntityId);

            modelBuilder.Entity<Sentiment>()
                .HasMany(s => s.EntitySentiments)
                .WithOne(es => es.Sentiment)
                .HasForeignKey(es => es.SentimentId);



            modelBuilder.Entity<EntitySentiment>()
                 .HasOne(es => es.Entry)
                 .WithMany(e => e.EntitySentiments)
                 .HasForeignKey(es => es.EntryId);

            modelBuilder.Entity<Entry>()
                .HasMany(e => e.EntitySentiments)
                .WithOne(es => es.Entry)
                .HasForeignKey(es => es.EntryId);

           modelBuilder.Entity<TopicSentiment>()
                .HasOne(ts => ts.Entry)
                .WithMany(e => e.TopicSentiments)
                .HasForeignKey(ts => ts.EntryId);
            modelBuilder.Entity<TopicSentiment>()
                .HasOne(ts => ts.Topic)
                .WithMany(t => t.TopicSentiments)
                .HasForeignKey(ts => ts.TopicId);
            modelBuilder.Entity<TopicSentiment>()
                .HasOne(ts => ts.Sentiment)
                .WithMany(s => s.TopicSentiments)
                .HasForeignKey(ts => ts.SentimentId);



         modelBuilder.Entity<Aspect>().ToTable("Aspect");
         modelBuilder.Entity<Entity>().ToTable("Entity");
         modelBuilder.Entity<Sentiment>().ToTable("Sentiment");
         modelBuilder.Entity<EntitySentiment>().ToTable("EntitySentiment");
         modelBuilder.Entity<TopicSentiment>().ToTable("TopicSentiment");
         modelBuilder.Entity<Topic>().ToTable("Topic");





        }
    }
}

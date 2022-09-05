﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using youtube_feed_asp.Data;

#nullable disable

namespace youtube_feed_asp.Migrations
{
    [DbContext(typeof(VideoContext))]
    [Migration("20220905001626_InitialModel")]
    partial class InitialModel
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.8");

            modelBuilder.Entity("youtube_feed_asp.Models.Channel", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("LastModified")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("youtube_feed_asp.Models.Video", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("TimeAdded")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TimePublished")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Type")
                        .HasColumnType("varchar(30)");

                    b.Property<string>("UploaderId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UploaderId");

                    b.ToTable("Videos");
                });

            modelBuilder.Entity("youtube_feed_asp.Models.Video", b =>
                {
                    b.HasOne("youtube_feed_asp.Models.Channel", "Uploader")
                        .WithMany("Videos")
                        .HasForeignKey("UploaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("youtube_feed_asp.Models.Channel", b =>
                {
                    b.Navigation("Videos");
                });
#pragma warning restore 612, 618
        }
    }
}

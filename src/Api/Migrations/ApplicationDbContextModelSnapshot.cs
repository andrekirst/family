﻿// <auto-generated />
using System;
using Api.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Api.Database.Authentication.GoogleAccountEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AccessToken")
                        .HasColumnType("text");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("GoogleId")
                        .IsRequired()
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.HasKey("Id");

                    b.ToTable("GoogleAccounts", "authentiction");
                });

            modelBuilder.Entity("Api.Database.Body.WeightTrackingEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid>("FamilyMemberEntityId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("FamilyMemberId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("MeasuredAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<double>("Weight")
                        .HasColumnType("double precision");

                    b.Property<string>("WeightUnit")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("FamilyMemberEntityId");

                    b.ToTable("WeightTrackingEntries", "weighttracking");
                });

            modelBuilder.Entity("Api.Database.Calendar.CalendarEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid>("FamilyMemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("FamilyMemberId");

                    b.ToTable("Calendars", "calendar");
                });

            modelBuilder.Entity("Api.Database.Core.DomainEventEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid?>("CreatedByFamilyMemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("CreatedForFamilyMemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("EventData")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<int>("EventVersion")
                        .HasColumnType("integer");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedByFamilyMemberId");

                    b.HasIndex("CreatedForFamilyMemberId");

                    b.ToTable("DomainEvents", "core");
                });

            modelBuilder.Entity("Api.Database.Core.FamilyMemberEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("AspNetUserId")
                        .HasMaxLength(450)
                        .HasColumnType("character varying(450)");

                    b.Property<DateTime?>("Birthdate")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("LastName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("FamilyMembers", "core");
                });

            modelBuilder.Entity("Api.Database.Core.NotificationMessageEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<Guid>("FamilyMemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Instance")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("MarkedAsRead")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("MessageType")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("FamilyMemberId");

                    b.ToTable("NotificatioMessages", "core");
                });

            modelBuilder.Entity("Api.Domain.Core.LabelEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("ChangedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("ConcurrencyToken")
                        .IsConcurrencyToken()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(265)
                        .HasColumnType("character varying(265)");

                    b.Property<string>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasMaxLength(256)
                        .IsUnicode(false)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.ToTable("Labels", "core");
                });

            modelBuilder.Entity("FamilyMemberEntityLabelEntity", b =>
                {
                    b.Property<Guid>("FamilyMembersId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("LabelsId")
                        .HasColumnType("uuid");

                    b.HasKey("FamilyMembersId", "LabelsId");

                    b.HasIndex("LabelsId");

                    b.ToTable("FamilyMemberEntityLabelEntity", "core");
                });

            modelBuilder.Entity("Api.Database.Body.WeightTrackingEntity", b =>
                {
                    b.HasOne("Api.Database.Core.FamilyMemberEntity", "FamilyMemberEntity")
                        .WithMany("WeightTrackingEntries")
                        .HasForeignKey("FamilyMemberEntityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FamilyMemberEntity");
                });

            modelBuilder.Entity("Api.Database.Calendar.CalendarEntity", b =>
                {
                    b.HasOne("Api.Database.Core.FamilyMemberEntity", "FamilyMemberEntity")
                        .WithMany("Calendars")
                        .HasForeignKey("FamilyMemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FamilyMemberEntity");
                });

            modelBuilder.Entity("Api.Database.Core.DomainEventEntity", b =>
                {
                    b.HasOne("Api.Database.Core.FamilyMemberEntity", "CreatedByFamilyMember")
                        .WithMany("CreatedByDomainEventEntries")
                        .HasForeignKey("CreatedByFamilyMemberId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Api.Database.Core.FamilyMemberEntity", "CreatedForFamilyMember")
                        .WithMany("CreatedForDomainEventEntries")
                        .HasForeignKey("CreatedForFamilyMemberId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.Navigation("CreatedByFamilyMember");

                    b.Navigation("CreatedForFamilyMember");
                });

            modelBuilder.Entity("Api.Database.Core.NotificationMessageEntity", b =>
                {
                    b.HasOne("Api.Database.Core.FamilyMemberEntity", "FamilyMemberEntity")
                        .WithMany("NotificationMessages")
                        .HasForeignKey("FamilyMemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FamilyMemberEntity");
                });

            modelBuilder.Entity("Api.Domain.Core.LabelEntity", b =>
                {
                    b.OwnsOne("Api.Domain.Core.Color", "Color", b1 =>
                        {
                            b1.Property<Guid>("LabelEntityId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasMaxLength(256)
                                .IsUnicode(false)
                                .HasColumnType("character varying(256)")
                                .HasColumnName("Color");

                            b1.HasKey("LabelEntityId");

                            b1.ToTable("Labels", "core");

                            b1.WithOwner()
                                .HasForeignKey("LabelEntityId");
                        });

                    b.Navigation("Color")
                        .IsRequired();
                });

            modelBuilder.Entity("FamilyMemberEntityLabelEntity", b =>
                {
                    b.HasOne("Api.Database.Core.FamilyMemberEntity", null)
                        .WithMany()
                        .HasForeignKey("FamilyMembersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Api.Domain.Core.LabelEntity", null)
                        .WithMany()
                        .HasForeignKey("LabelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Api.Database.Core.FamilyMemberEntity", b =>
                {
                    b.Navigation("Calendars");

                    b.Navigation("CreatedByDomainEventEntries");

                    b.Navigation("CreatedForDomainEventEntries");

                    b.Navigation("NotificationMessages");

                    b.Navigation("WeightTrackingEntries");
                });
#pragma warning restore 612, 618
        }
    }
}

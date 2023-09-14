﻿// <auto-generated />
using System;
using CryptoBank.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoBank.Migrations
{
    [DbContext(typeof(Context))]
    partial class ContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("public")
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CryptoBank.Features.Accounts.Domain.Account", b =>
                {
                    b.Property<string>("Number")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("number");

                    b.Property<decimal>("Amount")
                        .HasPrecision(20, 2)
                        .HasColumnType("numeric(20,2)")
                        .HasColumnName("amount");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasColumnName("currency");

                    b.Property<DateTime>("DateOfOpening")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_of_opening");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Number");

                    b.HasIndex("UserId");

                    b.ToTable("accounts", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.Authenticate.Domain.UserToken", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created");

                    b.Property<DateTime>("Expires")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("expires");

                    b.Property<string>("ReasonRevoked")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("reason_revoked");

                    b.Property<int?>("ReplacedByTokenId")
                        .HasColumnType("integer")
                        .HasColumnName("replaced_by_token_id");

                    b.Property<DateTime?>("Revoked")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("revoked");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("token");

                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("ReplacedByTokenId")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("user_tokens", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)")
                        .HasColumnName("description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("character varying(400)")
                        .HasColumnName("name");

                    b.HasKey("Id");

                    b.ToTable("roles", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_of_birth");

                    b.Property<DateTime>("DateOfRegistration")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date_of_registration");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("email");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasColumnName("password");

                    b.HasKey("Id");

                    b.ToTable("users", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.UserRole", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("integer")
                        .HasColumnName("user_id");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer")
                        .HasColumnName("role_id");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("user_roles", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.News.Domain.New", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasColumnName("id");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("author");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)")
                        .HasColumnName("description");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)")
                        .HasColumnName("title");

                    b.HasKey("Id");

                    b.ToTable("news", "public");
                });

            modelBuilder.Entity("CryptoBank.Features.Accounts.Domain.Account", b =>
                {
                    b.HasOne("CryptoBank.Features.Management.Domain.User", "User")
                        .WithMany("UserAccounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("CryptoBank.Features.Authenticate.Domain.UserToken", b =>
                {
                    b.HasOne("CryptoBank.Features.Authenticate.Domain.UserToken", "RefreshToken")
                        .WithOne()
                        .HasForeignKey("CryptoBank.Features.Authenticate.Domain.UserToken", "ReplacedByTokenId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("CryptoBank.Features.Management.Domain.User", "User")
                        .WithMany("UserTokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("RefreshToken");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.UserRole", b =>
                {
                    b.HasOne("CryptoBank.Features.Management.Domain.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CryptoBank.Features.Management.Domain.User", "User")
                        .WithMany("UserRoles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.Role", b =>
                {
                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("CryptoBank.Features.Management.Domain.User", b =>
                {
                    b.Navigation("UserAccounts");

                    b.Navigation("UserRoles");

                    b.Navigation("UserTokens");
                });
#pragma warning restore 612, 618
        }
    }
}

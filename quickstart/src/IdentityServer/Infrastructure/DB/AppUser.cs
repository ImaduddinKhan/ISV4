using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Infrastructure.DB
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(500)]
        public string WhatEverEntity { get; set; }
    }
}

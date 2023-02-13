using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppForMORecSys.Models;

namespace WebAppForMORecSys.Areas.Identity.Data;

// Add profile data for application users by adding properties to the User class
public class Account : IdentityUser
{
    [ForeignKey("User")]
    public int UserID { get; set; }

    public User User;

    public Account()
    {

    }
}


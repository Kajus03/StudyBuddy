using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudyBuddy.Shared.ValueObjects;

namespace StudyBuddy.Shared.Models
{
    public class Block
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public UserId SenderId { get; set; }
        public User? Sender { get; set; }
        [Required]
        public UserId ReceiverId { get; set; }
        public User? Receiver { get; set; }
    }
}

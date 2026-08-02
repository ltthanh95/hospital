using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.Abstraction
{

    public abstract class Person
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string FName { get; set; }
        public required string LName { get; set; }

        public required DateTime DoB { get; set; }

        public Gender Gender { get; set; }

        public string Address { get; set; }
        public string Phone { get; set; }

        public string Email { get; set; }

    }
}

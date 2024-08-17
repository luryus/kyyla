using System;
using System.ComponentModel.DataAnnotations;

namespace Kyyla.Model
{
    public class LockEvent
    {
        public enum Type { Lock, Unlock };

        [Required]
        public Type EventType { get; set; }

        [Key]
        public DateTime Timestamp { get; set; }
    }
}

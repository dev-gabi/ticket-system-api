using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
    public class ReplyImage : DBEntity
    {
        [Required]
        public int ReplyId { get; set; }
        [Required]
        public string Path { get; set; }

      //  public virtual Reply Reply{get;set;}
    }
}

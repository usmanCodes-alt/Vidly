using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vidly.Models.ViewModels
{
    public class CreateNewMovieViewModel
    {
        [Required]
        public Movie Movie { get; set; }
        public IEnumerable<SelectListItem> Categories { get; set; }
        public string StatusMessage { get; set; }
    }
}

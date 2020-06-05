using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalker.Models
{
    public class Walker
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NeighborhoodId { get; set; }
        public string ImageUrl { get; set; }
        public Neighborhood Neighborhood { get; set; }
    }
}

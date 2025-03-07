﻿using System;
using Watchster.Domain.Common;

namespace Watchster.Domain.Entities
{
    public class Movie : BaseEntity
    {
        public int TMDbId { get; set; }

        public string Title { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public string Genres { get; set; }

        public string PosterUrl { get; set; }

        public double Popularity { get; set; }

        public double TMDbVoteAverage { get; set; }

        public string Overview { get; set; }
    }
}

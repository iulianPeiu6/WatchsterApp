﻿using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Watchster.Application.Features.Commands;
using Watchster.Application.Features.Queries;
using Watchster.TMDb.Models;
using Watchster.TMDb.Services;

namespace Watchster.MovieImporter.Job
{
    public class MovieImporterJob : IJob
    {
        private readonly ILogger<MovieImporterJob> logger;
        private readonly ITMDbMovieDiscoverService movieDiscover;
        private readonly IMediator mediator;
        private Domain.Entities.AppSettings movieImporterSettings;
        private DateTime UpperBoundIntervalSearch;
        private const string TMDB_POSTER_ENDPOINT = "https://image.tmdb.org/t/p/original";

        public MovieImporterJob(
            ILogger<MovieImporterJob> logger,
            ITMDbMovieDiscoverService movieDiscover,
            IMediator mediator)
        {
            this.logger = logger;
            this.movieDiscover = movieDiscover;
            this.mediator = mediator;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var currentDateTime = DateTime.Now;
            try
            {
                logger.LogInformation("Starting importing new released movies");

                UpperBoundIntervalSearch = currentDateTime.AddDays(-1);

                var lastSyncDateTime = await GetLastSyncDateTime();

                var numOfImportedMovies = await ImportNewMoviesAfterDateAsync(lastSyncDateTime);

                await UpdateLastSyncDateTime(currentDateTime);

                logger.LogInformation($"Ended importing new released movies. {numOfImportedMovies} new movie(s) were imported.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await UpdateLastSyncDateTime(currentDateTime);
            }
        }

        private async Task<DateTime> GetLastSyncDateTime()
        {
            var query = new GetAppSettingsBySectionAndParameterQuery()
            {
                Section = "MovieImporter",
                Parameter = "LastSyncDate"
            };

            movieImporterSettings = await mediator.Send(query);
            DateTime lastSyncDate;
            if (movieImporterSettings.Value is null)
            {
                lastSyncDate = DateTime.MinValue;
            }
            else
            {
                lastSyncDate = DateTime.Parse(movieImporterSettings.Value, CultureInfo.InvariantCulture);
            }

            return lastSyncDate;
        }

        private async Task<int> ImportNewMoviesAfterDateAsync(DateTime lastSyncDateTime)
        {
            if (lastSyncDateTime == DateTime.MinValue)
            {
                return await ImportAllMoviesToPresent();
            }

            var result = movieDiscover.GetMoviesBetweenDatesFromPage(lastSyncDateTime, UpperBoundIntervalSearch);
            await ImportMovies(result.Movies);
            int numOfMoviesImported = result.Movies.Count;
            if (result.TotalPages > 1)
            {
                foreach (var page in Enumerable.Range(2, result.TotalPages - 2))
                {
                    var movies = movieDiscover.GetMoviesBetweenDatesFromPage(lastSyncDateTime, UpperBoundIntervalSearch, page).Movies;
                    await ImportMovies(movies);
                    numOfMoviesImported += movies.Count;
                }
            }

            return numOfMoviesImported;
        }

        private async Task<int> ImportAllMoviesToPresent()
        {
            int numOfMoviesImported = 0;
            for (int year = 1874; year <= UpperBoundIntervalSearch.Year; year++)
            {
                numOfMoviesImported += await ImportMoviesFromYearAsync(year);
            }
            return numOfMoviesImported;
        }

        private async Task<int> ImportMoviesFromYearAsync(int year)
        {
            int numOfMoviesImported = 0;
            var monthUpperBound = year == UpperBoundIntervalSearch.Year ? UpperBoundIntervalSearch.Month : 12;
            for (int month = 1; month <= monthUpperBound; month++)
            {
                var from = new DateTime(year, month, 1);
                var to = from.AddMonths(1).AddDays(-1);
                if (year == UpperBoundIntervalSearch.Year && month == UpperBoundIntervalSearch.Month)
                {
                    to = UpperBoundIntervalSearch;
                }

                numOfMoviesImported += await ImportMoviesInRabge(from, to);
            }
            return numOfMoviesImported;
        }

        private async Task<int> ImportMoviesInRabge(DateTime from, DateTime to)
        {

            var result = movieDiscover.GetMoviesBetweenDatesFromPage(from, to);
            await ImportMovies(result.Movies);
            int numOfMoviesImported = result.Movies.Count;

            if (result.TotalPages > 1)
            {
                foreach (var page in Enumerable.Range(2, result.TotalPages - 1))
                {
                    var movies = movieDiscover.GetMoviesBetweenDatesFromPage(from, to, page).Movies;
                    await ImportMovies(movies);
                    numOfMoviesImported += movies.Count;
                }
            }
            return numOfMoviesImported;
        }

        private async Task ImportMovies(List<Movie> movies)
        {
            foreach (var movie in movies)
            {
                var command = new CreateMovieCommand
                {
                    Title = movie.Title,
                    Overview = movie.Overview,
                    TMDbId = movie.TMDbId,
                    ReleaseDate = movie.ReleaseDate,
                    Genres = movie.Genres.Select(genre => genre.Name),
                    Popularity = movie.Popularity,
                    TMDbVoteAverage = movie.VoteAverage,
                    PosterUrl = $"{TMDB_POSTER_ENDPOINT}{movie.PosterPath}"
                };
                await mediator.Send(command);
            }
        }

        private async Task UpdateLastSyncDateTime(DateTime date)
        {
            var command = new UpdateAppSettingsCommand
            {
                Id = movieImporterSettings.Id,
                Section = movieImporterSettings.Section,
                Parameter = movieImporterSettings.Parameter,
                Description = movieImporterSettings.Description,
                Value = date.ToString()
            };
            await mediator.Send(command);
        }
    }
}

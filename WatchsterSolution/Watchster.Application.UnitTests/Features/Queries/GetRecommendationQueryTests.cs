﻿using FakeItEasy;
using Faker;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchster.Application.Features.Queries;
using Watchster.Application.Interfaces;
using Watchster.Application.Models;
using Watchster.Application.UnitTests.Fakes;
using Watchster.Domain.Entities;

namespace Watchster.Application.UnitTests.Features.Queries
{
    public class GetRecommendationsQueryTests
    {
        private readonly GetRecommendationsQueryHandler handler;
        private readonly FakeMovieRepository movieRepository;
        private readonly FakeRatingRepository ratingRepository;
        private readonly IMovieRecommender movieRecommender;

        public GetRecommendationsQueryTests()
        {
            ratingRepository = A.Fake<FakeRatingRepository>();
            movieRepository = A.Fake<FakeMovieRepository>();
            movieRecommender = A.Fake<IMovieRecommender>();
            handler = new GetRecommendationsQueryHandler(ratingRepository, movieRepository, movieRecommender);
        }

        [Test]
        public async Task Given_GetRecommendationsQueryWithNoUserRatings_When_GetRecommendationsQueryHandlerIsCalled_Then_GetRecommendationsQueryHandlerShouldReturnEmptyResultAsync()
        {
            // Arrange
            var query = new GetRecommendationsQuery
            {
                UserId = 101
            };

            // Act
            var response = await handler.Handle(query, default);

            // Assert
            response.Should().NotBeNull();
            response.Recommendations.Should().BeEmpty();
        }

        [Test]

        public async Task Given_GetRecommendationQueryWithUserReviews_When_GetRecommendationsQueryHandlerIsCalled_Then_GetRecommendationsQueryHandlerShouldReturnGetRecommendationsResponse()
        {
            // Arrange
            var repoMovies = new List<Movie>
            {
                new Movie
                {
                    Id = 1,
                    Title = Lorem.Sentence(),
                    Overview = Lorem.Sentence(),
                    Genres = Lorem.Sentence(),
                    Popularity = RandomNumber.Next(),
                    PosterUrl = Internet.Url(),
                    ReleaseDate = DateTime.Now,
                    TMDbId = RandomNumber.Next(),
                    TMDbVoteAverage = RandomNumber.Next()
                },
                new Movie
                {
                    Id = 2,
                    Title = Lorem.Sentence(),
                    Overview = Lorem.Sentence(),
                    Genres = Lorem.Sentence(),
                    Popularity = RandomNumber.Next(),
                    PosterUrl = Internet.Url(),
                    ReleaseDate = DateTime.Now,
                    TMDbId = RandomNumber.Next(),
                    TMDbVoteAverage = RandomNumber.Next()
                },
                new Movie
                {
                    Id = 3,
                    Title = Lorem.Sentence(),
                    Overview = Lorem.Sentence(),
                    Genres = Lorem.Sentence(),
                    Popularity = RandomNumber.Next(),
                    PosterUrl = Internet.Url(),
                    ReleaseDate = DateTime.Now,
                    TMDbId = RandomNumber.Next(),
                    TMDbVoteAverage = RandomNumber.Next()
                },
            }.AsEnumerable();

            foreach (Movie movie in repoMovies)
            {
                await movieRepository.AddAsync(movie);
            }

            var repoRatings = new List<Rating>
            {
                new Rating
                {
                    Id = 1,
                    UserId = 1,
                    MovieId = 1,
                    RatingValue = 7.2
                },
                new Rating
                {
                    Id = 2,
                    UserId = 1,
                    MovieId = 2,
                    RatingValue = 8.2
                },
                new Rating
                {
                    Id = 3,
                    UserId = 2,
                    MovieId = 2,
                    RatingValue = 9.2
                },
                new Rating
                {
                    Id = 4,
                    UserId = 2,
                    MovieId = 3,
                    RatingValue = 9.5
                }
            }.AsEnumerable();

            foreach (Rating rating in repoRatings)
            {
                await ratingRepository.AddAsync(rating);
            }

            var query = new GetRecommendationsQuery
            {
                UserId = 1
            };

            // Act
            var response = await handler.Handle(query, default);

            // Assert
            response.Should().NotBeNull();
            response.Should().BeOfType<GetRecommendationsResponse>();
            response.Recommendations.Should().NotBeNullOrEmpty();
            foreach (MovieRecommendation details in response.Recommendations)
            {
                details.Should().NotBeNull();
                details.Id.Should().BePositive();
                details.TMDbId.Should().BePositive();
                details.Title.Should().NotBeNullOrEmpty();
                details.ReleaseDate.Should().NotBeNull();
                details.Genres.Should().NotBeNull();
                details.Overview.Should().NotBeNullOrEmpty();
                details.Score.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(10);
            }
        }
    }
}

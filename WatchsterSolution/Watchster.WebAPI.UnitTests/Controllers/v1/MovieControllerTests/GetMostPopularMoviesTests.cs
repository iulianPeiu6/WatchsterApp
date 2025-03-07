﻿using FakeItEasy;
using NUnit.Framework;
using Watchster.Application.Features.Queries;
using Watchster.WebApi.UnitTests.v1.Abstracts;

namespace Watchster.WebApi.UnitTests.Controllers.v1.MovieControllerTests
{
    public class GetMostPopularMoviesTests : MovieControllerTestsBase
    {
        public GetMostPopularMoviesTests() : base()
        {
        }

        [Test]
        public void Given_MovieController_When_GetMostPopularMoviesIsCalled_Then_QueryShouldHaveHappenedOnce()
        {
            //arrange

            //act
            var result = controller.GetMostPopularAsync();

            //assert
            A.CallTo(() => mediator.Send(A<GetMostPopularMoviesQuery>._, default)).MustHaveHappenedOnceExactly();
        }
    }
}

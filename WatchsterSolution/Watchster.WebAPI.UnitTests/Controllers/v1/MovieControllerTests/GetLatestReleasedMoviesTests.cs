﻿using FakeItEasy;
using NUnit.Framework;
using Watchster.Application.Features.Queries;
using Watchster.WebApi.UnitTests.v1.Abstracts;

namespace Watchster.WebApi.UnitTests.Controllers.v1.MovieControllerTests
{
    public class GetLatestReleasedMoviesTests : MovieControllerTestsBase
    {
        public GetLatestReleasedMoviesTests() : base()
        {
        }

        [Test]
        public void Given_MovieController_When_GetLatestReleasedMoviesIsCalled_Then_QueryShouldHaveHappenedOnce()
        {
            //arrange

            //act
            var result = controller.GetLatestReleasedAsync();

            //assert
            A.CallTo(() => mediator.Send(A<GetLatestReleasedMoviesQuery>._, default)).MustHaveHappenedOnceExactly();
        }
    }
}

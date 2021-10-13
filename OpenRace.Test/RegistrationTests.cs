using System;
using FluentAssertions;
using Xunit;

namespace OpenRace.Test
{
    public class RegistrationTests
    {
        [Theory]
        [InlineData(1000, null, 1)]
        [InlineData(1000, 1, 2)]
        [InlineData(1000, 29, 30)]
        [InlineData(1000, 30, 131)]
        [InlineData(1000, 131, 132)]
        [InlineData(2000, null, 31)]
        [InlineData(2000, 59, 60)]
        [InlineData(2000, 61, 141)]
        public void Next_member_number_creating_is_correct(int distance, int? current, int nextExpected)
        {
            var config = AppConfig.Current;
            var result = config.GetNextMemberNumber(distance, current);
            result.Should().Be(nextExpected);
        }
    }
}
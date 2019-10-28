using System;
using System.Collections.Generic;
using Xunit;
using static Challenge.Program;

namespace Challenge
{
    public class Tests
    {
        // Rotate function tests
        [Fact]
        public void RotateEastToSouth() => Assert.Equal("S", Rotate("E", "R"));
        [Fact]
        public void RotateWestToNorth() => Assert.Equal("N", Rotate("W", "R"));
        [Fact]
        public void RotateSouthToEast() => Assert.Equal("E", Rotate("S", "L"));
        [Fact]
        public void RotateNorthToWest() => Assert.Equal("W", Rotate("N", "L"));

        // Move function tests with no previous warningCoordsList
        [Theory]
        // 9,9 is gridMaxY, gridMaxY
        // normal movement not getting lost
        [InlineData(0, 0, "N", 9, 9, null, 0, 1, false)]
        [InlineData(0, 1, "S", 9, 9, null, 0, 0, false)]
        [InlineData(0, 0, "E", 9, 9, null, 1, 0, false)]
        [InlineData(1, 0, "W", 9, 9, null, 0, 0, false)]

        // isLost=true (with no previous warningCoordsList)
        // North - go off top so lost, gridMaxX 2, gridMaxY 2, expectedX 2, expectedY 2
        [InlineData(2, 2, "N", 2, 2, null, 2, 2, true)]
        // South - go off bottom
        [InlineData(0, 0, "S", 2, 2, null, 0, 0, true)]
        // East
        [InlineData(2, 1, "E", 2, 2, null, 2, 1, true)]
        // West
        [InlineData(0, 1, "W", 2, 2, null, 0, 1, true)]
        public void MoveTests(int currentX, int currentY, string orientation, int gridMaxX, int gridMaxY,
            List<(int, int, string)> warningCoordsList, int expectedX, int expectedY, bool isLost)
        {
            Assert.Equal((expectedX, expectedY, isLost), 
                Move(currentX, currentY, orientation, gridMaxX, gridMaxY, warningCoordsList));
        }

        // Move function tests with previous warningCoordsList
        // expecting isLost=false and the coordinates to be the last known good position
        [Fact]
        public void MoveWithWarningCoordsN()
        {
            var expected = (0, 2, false);
            Assert.Equal(expected, Move(0, 2, "N", 2, 2, new List<(int, int, string)> { (0, 2, "N") }));
        }
        [Fact]
        public void MoveWithWarningCoordsS()
        {
            var expected = (2, 0, false);
            Assert.Equal(expected, Move(2, 0, "S", 2, 2, new List<(int, int, string)> { (2, 0, "S") }));
        }
        [Fact]
        public void MoveWithWarningCoordsE()
        {
            var expected = (2, 0, false);
            Assert.Equal(expected, Move(2, 0, "E", 2, 2, new List<(int, int, string)> { (2, 0, "E") }));
        }
        [Fact]
        public void MoveWithWarningCoordsW()
        {
            var expected = (0, 2, false);
            Assert.Equal(expected, Move(0, 2, "W", 2, 2, new List<(int, int, string)> { (0, 2, "W") }));
        }


        // Full tests of the app
        [Fact]
        public void RunFirstShip() => Assert.Equal("1 1 E", Run(new[] { "5 3\n1 1 E\nRFRFRFRF" }));

        [Fact]
        public void RunSecondShip() => Assert.Equal("3 3 N LOST", Run(new[] { "5 3\n3 2 N\nFRRFLLFFRRFLL" }));

        // Only pass the maxgrid size on the first ship
        [Fact]
        public void RunFirstAndSecondShip()
        {
            Assert.Equal("3 3 N LOST", Run(new[] { "5 3\n1 1 E\nRFRFRFRF", "3 2 N\nFRRFLLFFRRFLL" }));
        }
        // ThirdShip depends on the knowledge of SecondShip for a warning
        // so need to run SecondShip and ThirdShip together
        [Fact]
        public void RunThirdShip() => Assert.Equal("2 3 S", Run(new[] { "5 3\n3 2 N\nFRRFLLFFRRFLL", "0 3 W\nLLFFFLFLFL" }));

        [Fact]
        public void RunAllShips() => Assert.Equal("2 3 S", Run(new[] { "5 3\n1 1 E\nRFRFRFRF", "3 2 N\nFRRFLLFFRRFLL", "0 3 W\nLLFFFLFLFL" }));



        // Tests of exceptions
        [Fact]
        public void InstructionStringMoreOrEqualTo100ShouldThrow()
        {
            var instructions = new string('R', 100);
            Assert.Throws<ArgumentException>(() => Run(new[] { $"5 3\n1 1 E\n{instructions}" }));
        }
        [Fact]
        public void InstructionStringLessThan100ShouldNotThrow()
        {
            var instructions = new string('R', 99);
            // if no exception thrown the test will pass
            var result = Run(new[] { $"5 3\n1 1 E\n{instructions}" });
        }
        [Fact]
        public void GridMaxXShouldThrowIfMoreThan50() =>
                    Assert.Throws<ArgumentException>(() => Run(new[] { "51 3\n1 1 E\nRFRFRFRF" }));

        [Fact]
        public void GridMaxYShouldThrowIfMoreThan50() =>
            Assert.Throws<ArgumentException>(() => Run(new[] { "5 51\n1 1 E\nRFRFRFRF" }));

        [Fact]
        public void XShouldThrowIfMoreThan50() =>
            Assert.Throws<ArgumentException>(() => Run(new[] { "5 3\n51 1 E\nRFRFRFRF" }));

        [Fact]
        public void YShouldThrowIfMoreThan50() =>
            Assert.Throws<ArgumentException>(() => Run(new[] { "5 3\n5 51 E\nRFRFRFRF" }));
    }
}

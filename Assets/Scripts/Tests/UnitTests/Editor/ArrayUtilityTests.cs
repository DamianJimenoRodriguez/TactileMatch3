using NUnit.Framework;
using Tactile.TactileMatch3Challenge.Model;

namespace Tactile.TactileMatch3Challenge.Tests.UnitTests
{
    public class ArrayUtilityTests {
        
        [Test]
        public void ArrayUtility_GivenArray_ArrayIsTransposed() {
            // Arrange
            int[,] array = {
                {0, 1, 2, 3, 4},
            };

            // Act
            var transposed = ArrayUtility.TransposeArray(array);
            
            // Assert
            int[,] expectedTransposed = {
                {0},
                {1},
                {2}, 
                {3}, 
                {4}
            };

            Assert.That(transposed,Is.EqualTo(expectedTransposed));
        }
    }
}
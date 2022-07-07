using Tactile.TactileMatch3Challenge.Model;

namespace Tactile.TactileMatch3Challenge.Tests.UnitTests
{
    public class PieceSpawnerFake : IPieceSpawner {
        
        private readonly int value;
        
        public PieceSpawnerFake(int value) {
            this.value = value;
        }

        public int CreateBasicPiece() {
            return value;
        }
    }
}
namespace Tactile.TactileMatch3Challenge.Model {
    
    public class Piece {
        
        public int type { get; set; }
        
        public override string ToString() {
            return string.Format("(type:{0})",type);
        }
        
    }
    
}
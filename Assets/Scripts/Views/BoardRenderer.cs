using System.Collections.Generic;
using System.Linq;
using Tactile.TactileMatch3Challenge.Model;
using UnityEngine;
using Unity.Collections;
using System.Collections;

namespace Tactile.TactileMatch3Challenge.ViewComponents {

	public class BoardRenderer : MonoBehaviour {
		
		[SerializeField] private PieceTypeDatabase pieceTypeDatabase;
		[SerializeField] private VisualPiece visualPiecePrefab;
		
		private Board board;

		private VisualPiece[,] VisualPieces;

		public bool CanClick = true;


		public void Initialize(Board board) {
			this.board = board;
			VisualPieces = new VisualPiece[board.Width, board.Height];
			CenterCamera();
			CreateVisualPiecesFromBoardState();
			board.OnRemovePiece += RemovePieceAt;
		}

		private void CenterCamera() {
			Camera.main.transform.position = new Vector3((board.Width-1)*0.5f,-(board.Height-1)*0.5f);
		}

		private void CreateVisualPiecesFromBoardState() {
		

			foreach (var pieceInfo in board.IteratePieces()) {
				
				var visualPiece = CreateVisualPiece(pieceInfo.piece);
				visualPiece.transform.localPosition = LogicPosToVisualPos(pieceInfo.pos.x, pieceInfo.pos.y);
				VisualPieces[pieceInfo.pos.x, pieceInfo.pos.y] = visualPiece;
			}
		}
		
		public Vector3 LogicPosToVisualPos(float x,float y) { 
			return new Vector3(x, -y, -y);
		}

		private BoardPos ScreenPosToLogicPos(float x, float y) { 
			
			var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(x,y,-Camera.main.transform.position.z));
			var boardSpace = transform.InverseTransformPoint(worldPos);

			return new BoardPos() {
				x = Mathf.RoundToInt(boardSpace.x),
				y = -Mathf.RoundToInt(boardSpace.y)
			};

		}

		private VisualPiece CreateVisualPiece(Piece piece) {
			
			var pieceObject = Instantiate(visualPiecePrefab, transform, true);
			var sprite = pieceTypeDatabase.GetSpriteForPieceType(piece.type);
			pieceObject.SetSprite(sprite);
			return pieceObject;
			
		}

		private void DestroyVisualPieces() {
			foreach (var visualPiece in GetComponentsInChildren<VisualPiece>()) {
				Object.Destroy(visualPiece.gameObject);
			}
		}

		private void HandleChanges(Dictionary<Piece, ChangeInfo> changes)
		{
			for (int index = 0; index < changes.Count; index++)
			{
				var item = changes.ElementAt(index);
				VisualPiece visualPiece = null;
				Vector3 to = LogicPosToVisualPos(item.Value.ToPos.x, item.Value.ToPos.y);
				Vector3 from = LogicPosToVisualPos(item.Value.FromPos.x, item.Value.FromPos.y);

				if (item.Value.WasCreated)
                {
					visualPiece = CreateVisualPiece(item.Key);
					VisualPieces[item.Value.ToPos.x, item.Value.ToPos.y] = visualPiece;
					StartCoroutine(MovePieces(visualPiece, from, to, 2, 2));
				}
                else
                {
					visualPiece = VisualPieces[item.Value.FromPos.x, item.Value.FromPos.y];
					VisualPieces[item.Value.ToPos.x, item.Value.ToPos.y] = visualPiece;	
				StartCoroutine(MovePieces(visualPiece, from, to,2,0));
				}
                
			}
		}

		public IEnumerator MovePieces(VisualPiece piece, Vector3 From, Vector3 To, float LerpTime, float waitTime)
        {
			yield return new WaitForSeconds(waitTime);
			float elapsedTime = 0;
            while (elapsedTime< LerpTime)
            {
				elapsedTime += Time.deltaTime;
				yield return null;
				piece.transform.localPosition = Vector3.Lerp(From, To, elapsedTime / LerpTime);

			}
        }
		public IEnumerator CoolDown()
		{
			CanClick = false;
			yield return new WaitForSeconds(2);
			CanClick = true;
		}

		public void RemovePieceAt(int x, int y)
		{
            if (VisualPieces[x,y] != null)
            {
				Destroy(VisualPieces[x, y].gameObject);
			}
		}


		private void Update() {
			
			if (Input.GetMouseButtonDown(0) && CanClick) {

				var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);

				if (board.IsWithinBounds(pos.x, pos.y)) {
					ResolveResult result = board.Resolve(pos.x, pos.y);
					HandleChanges(result.changes);
					StartCoroutine(CoolDown());
				}

			}
		}
		
	}





}

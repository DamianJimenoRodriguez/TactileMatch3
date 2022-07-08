using System.Collections.Generic;
using System.Linq;
using Tactile.TactileMatch3Challenge.Model;
using UnityEngine;
using Unity.Collections;
using System.Collections;

namespace Tactile.TactileMatch3Challenge.ViewComponents
{
    public class BoardRenderer : MonoBehaviour
    {
        [SerializeField] private PieceTypeDatabase pieceTypeDatabase;
        [SerializeField] private VisualPiece visualPiecePrefab;

        private Board board;

        private VisualPiece[,] VisualPieces;

        private bool CanClick = true;

        [SerializeField] private Transform[] ScoreBoardPositions;
        [SerializeField] private float FallSpeed = 25;
        [SerializeField] private float ResolveSpeed = 15;
        [SerializeField] private float CreationTime = 15;
        public void Initialize(Board board)
        {
            this.board = board;
            VisualPieces = new VisualPiece[board.Width, board.Height];
            CenterCamera();
            CreateVisualPiecesFromBoardState();
            board.OnRemovePiece += RemovePieceAt;
        }

        private void CenterCamera()
        {
            Camera.main.transform.position = new Vector3((board.Width - 1) * 0.5f, -(board.Height - 1) * 0.5f);
        }

        private void CreateVisualPiecesFromBoardState()
        {
            foreach (var pieceInfo in board.IteratePieces())
            {
                var visualPiece = CreateVisualPiece(pieceInfo.piece);
                visualPiece.transform.localPosition = LogicPosToVisualPos(pieceInfo.pos.x, pieceInfo.pos.y);
                VisualPieces[pieceInfo.pos.x, pieceInfo.pos.y] = visualPiece;
            }
        }

        public Vector3 LogicPosToVisualPos(float x, float y)
        {
            return new Vector3(x, -y, -y);
        }

        private BoardPos ScreenPosToLogicPos(float x, float y)
        {
            var worldPos = Camera.main.ScreenToWorldPoint(new Vector3(x, y, -Camera.main.transform.position.z));
            var boardSpace = transform.InverseTransformPoint(worldPos);

            return new BoardPos()
            {
                x = Mathf.RoundToInt(boardSpace.x),
                y = -Mathf.RoundToInt(boardSpace.y)
            };
        }

        private VisualPiece CreateVisualPiece(Piece piece)
        {
            var pieceObject = Instantiate(visualPiecePrefab, transform, true);
            var sprite = pieceTypeDatabase.GetSpriteForPieceType(piece.type);
            pieceObject.SetSprite(sprite);
            return pieceObject;
        }

        private void DestroyVisualPieces()
        {
            foreach (var visualPiece in GetComponentsInChildren<VisualPiece>())
            {
                Object.Destroy(visualPiece.gameObject);
            }
        }


        private IEnumerator HandleChanges(Dictionary<Piece, ChangeInfo> changes)
        {
            CanClick = false;
            for (int index = 0; index < changes.Count; index++)
            {
                var item = changes.ElementAt(index);
                VisualPiece visualPiece = null;
                Vector3 to = LogicPosToVisualPos(item.Value.ToPos.x, item.Value.ToPos.y);
                Vector3 from = LogicPosToVisualPos(item.Value.FromPos.x, item.Value.FromPos.y);
                if (!item.Value.WasCreated)
                {
                    visualPiece = VisualPieces[item.Value.FromPos.x, item.Value.FromPos.y];
                    yield return StartCoroutine(MovePieces(visualPiece, from, to,FallSpeed));
                }
                else 
                {
                    visualPiece = CreateVisualPiece(item.Key);
                    from.y += 2;
                    visualPiece.transform.localPosition = from;
                    StartCoroutine(CretatePieceAnimation(visualPiece.transform, Vector3.zero, Vector3.one, CreationTime));
                    yield return StartCoroutine(MovePieces(visualPiece, from, to,FallSpeed));
                }
                VisualPieces[item.Value.ToPos.x, item.Value.ToPos.y] = visualPiece;
            }
            CanClick = true;
        }

        public IEnumerator CretatePieceAnimation(Transform transform, Vector3 Initial, Vector3 Final, float CreateTime)
        {
            float elapdedTime = 0;
            transform.localScale = Initial;
            while (elapdedTime < CreateTime)
            {
                elapdedTime += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Initial, Final, elapdedTime / CreateTime);
                yield return null;
            }
        }


        public IEnumerator MovePieces(VisualPiece piece, Vector3 From, Vector3 To, float speed)
        {
            piece.transform.localPosition = From;

            while (Vector3.Distance(piece.transform.localPosition, To) > 0.01f)
            {
                piece.transform.localPosition = Vector3.MoveTowards(piece.transform.localPosition, To, speed * Time.deltaTime);
                yield return null;
            }
        }

        public void RemovePieceAt(int x, int y, int type)
        {
            if (VisualPieces[x, y] != null)
            {
                StartCoroutine(RemovePiecesProcess(x, y, type));
            }
        }

        public IEnumerator RemovePiecesProcess(int x, int y, int type)
        {
            VisualPiece piece = VisualPieces[x, y];
            piece.GetComponent<SpriteRenderer>().sortingOrder = 32767;
            yield return StartCoroutine(MovePieces(piece, LogicPosToVisualPos(x, y), ScoreBoardPositions[type].position,ResolveSpeed));
            Destroy(piece.gameObject);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && CanClick)
            {
                var pos = ScreenPosToLogicPos(Input.mousePosition.x, Input.mousePosition.y);

                if (board.IsWithinBounds(pos.x, pos.y))
                {
                    ResolveResult result = board.Resolve(pos.x, pos.y);
                    StartCoroutine(HandleChanges(result.changes));
                }
            }
         
        }
    }
}
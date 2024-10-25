using UnityEngine;
using DG.Tweening;
namespace ColorBlocks
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] meshRenderer;
        [SerializeField] private Material[] materials;
        [SerializeField] private Block smashBlock;
        [SerializeField] private Transform gateTransform;
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int Direction { get; private set; }
        public int Color { get; private set; }

        private Vector3 _startPosition;
        private Vector3 _gateStartPosition;

        public void Init(int row, int col, int direction, int color)
        {
            Row = row;
            Col = col;
            Direction = direction;
            Color = color;
            foreach (var renderer in meshRenderer)
            {
                renderer.material = materials[color];
            }
            smashBlock.gameObject.SetActive(false);
            _startPosition = smashBlock.transform.position;
            _gateStartPosition = gateTransform.position;
        }

        public bool IsMatch(int color, int direction)
        {
            return Color == color && Direction == direction;
        }

        public void SmashBlock(Block block)
        {
            block.gameObject.SetActive(false);
            smashBlock.gameObject.SetActive(true);
            smashBlock.SetValues(0, 0, block.Directions, block.Color, block.Length);
            var targetPosition = _startPosition - Vector3.up * 3f;
            Sequence sequence = DOTween.Sequence();
            smashBlock.transform.position = _startPosition;
            sequence.Insert(0, smashBlock.transform.DOShakePosition(0.5f, 0.25f, 25, 90, false, true));
            sequence.Insert(0, smashBlock.transform.DOMoveY(targetPosition.y, 0.5f).SetEase(Ease.InQuad));
            
            Sequence gateSequence = DOTween.Sequence();
            gateSequence.Append(gateTransform.DOMove(_gateStartPosition - Vector3.up * 1.5f, 0.1f).SetEase(Ease.InQuad));
            gateSequence.Append(gateTransform.DOMove(_gateStartPosition, 0.75f));
            sequence.OnComplete(() => smashBlock.gameObject.SetActive(false));
        }

    }
}

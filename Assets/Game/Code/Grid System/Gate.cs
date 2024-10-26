using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
namespace ColorBlocks
{
    public class Gate : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] meshRenderer;
        [SerializeField] private Material[] materials;
        [SerializeField] private Block smashBlock;
        [SerializeField] private Transform shakeObject;
        [SerializeField] private Transform gateTransform;
        public int Row { get; private set; }
        public int Col { get; private set; }
        public int Direction { get; private set; }
        public int Color { get; private set; }

        private Vector3 _smashblockStartPosition;
        private Vector3 _shakeObjectStartPosition;
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
            _smashblockStartPosition = smashBlock.transform.position;
            _shakeObjectStartPosition = shakeObject.localPosition;
            _gateStartPosition = gateTransform.position;
        }

        public bool IsMatch(int color, int direction)
        {
            return Color == color && Direction == direction;
        }

        public void SmashBlock(Block block)
        {
            StartCoroutine(SmashBlockCoroutine(block));
        }

        private IEnumerator SmashBlockCoroutine(Block block)
        {
            yield return new WaitForSeconds(0.15f);

            block.gameObject.SetActive(false);
            smashBlock.gameObject.SetActive(true);
            smashBlock.SetValues(0, 0, block.Directions, block.Color, block.Length);
            var targetPosition = _smashblockStartPosition - Vector3.up * 3f;
            Sequence sequence = DOTween.Sequence();
            smashBlock.transform.position = _smashblockStartPosition;
            shakeObject.localPosition = _shakeObjectStartPosition;
            sequence.Append(smashBlock.transform.DOMove(targetPosition, 1f).SetEase(Ease.Linear));
            sequence.Join(shakeObject.transform.DOShakePosition(1f, new Vector3(0.025f, 0f, 0.025f), 25, 90, false, true));
            sequence.Join(shakeObject.transform.DOShakeRotation(1f, new Vector3(1, 0, 1), 10, 90, false));

            Sequence gateSequence = DOTween.Sequence();
            gateSequence.Append(gateTransform.DOMove(_gateStartPosition - Vector3.up * 1.5f, 0.1f).SetEase(Ease.InQuad));
            gateSequence.Append(gateTransform.DOMove(_gateStartPosition, 0.75f));
            
            sequence.OnComplete(() =>
            {
                smashBlock.gameObject.SetActive(false);
            });
        }

    }
}

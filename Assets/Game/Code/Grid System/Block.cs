using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
namespace ColorBlocks
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Material[] _colorMaterials;

        public int Row { get; private set; }
        public int Col { get; private set; }
        public List<int> Directions { get; private set; }
        public int Color { get; private set; }
        public int Length { get; private set; }

        public Vector3 CenterPosition
        {
            get
            {
                return transform.position + _meshRenderer.bounds.center - transform.position;
            }
        }

        public Vector3 TopPosition
        {
            get
            {
                return transform.position + _meshRenderer.bounds.extents.y * Vector3.up;
            }
        }

        public void SetValues(int row, int col, List<int> directions, int color, int length)
        {
            Row = row;
            Col = col;
            Directions = directions;
            Color = color;
            Length = length;
            _meshRenderer.material = _colorMaterials[color];
        }

        public void Move(int newRow, int newCol, float moveTime)
        {
            Row = newRow;
            Col = newCol;
            transform.DOMove(Grid.Instance.GetCellPosition(newRow, newCol), moveTime);
        }

        public bool CanMove(int direction)
        {
            return Directions.Contains(direction);
        }
    }
}

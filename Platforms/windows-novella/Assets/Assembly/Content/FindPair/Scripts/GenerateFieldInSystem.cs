using System.Collections.Generic;
using Leopotam.EcsProto.QoL;
using Leopotam.EcsProto;
using UnityEngine;

namespace Assembly.FindPair
{
    public sealed class GenerateFieldInSystem : MonoBehaviour, IProtoInitSystem, IProtoDestroySystem
    {
        [DI] private readonly AspectToField _aspectToField = null;

        [SerializeField] private RectTransform _storage = null;
        [SerializeField] private Sprite[] _pictures = null;
        [SerializeField] private ViewToCell _prefab = null;

        private ViewToCell[] _cellsUsed = new ViewToCell[0];

        public void Init(IProtoSystems systems)
        {
            int[] dir = new int[] { -1, 0, 1, 2 };
            List<int> necessaryToAssignPictures = new();
            int count = 0;
            int max = dir.Length * dir.Length;
            _cellsUsed = new ViewToCell[max];
            for (int x = 0; x < dir.Length; x++)
            {
                for (int y = 0; y < dir.Length; y++)
                {
                    int indexPictureUsed = Random.Range(0, _pictures.Length);

                    if (necessaryToAssignPictures.Contains(indexPictureUsed))
                        necessaryToAssignPictures.Remove(indexPictureUsed);
                    else if (max - count <= necessaryToAssignPictures.Count)
                    {
                        indexPictureUsed = necessaryToAssignPictures[Random.Range(0, necessaryToAssignPictures.Count)];
                        necessaryToAssignPictures.Remove(indexPictureUsed);
                    }
                    else
                        necessaryToAssignPictures.Add(indexPictureUsed);

                    Vector2 index = new(dir[x], dir[y]);
                    _cellsUsed[count] = Create(index, indexPictureUsed);
                    count++;
                }
            }
        }
        public void Destroy()
        {
            for (int index = 0; index < _cellsUsed.Length; index++)
                Destroy(_cellsUsed[index].gameObject);
        }
        private ViewToCell Create(Vector2 index, int indexPictureUsed)
        {
            Sprite picture = _pictures[indexPictureUsed];
            ViewToCell cell = ViewToCell.Instantiate(_prefab, _storage, false);
            _aspectToField.PoolByCell.NewEntity(out ProtoEntity entity) = new(cell, indexPictureUsed);

            Vector2 position = index * cell.Root.sizeDelta;
            cell.name = entity.ToString();
            cell.Picture.sprite = picture;
            cell.Root.localPosition = _storage.rect.center - position;
            return cell;
        }
    }
}
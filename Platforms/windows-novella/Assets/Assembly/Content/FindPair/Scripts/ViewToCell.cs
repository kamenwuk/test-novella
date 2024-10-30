using UnityEngine.UI;
using UnityEngine;

namespace Assembly.FindPair
{
    public sealed class ViewToCell : MonoBehaviour
    {
        [field: SerializeField] public RectTransform Root { get; private set; }
        [field: SerializeField] public Image Picture { get; private set; }
        [field: SerializeField] public Toggle Toggle { get; private set; }
    }
}

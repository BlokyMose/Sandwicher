using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandwicher
{
    [InlineEditor]
    [CreateAssetMenu(fileName = "Tol_", menuName = "SO/Tolerance")]
    public class Tolerance : ScriptableObject
    {
        [SerializeField, Range(0f,1f)]
        float toleratedRange;

        [SerializeField, Range(0f,1f)]
        float rejectedRange;

        public float ToleratedRange { get => toleratedRange; }
        public float RejectedRange { get => rejectedRange; }
    }
}

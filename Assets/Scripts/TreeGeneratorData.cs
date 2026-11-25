using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    [Serializable]
    [CreateAssetMenu]
    public class TreeGeneratorData : ScriptableObject
    {
        [Header("Settings")]
        public string Axiom = "FB";
        public int Iterations = 6;
        public Vector2 Lenght = new Vector2(1, 2);
        public float AngleMin;
        public float AngleMax;
        public float AngleYMin; 
        public float AngleYMax;
        public Material TreeMaterial;
        
        [Header("Radius")]
        public float BaseRadius = 0.5f;
        [Range(0.1f, 1)] public float RadiusRatio = 0.95f;
        
        [Header("Leaves")]
        public GameObject LeafPrefab;
        public float LeafScale = 0.3f;
        public int LeavesPerBranch = 3;
        
        [Header("Rules")] 
        public List<BranchRules> BranchRules = new List<BranchRules>(); 
    }
}
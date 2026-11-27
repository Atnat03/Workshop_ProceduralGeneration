using System;
using System.Collections.Generic;
using UnityEngine;

namespace TreeGeneration
{
    [Serializable]
    public class BranchRules
    {
        public char Rule;
        public List<BranchRulesProbability> BranchRulesProbabilityList = new List<BranchRulesProbability>();
    }
    
    [Serializable]
    public class BranchRulesProbability
    {
        public string Rule;
        [Range(0, 100)] public float Probability;
    }
}
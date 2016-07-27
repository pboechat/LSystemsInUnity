using System.Collections.Generic;
using System;

public class ProductionRuleSet
{
	private Dictionary<string, List<ProductionRule>> _lookupTable = new Dictionary<string, List<ProductionRule>> ();
	
	public void Add (ProductionRule productionRule)
	{
		List<ProductionRule> list;
		if (!_lookupTable.ContainsKey (productionRule.predecessor)) {
			list = new List<ProductionRule> ();
			_lookupTable [productionRule.predecessor] = list;
		} else {
			list = _lookupTable [productionRule.predecessor];
		}
		list.Add (productionRule);
	}
	
	public bool Contains (string module)
	{
		return _lookupTable.ContainsKey (module);
	}
	
	public ProductionRule Match (string module)
	{
		if (!_lookupTable.ContainsKey (module)) {
			return null;
		}
		
		List<ProductionRule> list = _lookupTable [module];
		
		if (list.Count == 1) {
			return list [0];
		}
		
		float chance = UnityEngine.Random.value;
		
		foreach (ProductionRule productionRule in list) {
			if (productionRule.probability <= chance) {
				return productionRule;
			}
		}
		
		// TODO: throw an assertion!
		throw new Exception ("Should never happen!");
	}
	
	public bool CheckProbabilities ()
	{
		foreach (List<ProductionRule> list in _lookupTable.Values) {
			// Shortcut for modules with only 1 production rule
			if (list.Count == 1 && list [0].probability != 1) {
				return false;
			}
			
			float acc = 0;
			foreach (ProductionRule productionRule in list) {
				acc += productionRule.probability;
			}
			
			if (acc != 1) {
				return false;
			}
		}
	
		return true;
	}
	
}
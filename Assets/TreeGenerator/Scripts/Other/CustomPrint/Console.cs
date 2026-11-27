using System.Collections.Generic;
using UnityEngine;

namespace TreePrint
{
	public class TreePrint
	{
		
		#region Classic Print
		
		// ReSharper disable Unity.PerformanceAnalysis
		public static void Print(string msg)
		{
			Debug.Log(msg);
		}
		
		//Print avec une couleur et un style
		public static void Print(string msg, TreePrintColor treePrintColor, ConsoleStyle style)
		{
			(string, string) styleStr = TreePrintOption.GetStyle(style);
			
			 string s = $"{styleStr.Item1}" +
			            $"{TreePrintOption.GetColor(treePrintColor)} " +
			            $"{msg}" +
			            "</color> " +
			            $"{styleStr.Item2}";
			
			Debug.Log(s);
		}
		
		//Print avec une couleur uniquement
		public static void Print(string msg, TreePrintColor treePrintColor)
		{
			 string s = "" +
			            $"{TreePrintOption.GetColor(treePrintColor)}" +
			            $"{msg}" +
			            "</color>";
			
			Debug.Log(s);
		}
		
		//Print avec un style uniquement
		public static void Print(string msg, ConsoleStyle style)
		{
			(string, string) styleStr = TreePrintOption.GetStyle(style);
			
			string s = $"{styleStr.Item1}" +
			           $"{msg}" +
			           "</color> " +
			           $"{styleStr.Item2}";
			
			Debug.Log(s);
		}
		#endregion
		
		#region List Print
		
		//Print List element par element
		public static void PrintList<T>(List<T> list, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Count; i++)
			{
				s += $"Element {i} : {list[i]} \n";
			}
			
			Debug.Log(s);
		}
		
		//Print List element par element avec une couleur
		public static void PrintList<T>(List<T> list, TreePrintColor treePrintColorElement, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Count; i++)
			{
				s += $"Element {i} : {TreePrintOption.GetColor(treePrintColorElement)}{list[i]}</color> \n";
			}
			
			Debug.Log(s);
		}

		
		#endregion
		
		#region Array Print
		
		//Print Array element par element
		public static void PrintList<T>(T[] list, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Length; i++)
			{
				s += $"Element {i} : {list[i]} \n";
			}
			
			Debug.Log(s);
		}
		
		//Print Array element par element avec une couleur
		public static void PrintList<T>(T[] list, TreePrintColor treePrintColorElement, string listName = "")
		{
			string s = $"List {listName}\n";
			
			for (int i = 0; i < list.Length; i++)
			{
				s += $"Element {i} : {TreePrintOption.GetColor(treePrintColorElement)}{list[i]}</color> \n";
			}
			
			Debug.Log(s);
		}
		
		#endregion

		#region Bool Print
		public static void PrintBool(bool b, string boolName = "")
		{
			string s = $"{boolName} ";
			
			string color = b ? TreePrintOption.GetColor(TreePrintColor.Green) : TreePrintOption.GetColor(TreePrintColor.Red);
			
			s += $"{color}{b}</color>";
			
			Debug.Log(s);
		}
		#endregion

		#region Space Print
		public static void PrintSpace()
		{
			Debug.Log("──────────────────────────────────");
		}
		
		public static void PrintSpace(TreePrintColor treePrintColor)
		{
			Debug.Log($"{TreePrintOption.GetColor(treePrintColor)}──────────────────────────────────</color>");
		}
		#endregion
	}

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityUtility
{
	public class InspectorComment : MonoBehaviour
	{
		[TextArea(3, 15)]
		public string comment = "Hello, world!";
	} 
}

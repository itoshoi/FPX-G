using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimRecord
{
	public enum DataSet{
		WattsStrogatz, Amazon, Tree, BarabasiAlbert
	}

	public DataSet dataSet;
	public float graphDensity;
	// exp distribution lambda
	public float lambda;
	// hop count between first node and goal node
	public float distance;
	// buff of selecting goal probability
	public int goalPriority;
	// operation count
	public float opCount;
	// probability of select unknown node
	public float pSelectUnknown;
	// probability of return to first node
	public float pReturnFirst;
}

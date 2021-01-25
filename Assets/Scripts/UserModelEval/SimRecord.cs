using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimRecord
{
	public enum DataSet{
		WattsStrogatz, Amazon, Tree, BarabasiAlbert
	}

	public DataSet dataSet;
	// all node count
	public float nodeCount;
	// goal cound
	public int goalCount;
	// graph density
	public float graphDensity;
	// exp distribution lambda
	public float lambda;
	// hop count between first node and goal node
	public float distance;
	// operation count
	public float opCount;
	// probability of select unknown node
	public float pSelectUnknown;
	// probability of return to first node
	public float pReturnFirst;
	// probability of select goal when visible
	public float pSelectVisibleGoal;
}

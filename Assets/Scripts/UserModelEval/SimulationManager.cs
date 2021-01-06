using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public class SimulationManager : MonoBehaviour
{
	[SerializeField, Range(0, 1)] private float progress = 0;
	[SerializeField] private float startLambda = 0.1f;
	[SerializeField] private float endLambda = 2f;
	[SerializeField] private SimRecord.DataSet dataSet;
	[SerializeField] private int allNodeCount = 200;
	[SerializeField] private int meanDegree = 6;
	[SerializeField] private int distanceFtoG = 3;
	[SerializeField] private int goalPriority = 5;

    [SerializeField] private int maxEpoc = 1000;

	private void Start()
    {
		StartSimulation();
		// StartCoroutine(StartSimulationCoroutine());
		// StartCoroutine(SimulationCoroutine(0.1f, 0));
    }

	private IEnumerator StartSimulationCoroutine(){
        Debug.Log("Start Simulation");
        for (float l = startLambda; l < endLambda; l += 0.1f)
        {
			yield return StartCoroutine(SimulationCoroutine(l, 0));
        }

		// wiki data
		yield return StartCoroutine(SimulationCoroutine(10, 0.1f));

        Debug.Log("All Done.");
		yield break;
	}

	private async void StartSimulation(){
        Debug.Log("Start Simulation");

		var tasks = new List<Task>();
        for (float l = startLambda; l < endLambda; l += 0.1f)
        {
			var task_l = l;
			var task = Task.Run(() => {Simulate(task_l, 0);});
			// await task;
			tasks.Add(task);
        }

		// wiki data
		// tasks.Add(Task.Run(() => {Simulate(10, 0.1f);}));

		await Task.WhenAll(tasks);
        Debug.Log("All Done.");
	}

	private void Simulate(float lambda, float pReturnFirst){
        Debug.Log(lambda + " Start.");

		var userModel = new VRInterfaceModel();
        userModel.Lambda = lambda;
        userModel.dataSet = dataSet;
		userModel.allNodeCount = allNodeCount;
		userModel.meanDegree = meanDegree;
		userModel.distanceFtoG = distanceFtoG;
		userModel.goalPriority = goalPriority;
		// 始点ノードに戻る確率
		userModel.pReturnFirst = pReturnFirst;
        for (int i = 0; i < maxEpoc; i++)
        {
			progress = (float)i / (float)maxEpoc;
            userModel.Simulate();
			Thread.Sleep(16);
        }

        SimDataWriter.WriteData(userModel.Records, dataSet, "lambda_" + lambda.ToString("F1").Replace(".", "_") + ".csv");
        Debug.Log(lambda + " Done.");
	}

    private IEnumerator SimulationCoroutine(float lambda, float pReturnFirst)
    {
        Debug.Log(lambda + " Start.");

		var userModel = new VRInterfaceModel();
        userModel.Lambda = lambda;
        userModel.dataSet = dataSet;
		userModel.allNodeCount = allNodeCount;
		userModel.meanDegree = meanDegree;
		userModel.distanceFtoG = distanceFtoG;
		userModel.goalPriority = goalPriority;
		// 始点ノードに戻る確率
		userModel.pReturnFirst = pReturnFirst;
        for (int i = 0; i < maxEpoc; i++)
        {
			progress = (float)i / (float)maxEpoc;
            userModel.Simulate();

            yield return null;
        }

        SimDataWriter.WriteData(userModel.Records, dataSet, "lambda_" + lambda.ToString("F1").Replace(".", "_") + ".csv");
        Debug.Log(lambda + " Done.");
        yield break;
    }
}

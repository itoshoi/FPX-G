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
	[SerializeField] private int goalNodeCount = 1;
	[SerializeField] private int meanDegree = 6;
	// [SerializeField] private int distanceFtoG = 3;
	[SerializeField] private float selectVisibleGoal = 0.5f;

    [SerializeField] private int maxEpoc = 1000;

	private async Task Start()
    {
		// await StartSimulation();
		await SimulateAll();
		// StartCoroutine(StartSimulationCoroutine());
		// StartCoroutine(SimulationCoroutine(0.1f, 0));
		Debug.Log("Simulation End");
    }

	private async Task SimulateAll(){
		var tasks = new List<Task>();
		// BarabasiAlbertのm2~m6までをシミュレート
		for(int m = 2; m <= 6; m++){
			Debug.Log("m = " + m);
			dataSet = SimRecord.DataSet.BarabasiAlbert;
			meanDegree = m * 2;
			await StartSimulation();
			// tasks.Add(StartSimulation());
		}
		// await Task.WhenAll(tasks);
		// tasks.Clear();

		// WattsStrogatzのk4 ~ k12までをシミュレート
		for(int k = 4; k <= 12; k += 2){
			Debug.Log("k = " + k);
			dataSet = SimRecord.DataSet.WattsStrogatz;
			meanDegree = k;
			await StartSimulation();
			// tasks.Add(StartSimulation());
		}
		// await Task.WhenAll(tasks);
		// tasks.Clear();

		// Treeをシミュレート
		dataSet = SimRecord.DataSet.Tree;
		await StartSimulation();
		// tasks.Add(StartSimulation());
		// await Task.WhenAll(tasks);
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

	private async Task StartSimulation(){
        Debug.Log("Start Simulation");

		var tasks = new List<Task>();
        for (float l = startLambda; l < endLambda; l += 0.2f)
        {
			var task_l = l;
			var task = Task.Run(() => {Simulate(task_l, 0);});
			// await task;
			tasks.Add(task);
        }

		// wiki data
		tasks.Add(Task.Run(() => {Simulate(10, 0.1f);}));

		await Task.WhenAll(tasks);
        Debug.Log("All Done.");
	}

	private void Simulate(float lambda, float pReturnFirst){
        Debug.Log(lambda + " Start.");

		var userModel = new VRInterfaceModel();
        userModel.Lambda = lambda;
        userModel.dataSet = dataSet;
		userModel.allNodeCount = allNodeCount;
		userModel.goalCount = goalNodeCount;
		userModel.meanDegree = meanDegree;
		// userModel.distanceFtoG = distanceFtoG;
		userModel.pSelectVisibleGoal = selectVisibleGoal;
		// 始点ノードに戻る確率
		userModel.pReturnFirst = pReturnFirst;
        for (int i = 0; i < maxEpoc; i++)
        {
			progress = (float)i / (float)maxEpoc;
            userModel.Simulate();
			Thread.Sleep(16);
        }

        SimDataWriter.WriteData(userModel.Records, dataSet, "lambda_" + lambda.ToString("F1").Replace(".", "_") + ".csv", userModel);
        Debug.Log(lambda + " Done.");
	}

    private IEnumerator SimulationCoroutine(float lambda, float pReturnFirst)
    {
        Debug.Log(lambda + " Start.");

		var userModel = new VRInterfaceModel();
        userModel.Lambda = lambda;
        userModel.dataSet = dataSet;
		userModel.allNodeCount = allNodeCount;
		userModel.goalCount = goalNodeCount;
		userModel.meanDegree = meanDegree;
		// userModel.distanceFtoG = distanceFtoG;
		// 始点ノードに戻る確率
		userModel.pReturnFirst = pReturnFirst;
        for (int i = 0; i < maxEpoc; i++)
        {
			progress = (float)i / (float)maxEpoc;
            userModel.Simulate();

            yield return null;
        }

        SimDataWriter.WriteData(userModel.Records, dataSet, "lambda_" + lambda.ToString("F1").Replace(".", "_") + ".csv", userModel);
        Debug.Log(lambda + " Done.");
        yield break;
    }
}

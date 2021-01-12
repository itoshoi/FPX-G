using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;

public class SimDataWriter : MonoBehaviour
{
	// graphModelParams[0] is nodeCount
	// graphModelParams[1] is meanDegree
    public static void WriteData(SimRecord[] simRecords, SimRecord.DataSet dataSet, string fileName, VRInterfaceModel model)
    {
        // var fileDir = Application.dataPath + "/Data/Simulation/" + dataSet.ToString();
        var fileDir = "/media/GDrive/ToyamaLab/VRInterface/Assets" + "/Data/Simulation/SelectVisibleGoal" + (model.pSelectVisibleGoal * 100) + "/" + dataSet.ToString();
		switch(dataSet){
			case SimRecord.DataSet.BarabasiAlbert:
				fileDir += "_m" + (model.meanDegree / 2).ToString("F0");
				fileDir += "_n" + (model.allNodeCount).ToString("F0");
				break;
			case SimRecord.DataSet.WattsStrogatz:
				fileDir += "_k" + (model.meanDegree).ToString("F0");
				fileDir += "_n" + (model.allNodeCount).ToString("F0");
				break;
		}

        Directory.CreateDirectory(fileDir);
        var filePath = fileDir + "/" + fileName;
        StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_JIS"));
        sw.WriteLine("DataSet,NodeCount,GraphDensity,UserModel(lambda),Distance,ProbSelectUnknown,ProbReturnFirst,ProbSelectVisibleGoal,OperationCount");
        foreach (var record in simRecords)
        {
            var str = record.dataSet.ToString();
            str += "," + record.nodeCount;
            str += "," + record.graphDensity.ToString("F5");
            str += "," + record.lambda.ToString("F1");
            str += "," + record.distance;
            str += "," + record.pSelectUnknown;
            str += "," + record.pReturnFirst;
            str += "," + record.pSelectVisibleGoal;
            str += "," + record.opCount;
            sw.WriteLine(str);
        }
        sw.Close();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Text;

public class SimDataWriter : MonoBehaviour
{
    public static void WriteData(SimRecord[] simRecords, SimRecord.DataSet dataSet, string fileName)
    {
        var fileDir = Application.dataPath + "/Data/Simulation/" + dataSet.ToString();
        Directory.CreateDirectory(fileDir);
        var filePath = fileDir + "/" + fileName;
        StreamWriter sw = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift_JIS"));
        sw.WriteLine("DataSet,NodeCount,GraphDensity,UserModel(lambda),Distance,ProbSelectUnknown,ProbReturnFirst,GoalPriority,OperationCount");
        foreach (var record in simRecords)
        {
            var str = record.dataSet.ToString();
            str += "," + record.nodeCount;
            str += "," + record.graphDensity.ToString("F5");
            str += "," + record.lambda.ToString("F1");
            str += "," + record.distance;
            str += "," + record.pSelectUnknown;
            str += "," + record.pReturnFirst;
            str += "," + record.goalPriority;
            str += "," + record.opCount;
            sw.WriteLine(str);
        }
        sw.Close();
    }
}


// ===============================
// RhythmMapTable.cs
// ===============================

using System;

[Serializable]
public class RhythmMapTable
{
    public float[] hitTimeStamps;
    public float[,] hitDurations;

    public RhythmMapTable(int rowCount, int columnCount)
    {
        hitTimeStamps = new float[rowCount];
        hitDurations = new float[rowCount, columnCount];
    }
}

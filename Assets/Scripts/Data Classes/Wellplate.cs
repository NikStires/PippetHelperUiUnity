using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wellplate : LabMaterial
{
    public int numWells;
    public Dictionary<string, Well> wells;

    public Wellplate(int id, string name, int numberOfWells) : base(id, name, numberOfWells)
    {
        wells = new Dictionary<string, Well>();
    }

    public override bool ContainsWell(string wellID)
    {
        return wells.ContainsKey(wellID);
    }

    public override void AddWell(string wellID, Well newWell)
    {
        wells.Add(wellID, newWell);
    }

    public override Well GetWell(string wellID)
    {
        return wells[wellID];
    }

    public override Dictionary<string, Well> GetWells()
    {
        return wells;
    }
}

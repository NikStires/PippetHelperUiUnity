using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabAction
{
    public enum ActionType
    { 
        Pipette,
        Transfer,
        Dilution
    }

    public struct Source
    {
        public string matID;
        public string matSubID;
        public Color color;
        public string colorName;
        public float volume;
        public string units;

        public Source(string materialID, string materialSubID, Color sourceColor, string sourceColorName, float sourceVolume, string sourceVolUnits)
        {
            matID = materialID;
            matSubID = materialSubID;
            color = sourceColor;
            colorName = sourceColorName;
            volume = sourceVolume;
            units = sourceVolUnits;
        }
    }

    public struct Target
    {
        public string matID;
        public string matSubID;
        public Color color;
        public string colorName;

        public Target(string materialID, string materialSubID, Color targetColor, string targetColorName)
        {
            matID = materialID;
            matSubID = materialSubID;
            color = targetColor;
            colorName = targetColorName;
        }
    }

    public ActionType type;
    public Source source;
    public Target target;

    public LabAction(ActionType action, Source sourceMat, Target targetMat)
    {
        type = action;
        source = sourceMat;
        target = targetMat;
    }
}

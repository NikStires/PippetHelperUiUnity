using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using SFB;//Copyright (c) 2017 G�khan G�k�e Under MIT License

public class ProcedureLoader : MonoBehaviour
{
    public static Subject<bool> procedureStream = new Subject<bool>();

    private string folderPath;

    private string[] fileName;

    // Start is called before the first frame update
    void Start()
    {
        var extensionList = new[] {
                new ExtensionFilter("Comma Seperated Variables", "csv"),
            };

#if UNITY_STANDALONE && !UNITY_EDITOR
        //check if protocol folder exists
        folderPath = Application.dataPath + "/../protocols";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        //open file
        fileName = StandaloneFileBrowser.OpenFilePanel("Open File", folderPath, extensionList, true); //Copyright (c) 2017 G�khan G�k�e Under MIT License
#endif
#if UNITY_EDITOR
        fileName = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensionList, true); //Copyright (c) 2017 G�khan G�k�e Under MIT License
#endif


        if (fileName.Count() > 0)
        {
            StartCoroutine(LoadProcedure(fileName[0]));
        }
        else
        {
            Application.Quit();
        }
    }

    ///loads procedure from csv (async to ensure all objects in scene are instantiated fully before loading)
    IEnumerator LoadProcedure(string fileName)
    {
        yield return new WaitForEndOfFrame();

        StreamReader sr = new StreamReader(fileName);

        string currentLine;

        string[] lineCells;

        bool firstStep = true;

        //read the file until the end of file is reached
        while ((currentLine = sr.ReadLine()) != null)
        {
            if(currentLine.Contains("plate:horizontal") || currentLine.Contains("plate:vertical"))
            {
                if(!firstStep)
                {
                    SessionState.AddNewStep();
                }
                else
                {
                    firstStep = false;
                }
            }
            else
            {
                lineCells = currentLine.Split(',');
                
                //cell 0 will always be blank
                //cell goes: "","wellID","#Hex","ColorName","SampleName:SampleAbreviation", "SampleVolume"
                string wellId = lineCells[1];
                Color color;
                ColorUtility.TryParseHtmlString(lineCells[2], out color);
                string colorName = lineCells[3];
                string[] nameAbrev = lineCells[4].Split(":");
                string SampleName = nameAbrev[0];
                string SampleAbbreviation = nameAbrev[1];
                float SampleVolume = float.Parse(lineCells[5], CultureInfo.InvariantCulture.NumberFormat);

                //add Sample to sessionState
                Sample newSample = new Sample(SampleName, SampleAbbreviation, colorName, color);
                SessionState.AddNewSample(newSample.name, newSample.abreviation, newSample.colorName, newSample.color);
                //set new Sample as active  
;               SessionState.ActiveSample = SessionState.AvailableSamples.Where(sample => sample.name == SampleName).FirstOrDefault();
                //set tool volume
                SessionState.ActiveTool.volume = SampleVolume;

                //if the well id has a colon this is a multichannel
                if(wellId.Contains(':'))
                {
                    int numChannels;
                    string activeWellId;
                    
                    //get the first and last well of the groups
                    string[] wellGroup = wellId.Split(':');

                    activeWellId = wellGroup[0];

                    //fill well group horizontal
                    if (wellGroup[0][0] == wellGroup[1][0])
                    {
                        numChannels = GetNumberChannels(wellGroup, true);
                        
                        while(numChannels > 0)
                        {
                            if (activeWellId == wellGroup[0])
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, true, false);
                                
                            }
                            else if(activeWellId == wellGroup[1])
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, false, true);
                                
                            }
                            else
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, false, false);
                                
                            }
                            numChannels--;
                            activeWellId = GetNextWellHorizontal(activeWellId);
                        }
                    }
                    //fill well group vertical
                    else
                    {
                        numChannels = GetNumberChannels(wellGroup, false);
                        while (numChannels > 0)
                        {
                            if (activeWellId == wellGroup[0])
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, true, false);
                                
                            }
                            else if (activeWellId == wellGroup[1])
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, false, true);
                                
                            }
                            else
                            {
                                SessionState.AddActiveSampleToWell(activeWellId, true, false, false);
                                
                            }
                            numChannels--;
                            activeWellId = GetNextWellVertical(activeWellId);
                        };
                    }
                }
                //else its a single well
                else
                {
                    SessionState.AddActiveSampleToWell(wellId, false, false, false);
                }
            }
        }
        SessionState.SetStep(0);
        procedureStream.OnNext(true);

        if(fileName != null)
        {
            SessionState.ProcedureName = Path.GetFileNameWithoutExtension(fileName);
        }
    }

    //returns number of channels from well group identifier
    int GetNumberChannels(string[] wellGroup, bool isHorizontal)
    {
        int startWellNumber;
        int endWellNumber;
        
        if (isHorizontal)
        {
            if (wellGroup[0].Length == 2)
            {
                startWellNumber = Int32.Parse(wellGroup[0][1].ToString());
            }
            else
            {
                char[] chars = { wellGroup[0][1], wellGroup[0][2] };
                startWellNumber = Int32.Parse(new string(chars));
            }
            if (wellGroup[1].Length == 2)
            {
                endWellNumber = Int32.Parse(wellGroup[1][1].ToString());
            }
            else
            {
                char[] chars = { wellGroup[1][1], wellGroup[1][2] };
                endWellNumber = Int32.Parse(new string(chars));
            }
        }
        else
        {
            startWellNumber = (wellGroup[0][0] - 64);
            endWellNumber = (wellGroup[1][0] - 64);
        }

        return endWellNumber - (startWellNumber - 1);
    }

    //returns the well id of the well to right
    string GetNextWellHorizontal(string currentWell)
    {
        int startWellNumber;

        if (currentWell.Length == 2)
        {
            startWellNumber = Int32.Parse(currentWell[1].ToString());
        }
        else
        {
            char[] chars = { currentWell[1], currentWell[2] };
            startWellNumber = Int32.Parse(new string(chars));
        }
        
        startWellNumber++;

        return new string(currentWell[0] + startWellNumber.ToString());
    }

    //returns the id of the well below
    string GetNextWellVertical(string currentWell)
    {
        string columnId = currentWell.Substring(1);

        char nextRowId = (char)(((int)currentWell[0]) + 1);

        return new string(nextRowId.ToString() + columnId);
    }
}

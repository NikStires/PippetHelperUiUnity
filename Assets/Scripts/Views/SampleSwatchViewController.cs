﻿using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UniRx;

public class SampleSwatchViewController : MonoBehaviour, IPointerDownHandler
{
    public TextMeshProUGUI sampleName;
    public TextMeshProUGUI abreviation;
    public Image swatch;

    public Button editButton;
    public Button deleteButton;

    void Start()
    {
        deleteButton.onClick.AddListener(delegate
        {
            if (sampleName.text != null)
            {
                SessionState.RemoveSample(sampleName.text);
                Destroy(gameObject);
                Debug.Log(SessionState.AvailableSamples.Count);
            }
        });

        SessionState.editedSampleStream.Subscribe(sampleNames =>
        {
            //if the edited samples old name is equal to this displays sample name update this display
            if (sampleNames.Item1 == sampleName.text)
            {
                Sample editedSample = SessionState.AvailableSamples.Where(sample => sample.sampleName == sampleNames.Item2).FirstOrDefault();
                if (editedSample != null)
                {
                    InitSampleItem(editedSample.sampleName, editedSample.abreviation, editedSample.color);
                }
            }
        });
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left & !SessionState.FormActive)
        {
            SessionState.SetActiveSample(SessionState.AvailableSamples.Where(x => x.sampleName.Equals(sampleName.GetComponent<TMP_Text>().text)).FirstOrDefault());
        }
    }

    public bool InitSampleItem(string SampleName, string SampleAbrev, Color displayColor)
    {
        if(swatch != null && sampleName != null && abreviation != null)
        {
            swatch.color = displayColor;
            sampleName.GetComponent<TMP_Text>().text = SampleName;
            abreviation.GetComponent<TMP_Text>().text = SampleAbrev;
            return true;
        }
        else return false;
    }
}

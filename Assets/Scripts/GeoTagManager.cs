using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using LitJson;
using System;

public class GeoTagManager : MonoBehaviour
{
    public Text statusText;
    public InputField geoTagName;
    public CanvasGroup createGeoTagCanvas;
    public GameObject displayPrefab;

    private bool isCreating = false;
    private string savePath;
    private List<GeoTag> geoTags = new List<GeoTag>();
    private LocationInfo currentLocation;

    private void Start()
    {
        savePath = Application.persistentDataPath + "/tags.json";
        FetchTags();
    }

    public void AddGeoTag()
    {
        createGeoTagCanvas.alpha = 1;
        createGeoTagCanvas.blocksRaycasts = true;
    }

    public void CreateNewGeoTag()
    {
        if (!isCreating)
            StartCoroutine(FetchLocationData());
    }

    private IEnumerator FetchLocationData()
    {
        // First, check if user has location service enabled
        if (!Input.location.isEnabledByUser)
            yield break;

        isCreating = true;

        // Start service before querying location
        Input.location.Start();

        statusText.text = "Fetching Location..";

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            statusText.text = "Location Timed out";
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            statusText.text = "Unable to determine device location";
            yield break;
        }
        else
        {
            //Create GeoTag
            GeoTag geoTag = new GeoTag();
            geoTag.latitude = Input.location.lastData.latitude;
            geoTag.longitude = Input.location.lastData.longitude;
            geoTag.name = geoTagName.text;
            geoTags.Add(geoTag);
            SaveTags();

            //Create tag display
            var obj = Instantiate(displayPrefab, Camera.main.transform.position + (Camera.main.transform.forward * 1), Quaternion.identity);
            obj.transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
            obj.GetComponent<GeoTagDisplay>().Initialize(geoTag);
        }

        createGeoTagCanvas.alpha = 0;
        createGeoTagCanvas.blocksRaycasts = false;
        Input.location.Stop();
        isCreating = false;
    }

    private void FetchTags()
    {
        JsonData jsonData = JsonMapper.ToObject(File.ReadAllText(savePath));
        for (int i = 0; i < jsonData.Count; i++)
        {
            GeoTag geoTag = new GeoTag();
            geoTag.name = (string)jsonData[i]["name"];
            geoTag.latitude = (float)jsonData[i]["latitude"];
            geoTag.longitude = (float)jsonData[i]["longitude"];
            geoTags.Add(geoTag);
        }
    }

    private void SaveTags()
    {
        string jsonData = JsonMapper.ToJson(geoTags);
        File.WriteAllText(savePath, jsonData);
    }


    private float GetDistance(float lat1, float lon1, float lat2, float lon2, char unit)
    {
        if ((Math.Abs(lat1 - lat2) < Mathf.Epsilon) && (Math.Abs(lon1 - lon2) < Mathf.Epsilon))
        {
            return 0;
        }
        else
        {
            double theta = lon1 - lon2;
            double dist = Math.Sin((lat1 * Mathf.Deg2Rad)) * Math.Sin((lat2 * Mathf.Deg2Rad)) + Math.Cos((lat1 * Mathf.Deg2Rad)) * Math.Cos((lat2 * Mathf.Deg2Rad)) * Math.Cos((theta * Mathf.Deg2Rad));
            dist = Math.Acos(dist);
            dist = dist * Mathf.Deg2Rad;
            dist = dist * 60 * 1.1515;
            if (unit == 'K')
            {
                dist = dist * 1.609344;
            }
            else if (unit == 'N')
            {
                dist = dist * 0.8684;
            }
            return (float)dist;
        }
    }
}
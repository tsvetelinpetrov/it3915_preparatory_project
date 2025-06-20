using System.Collections.Generic;
using System.IO;
using System.Text;
using Dummiesman;
using UnityEngine;

public class GreenhouseManager : MonoBehaviour
{
    public Material plantMaterial;
    public GameObject plantHolder;
    public GameObject dummyPlant;
    public GameObject airflowVisualizer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Each x seconds, get the current data
        InvokeRepeating("InitializeAllCurrentData", 0, SettingsManager.RefreshRate);

        // Get the plant model
        PlantModelOrigin modelOrigin = SettingsManager.PlantModelOrigin;
        if (modelOrigin == PlantModelOrigin.DataSource)
        {
            dummyPlant.SetActive(false);
            GetPlantModel();
        }
        else if (modelOrigin == PlantModelOrigin.DummyPlant)
        {
            // Load the dummy plant model from the scene
            dummyPlant.SetActive(true);
        }

        GetCurrentAirflow();
    }

    private void InitializeAllCurrentData()
    {
        // Check if API calls are blocked to prevent data mismatch
        if (GlobalParameters.Instance.BlockAPICalls)
            return;

        IDataSource dataSource = DataSourceFactory.GetDataSource();

        EventCenter.Controls.ChangeRefreshingStatus(true);
        EventCenter.Measurements.ChangeRefreshingStatus(true);

        // Call GetAllCurrent
        dataSource.GetAllCurrent(
            (current) =>
            {
                // Check if API calls are blocked to prevent data mismatch
                if (GlobalParameters.Instance.BlockAPICalls)
                    return;

                ProcessControlsData(current.Controls);
                ProcessMeasurementsData(current.Measurements);
                ProcessDisruptiveData(current.Disruptive);
                EventCenter.Controls.ChangeRefreshingStatus(false);
                EventCenter.Measurements.ChangeRefreshingStatus(false);
            },
            (error) =>
            {
                Debug.LogError($"Failed to get current data: {error}");
                EventCenter.Controls.ChangeRefreshingStatus(false);
                EventCenter.Measurements.ChangeRefreshingStatus(false);
            }
        );
    }

    private void ProcessControlsData(Controls controls)
    {
        EventCenter.Controls.ChangeControls(controls);

        if (controls.LightOn && !GlobalParameters.Instance.LightsStatus)
        {
            EventCenter.Controls.TurnOnLights();
        }
        else if (!controls.LightOn && GlobalParameters.Instance.LightsStatus)
        {
            EventCenter.Controls.TurnOffLights();
        }

        if (
            controls.FanOn
            && !GlobalParameters.Instance.UpperFanStatus
            && !GlobalParameters.Instance.LowerFanStatus
        )
        {
            EventCenter.Controls.TurnOnUpperFan();
            EventCenter.Controls.TurnOnLowerFan();
        }
        else if (
            !controls.FanOn
            && (
                GlobalParameters.Instance.UpperFanStatus || GlobalParameters.Instance.LowerFanStatus
            )
        )
        {
            EventCenter.Controls.TurnOffUpperFan();
            EventCenter.Controls.TurnOffLowerFan();
        }

        if (controls.ValveOpen && !GlobalParameters.Instance.ValveStatus)
        {
            EventCenter.Controls.OpenValve();
        }
        else if (!controls.ValveOpen && GlobalParameters.Instance.ValveStatus)
        {
            EventCenter.Controls.CloseValve();
        }
    }

    private void ProcessMeasurementsData(Measurement measurements)
    {
        EventCenter.Measurements.ChangeMeasurements(measurements);
    }

    private void ProcessDisruptiveData(List<Disruptive> disruptive)
    {
        // TODO: Implement the logic to process the disruptive data
    }

    private void GetPlantModel()
    {
        IDataSource dataSource = DataSourceFactory.GetDataSource();
        dataSource.GetPlantObjModel(
            (model) =>
            {
                var textStream = new MemoryStream(Encoding.UTF8.GetBytes(model));
                var loadedObj = new OBJLoader().Load(textStream);

                // Apply the material to the plant
                foreach (var meshRenderer in loadedObj.GetComponentsInChildren<MeshRenderer>())
                {
                    meshRenderer.material = plantMaterial;
                }

                // --- Ensure the model is centered ---
                // Calculate the combined bounds of all renderers
                var renderers = loadedObj.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds bounds = renderers[0].bounds;
                    foreach (var r in renderers)
                        bounds.Encapsulate(r.bounds);

                    // Calculate offset so base is at y=0 and center is at (0,0,0) horizontally
                    Vector3 offset = bounds.center;
                    offset.y = bounds.min.y;

                    // Move all children so the base is at y=0
                    foreach (Transform child in loadedObj.transform)
                    {
                        child.localPosition -= offset;
                    }
                }

                // Set the plant position in plantHolder
                loadedObj.transform.SetParent(plantHolder.transform, false);
                loadedObj.transform.localPosition = Vector3.zero;
                loadedObj.transform.localRotation = Quaternion.identity;
                loadedObj.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            },
            SettingsManager.PlantModelQuality == PlantQuality.High ? true : false,
            (error) =>
            {
                Debug.LogError($"Failed to get plant model: {error}");
            }
        );
    }

    private void GetCurrentAirflow()
    {
        IDataSource dataSource = DataSourceFactory.GetDataSource();
        dataSource.GetCurrentAirflow(
            (airflow) =>
            {
                EventCenter.Airflow.NewAirflowData(airflow);
            },
            (error) =>
            {
                Debug.LogError($"Failed to get airflow data: {error}");
            }
        );
    }

    public void ToggleOnAirflowVisualization()
    {
        airflowVisualizer.SetActive(true);
    }

    public void ToggleOffAirflowVisualization()
    {
        airflowVisualizer.SetActive(false);
    }
}

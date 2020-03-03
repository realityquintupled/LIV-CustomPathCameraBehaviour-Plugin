using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class CustomPathPlugin : IPluginCameraBehaviour {
    public string ID => "CustomPathPlugin";
    public string name => "Custom Path";
    public string author => "Reality Quintupled";
    public string version => "0.4.1";

    public static CustomPathPlugin instance;

    public IPluginSettings settings => new EmptySettings();

    public event EventHandler ApplySettings;

    private PluginCameraHelper helper;

    private string[] paths;
    private int pathIndex;
    private PathRenderer pathRenderer;

    private TextMeshPro previous;
    private TextMeshPro next;
    private TextMeshPro pathNameDisplay;
    private GameObject previousButton;
    private GameObject nextButton;

    private float c;
    private float speed;
    private CameraTarget mode;
    private GameObject fixedPoint;
    private Transform target;
    private Spline spline;

    private Transform wrapper;

    public CustomPathPlugin() { }
    
    public void OnActivate(PluginCameraHelper helper) {
        if (instance == null)
            instance = this;

        this.helper = helper;

        wrapper = new GameObject("CustomCameraPathPluginWrapper").transform;

        GameObject nameObject = new GameObject("PathName");
        nameObject.transform.position = new Vector3(0, 1.5f, 1);
        nameObject.transform.localScale = new Vector3(.03f, .03f, .03f);
        nameObject.transform.parent = wrapper;
        pathNameDisplay = nameObject.AddComponent<TextMeshPro>();
        pathNameDisplay.fontSize = 32;
        pathNameDisplay.alignment = TextAlignmentOptions.Center;
        pathNameDisplay.text = "No paths found!";

        string pathDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LIV", "CustomCameraPaths");

        if (!Directory.Exists(pathDirectory))
            return;

        paths = Directory.GetFiles(pathDirectory, "*.path");

        if (paths.Length == 0)
            return;

        GameObject previousObject = new GameObject("PreviousText");
        previousObject.transform.position = new Vector3(-.25f, 1.25f, .75f);
        previousObject.transform.localScale = new Vector3(.03f, .03f, .03f);
        previousObject.transform.parent = wrapper;
        previous = previousObject.AddComponent<TextMeshPro>();
        previous.fontSize = 28;
        previous.alignment = TextAlignmentOptions.Center;
        previous.text = "<<";

        previousButton = new GameObject("PreviousButton");
        previousButton.AddComponent<BoxCollider>();
        previousButton.transform.localScale = new Vector3(.05f, .05f, .01f);
        previousButton.transform.position = previousObject.transform.position;
        previousButton.transform.parent = wrapper;
        InputObject previousInput = previousButton.AddComponent<InputObject>();
        previousInput.direction = -1;
        previousInput.textMesh = previous;

        GameObject nextObject = new GameObject("NextText");
        nextObject.transform.position = new Vector3(.25f, 1.25f, .75f);
        nextObject.transform.localScale = new Vector3(.03f, .03f, .03f);
        nextObject.transform.parent = wrapper;
        next = nextObject.AddComponent<TextMeshPro>();
        next.fontSize = 28;
        next.alignment = TextAlignmentOptions.Center;
        next.text = ">>";

        nextButton = new GameObject("NextButton");
        nextButton.AddComponent<BoxCollider>();
        nextButton.transform.localScale = new Vector3(.05f, .05f, .01f);
        nextButton.transform.position = nextObject.transform.position;
        nextButton.transform.parent = wrapper;
        InputObject nextInput = nextButton.AddComponent<InputObject>();
        nextInput.direction = 1;
        nextInput.textMesh = next;

        if(helper.playerLeftHand.GetComponent<SphereCollider>() == null) {
            SphereCollider leftHandCollider = helper.playerLeftHand.gameObject.AddComponent<SphereCollider>();
            leftHandCollider.radius = .03f;
            leftHandCollider.isTrigger = true;
            SphereCollider rightHandCollider = helper.playerRightHand.gameObject.AddComponent<SphereCollider>();
            rightHandCollider.radius = .03f;
            rightHandCollider.isTrigger = true;
        }

        GameObject pathObj = new GameObject("CameraPath");
        pathObj.transform.position = Vector3.zero;
        pathObj.transform.parent = wrapper;
        pathRenderer = pathObj.AddComponent<PathRenderer>();

        fixedPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fixedPoint.transform.localScale = new Vector3(.05f, .05f, .05f);
        fixedPoint.GetComponent<Renderer>().material.color = new Color(34, 139, 34);

        ImportCameraPath(paths[0]);
    }

    public void OnSettingsDeserialized() {}
    
    public void OnFixedUpdate() {}
    
    public void OnUpdate() {}

    public void ChangePath(int direction) {
        pathIndex = (pathIndex + direction + paths.Length) % paths.Length;
        ImportCameraPath(paths[pathIndex]);
    }

    public void OnLateUpdate() {
        if(target == null) {
            pathNameDisplay.text = "Error: Camera target missing!";
            return;
        }
        Vector3 newPos = spline.PointAtTime(Time.time * speed, c);
        helper.UpdateCameraPose(newPos, Quaternion.LookRotation(target.position - newPos, Vector3.up));
    }

    public void ImportCameraPath(string filePath) {
        string pathName = new FileInfo(filePath).Name.Split('.')[0];
        pathNameDisplay.text = pathName;

        List<Vector3> points = new List<Vector3>();
        using (StreamReader reader = new StreamReader(filePath)) {
            while (!reader.EndOfStream) {
                string line = reader.ReadLine();
                if (line.StartsWith(";")) {
                    string[] pair = line.TrimStart(';').Split(':');
                    switch (pair[0]) {
                        case "c":
                            c = float.Parse(pair[1]);
                            break;
                        case "speed":
                            speed = float.Parse(pair[1]);
                            break;
                        case "mode":
                            mode = (CameraTarget)Enum.Parse(typeof(CameraTarget), pair[1]);
                            break;
                        case "point":
                            float[] coords = pair[1].Split(',').ToList().ConvertAll<float>(c => float.Parse(c)).ToArray();
                            fixedPoint.transform.position = new Vector3(coords[0], coords[1], coords[2]);
                            break;

                    }
                } else {
                    float[] coords = line.Split(',').ToList().ConvertAll<float>(c => float.Parse(c)).ToArray();
                    points.Add(new Vector3(coords[0], coords[1], coords[2]));
                }
            }
        }

        spline = new Spline(points);
        pathRenderer.AddPoints(points);
        pathRenderer.RenderSpline(spline, c);

        switch (mode) {
            case CameraTarget.Head:
                target = helper.playerHead;
                fixedPoint.SetActive(false);
                break;
            case CameraTarget.Fixed_Point:
                target = fixedPoint.transform;
                fixedPoint.SetActive(true);
                break;
        }
    }
    
    public void OnDeactivate() {
        if (wrapper != null)
            Object.Destroy(wrapper.gameObject);
    }
    
    public void OnDestroy() {}
}
// There be no settings soooo...
public class EmptySettings : IPluginSettings { }

public enum CameraTarget {
    Head = 0,
    Fixed_Point = 1
}
using System.Collections;
using TMPro;
using UnityEngine;

class InputObject : MonoBehaviour{
    public int direction;
    public TextMeshPro textMesh;

    private bool triggered = false;
    private string text;

    private void Start() {
        gameObject.AddComponent<Rigidbody>().isKinematic = true;
        text = textMesh.text;
    }

    private void OnTriggerEnter(Collider other) {
        if(!triggered) {
            triggered = true;
            CustomPathPlugin.instance.ChangePath(direction);
            textMesh.text = "<color=red>" + text + "</color>";
        }
    }

    private void OnTriggerExit(Collider other) {
        if (triggered)
            StartCoroutine(Debounce());
    }

    private IEnumerator Debounce() {
        yield return new WaitForSeconds(.25f);
        triggered = false;
        textMesh.text = text;
    }
}

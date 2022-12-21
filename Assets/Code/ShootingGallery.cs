using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShootingGallery : MonoBehaviour
{
    public enum GaleryStates
    {
        Innactive,
        Playing,
        Finished
    }

    public int points = 0;
    public GameObject pointsText;

    public GaleryStates states;

    public float time;
    public GameObject textZone;
    public GameObject timeTextZone;

    public GameObject rockToHell;

    public List<GameObject> objectList;
    public List<GameObject> completeList;
    public List<GameObject> deadObjectList;

    public List<Animator> dummiesAnimations;

    private void Start()
    {
        pointsText.GetComponent<TextMeshProUGUI>().text = points.ToString();
        textZone.SetActive(false);

        foreach (GameObject collider in objectList)
            collider.GetComponent<DummyHP>().m_Life = 10;

        time = 20;

        foreach (GameObject collider in objectList)
            completeList.Add(collider);

        SetInnactiveState();
    }

    void Update()
    {
        switch (states)
        {
            case GaleryStates.Playing:
                PlayingState();
                break;
            default:
                break;
        }
    }

    public void SetInnactiveState()
    {
        DisableAllMeshes();
        states = GaleryStates.Innactive;
    }

    public void SetPlayingState()
    {
        EnableAllMeshes();
        states = GaleryStates.Playing;
    }

    public void PlayingState()
    {
        if (deadObjectList.Count == completeList.Count)
        {
            SetFinishedState();
            return;
        }

        time -= Time.deltaTime;
        timeTextZone.GetComponent<TextMeshProUGUI>().text = "Time " + (int)time;

        
        if (time <= 0)
        {
            SetFailedState();
            return;
        }

        foreach (GameObject collider in objectList)
        {
            
            if (collider.GetComponent<DummyHP>().HasDied())
            {
                deadObjectList.Add(collider);
                points = points + (int)Vector3.Distance(gameObject.transform.position, collider.transform.position);
                pointsText.GetComponent<TextMeshProUGUI>().text = points.ToString();
                collider.GetComponent<MeshCollider>().enabled = false;
                objectList.Remove(collider);
            }
        }
    }

    private void SetFailedState()
    {
        timeTextZone.GetComponent<TextMeshProUGUI>().text = "Has Perdido";
        RestartGame();
    }

    public void SetFinishedState()
    {
        states = GaleryStates.Finished;
        timeTextZone.GetComponent<TextMeshProUGUI>().text = "Has Ganado!";
        rockToHell.GetComponent<PlayOnceAnimation>().PlayOnce();
        StartCoroutine(playOnceAnimation());

    }

    public IEnumerator playOnceAnimation()
    {
        yield return new WaitForSeconds(rockToHell.GetComponent<PlayOnceAnimation>().GetClipDuration());
        rockToHell.SetActive(false);
    }

    private void EnableAllMeshes()
    {
        foreach (GameObject collider in objectList)
            collider.GetComponent<MeshCollider>().enabled = true;
    }

    private void DisableAllMeshes()
    {
        foreach (GameObject collider in objectList)
            collider.GetComponent<MeshCollider>().enabled = false;
    }

    public void RestartGame()
    {
        if(deadObjectList.Count > 0)
            deadObjectList.Clear();

        if(objectList.Count > 0)
            objectList.Clear();
        foreach (GameObject collider in completeList) 
            objectList.Add(collider);

        foreach (GameObject collider in objectList)
            collider.GetComponent<DummyHP>().SetHP(10);

        time = 10;
        points = 0;
        pointsText.GetComponent<TextMeshProUGUI>().text = points.ToString();

        timeTextZone.GetComponent<TextMeshProUGUI>().text = "Pulsa E para iniciar"; 

        SetInnactiveState();
    }

    private void OnTriggerStay(Collider other)
    {
        textZone.SetActive(true);
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (states == GaleryStates.Finished)
                RestartGame();
            SetPlayingState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        textZone.SetActive(false);

        if (states != GaleryStates.Finished)
            RestartGame();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowroomManager : MonoBehaviour
{
    [SerializeField] List<GameObject> situations;
    [SerializeField] Transform spawnPoints;
    [SerializeField] Transform spawnPointEnter;
    [SerializeField] Transform spawnPointEscape;

    int currentSituationIndex;
    Coroutine MoveCurrentSituation;
    ShowroomSituation currentSituation;

    // Start is called before the first frame update
    void Start()
    {
        currentSituationIndex = 0;
        currentSituation = Instantiate(situations[currentSituationIndex], spawnPointEnter.transform.position, spawnPointEnter.transform.rotation).GetComponent<ShowroomSituation>();
        MoveCurrentSituation = StartCoroutine(MoveTo(currentSituation.gameObject, spawnPoints.transform.position, 1.0f, false));
    }

    // Update is called once per frame
    void Update()
    {
        if(MoveCurrentSituation != null)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(currentSituation.HasStarted)
            {
                Restart();
            }
            currentSituation.StartSimulation();
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            GoToNextSituation();
        }
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GoToPreviousSituation();
        }
    }

    private void Restart()
    {
        Destroy(currentSituation.gameObject);
        currentSituation = Instantiate(situations[currentSituationIndex], spawnPoints.transform.position, spawnPoints.transform.rotation).GetComponent<ShowroomSituation>();
    }

    private void GoToNextSituation()
    {
        currentSituationIndex++;
        if(currentSituationIndex >= situations.Count)
        {
            currentSituationIndex = 0;
        }

        StartCoroutine(MoveTo(currentSituation.gameObject, spawnPointEscape.transform.position, 1.0f, true));
        currentSituation = Instantiate(situations[currentSituationIndex], spawnPointEnter.transform.position, spawnPointEnter.transform.rotation).GetComponent<ShowroomSituation>();
        MoveCurrentSituation = StartCoroutine(MoveTo(currentSituation.gameObject, spawnPoints.transform.position, 1.0f, false));
    }

    private void GoToPreviousSituation()
    {
        currentSituationIndex--;
        if (currentSituationIndex < 0)
        {
            currentSituationIndex = situations.Count - 1;
        }

        StartCoroutine(MoveTo(currentSituation.gameObject, spawnPointEnter.transform.position, 1.0f, true));
        currentSituation = Instantiate(situations[currentSituationIndex], spawnPointEscape.transform.position, spawnPointEscape.transform.rotation).GetComponent<ShowroomSituation>();
        MoveCurrentSituation = StartCoroutine(MoveTo(currentSituation.gameObject, spawnPoints.transform.position, 1.0f, false));
    }

    IEnumerator MoveTo(GameObject GO, Vector3 target, float time, bool destroyAtEnd)
    {
        Vector3 startPos = GO.transform.position;
        for(float t = 0; t < time; t += Time.deltaTime)
        {
            GO.transform.position = Vector3.Lerp(startPos, target, t / time);
            yield return null;  
        }

        GO.transform.position = target;
        if(destroyAtEnd)
        {
            Destroy(GO);
        }

        MoveCurrentSituation = null;
    }
}

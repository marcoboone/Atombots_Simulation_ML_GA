using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class SwarmGenerator : MonoBehaviour
{
    List<GameObject> automata;
    [SerializeField] GameObject automatonPreFab;
    [SerializeField] private int numberOfAutomata;
   // [SerializeField] Text generationText;
    private int score;
  

    // Start is called before the first frame update
    void Start()
    {

        Storage.generation++;
        //generationText.text = "GEN: " + Storage.generation;

        automata = new List<GameObject>();
        for (int i = 0; i < numberOfAutomata; i++)
        {
            // Create random automaton in general area
            GameObject automaton = Instantiate(automatonPreFab, transform.position + new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), 0), transform.rotation);
            automaton.GetComponent<Automaton>().BuildAutomaton();
            if (Storage.bestBrainWeightMatrix != null && i % 3 != 0) // i %10 == 0
            {
                automaton.GetComponent<Automaton>().network.SetMatrices(Storage.MutatedWeights(), Storage.MutatedBiases());
            }
            automata.Add(automaton);
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckGameStatus();
    }

    public void CheckGameStatus()
    {
        int numberOfAutomataAlive = automata.Count;
        for (int i = 0; i < automata.Count; i++)
        {
            if (!automata[i].GetComponent<Automaton>().isDead)
                return;
        }
        UpdateIndividualStorage();
        ReloadGeneration();
    }

    public void ReloadGeneration()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void UpdateIndividualStorage()
    {
        Storage.UpdateBestBrain(automata);
    }

 
}

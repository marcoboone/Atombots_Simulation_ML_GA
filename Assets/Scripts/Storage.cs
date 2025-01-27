using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using System;

public static class Storage
{
    public static List<double[,]> bestBrainWeightMatrix;
    public static List<double[]> bestBrainBiasMatrix;
    public static float bestAvgFitness = 2f;
    public static int generation;

    public static void UpdateBestBrain(List<GameObject> automata)
    {
        List<GameObject> bestAutomata = FindBestAutomata(2, automata);
        float genAvgFitness = AverageFitness(bestAutomata);
        if (genAvgFitness > bestAvgFitness)
        {
            Debug.Log("genAvgFitness" + genAvgFitness);
            bestAvgFitness = genAvgFitness;
            AverageAutomataMatrices(bestAutomata);
        }
    }

    public static float AverageFitness(List<GameObject> automata)
    {
        float avgFitness = 0;
        for (int i = 0; i < automata.Count; i++)
        {
            avgFitness += automata[i].GetComponent<Automaton>().timeAlive;
        }
        return avgFitness / automata.Count;
    }

    public static List<GameObject> FindBestAutomata(int automataCount, List<GameObject> automata)
    {
        List<GameObject> bestAutomata = new List<GameObject>();

        for (int i = 0; i < automata.Count; i++)
        {
            // Finds the best automata in a list
            GameObject bestAutomaton = FindBestAutomaton(automata);

            bestAutomata.Add(bestAutomaton);
            automata.Remove(bestAutomaton);
            i--;

            // We don't want every automaton at the beginning so this will protect from the network getting messed up early
            if (bestAutomata.Count == automataCount || automata.Count == 0)
                break;
        }

        return bestAutomata;
    }

    public static GameObject FindBestAutomaton(List<GameObject> automata)
    {
        float genBestTime = 0;
        GameObject bestAutomaton = automata[0];
        for (int i = 0; i < automata.Count; i++)
        {
            if (automata[i].GetComponent<Automaton>().timeAlive > genBestTime)
            {
                genBestTime = automata[i].GetComponent<Automaton>().timeAlive;
                bestAutomaton = automata[i];
            }
        }

        return bestAutomaton;
    }

    public static void AverageAutomataMatrices(List<GameObject> automata)
    {
        // Get matrices from the first automaton
        bestBrainBiasMatrix = new List<double[]>();
        bestBrainWeightMatrix = new List<double[,]>();

        int layers = automata[0].GetComponent<Automaton>().network.layers.Length;

        for (int j = 0; j < layers; j++)
        {
            // Creates a matrix of weights that will be averaged and used as the weights for the jth layer
            List<double[,]> layerWeights = new List<double[,]>();
            List<double[]> layerBiases = new List<double[]>();
            for (int i = 0; i < automata.Count; i++)
            {
                // Adds the weights from every automaton's layer j to a list
                layerWeights.Add(automata[i].GetComponent<Automaton>().network.layers[j].weights);
                layerBiases.Add(automata[i].GetComponent<Automaton>().network.layers[j].biases);
            }
            bestBrainWeightMatrix.Add(Average2DArrays(layerWeights));
            bestBrainBiasMatrix.Add(Average1DArrays(layerBiases));
        }
    }

    public static double[,] Average2DArrays(List<double[,]> arrays)
    {
        int numRows = arrays[0].GetLength(0);
        int numCols = arrays[0].GetLength(1);
        double[,] result = new double[numRows, numCols];

        // Loop through each element and compute the average
        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                double sum = 0;
                for (int k = 0; k < arrays.Count; k++)
                {
                    sum += arrays[k][i, j];
                }
                result[i, j] = sum / arrays.Count;
            }
        }

        return result;
    }

    public static double[] Average1DArrays(List<double[]> arrays)
    {
        int arrayLength = arrays[0].Length;
        double[] avgArray = new double[arrayLength];

        for (int i = 0; i < arrays.Count; i++)
        {
            for (int j = 0; j < arrayLength; j++)
            {
                avgArray[j] += arrays[i][j];
            }
        }

        for (int i = 0; i < arrayLength; i++)
        {
            avgArray[i] /= arrays.Count;
        }

        return avgArray;
    }

    public static Tuple<List<double[,]>, List<double[]>> GetAutomataMatrices(GameObject bestAutomaton)
    {
        List<double[,]> weightMatrix = new List<double[,]>();
        List<double[]> biasMatrix = new List<double[]>();
        Network network = bestAutomaton.GetComponent<Automaton>().network;
        for (int i = 0; i < network.layers.Length; i++)
        {
            weightMatrix.Add(network.layers[i].weights);
            biasMatrix.Add(network.layers[i].biases);
        }

        return new Tuple<List<double[,]>, List<double[]>>(weightMatrix, biasMatrix);
    }

    public static List<double[,]> MutatedWeights()
    {
        List<double[,]> weights = bestBrainWeightMatrix;

        for (int index = 0; index < weights.Count; index++)
            for (int row = 0; row < weights[index].GetLength(0); row++)
                for (int col = 0; col < weights[index].GetLength(1); col++)
                    weights[index][row, col] += UnityEngine.Random.Range(-.025f, .025f);

        return weights;
    }

    public static List<double[]> MutatedBiases()
    {
        List<double[]> biases = bestBrainBiasMatrix;

        for (int index = 0; index < biases.Count; index++)
            for (int row = 0; row < biases[index].GetLength(0); row++)
                biases[index][row] += UnityEngine.Random.Range(-.025f, .025f);

        return biases;
    }

    /*
    public static void AverageBestAutomata(List<GameObject> individuals)
    {
        bestBrainWeightMatrix = new List<double[,]>();
        bestBrainBiasMatrix = new List<double[]>();
        InitializeMatrices(individuals);    
        Debug.Log(individuals.Count); 

        for (int layer = 0; layer < individuals[0].GetComponent<Automata>().network.layers.Length; layer++)
        {
            for (int i = 0; i < individuals[0].GetComponent<Automata>().network.layers[layer].numNodesExport; i++)
            {
                //might need a for loop here later
                bestBrainBiasMatrix[layer][i] = (individuals[0].GetComponent<Automata>().network.layers[layer].biases[i] + individuals[1].GetComponent<Automata>().network.layers[layer].biases[i])/2;
                for (int j = 0; j < individuals[0].GetComponent<Automata>().network.layers[layer].numNodesImport; j++)
                {
                    bestBrainWeightMatrix[layer][j,i] = (individuals[0].GetComponent<Automata>().network.layers[layer].weights[j,i] + individuals[1].GetComponent<Automata>().network.layers[layer].weights[j,i]) / 2;
                }
            }
        }
    }

    public static void InitializeMatrices(List<GameObject> individuals)
    {
        int numLayers = individuals[0].GetComponent<Automata>().network.layers.Length;

        for(int i = 0; i< numLayers; i++)
        {
            int export = individuals[0].GetComponent<Automata>().network.layers[i].numNodesExport;
            int import = individuals[0].GetComponent<Automata>().network.layers[i].numNodesImport;
            bestBrainWeightMatrix.Add(new double[import, export]);
            bestBrainBiasMatrix.Add(new double[export]); 
        }
    }

    public static void SetMatricesToAutomaton(GameObject bestAutomaton)
    {
        bestBrainWeightMatrix = new List<double[,]>();
        bestBrainBiasMatrix = new List<double[]>(); 
        Network network = bestAutomaton.GetComponent<Automata>().network;
        for(int i = 0; i<network.layers.Length; i++)
        {
            bestBrainWeightMatrix.Add(network.layers[i].weights);
            bestBrainBiasMatrix.Add(network.layers[i].biases);
        }
    }
    */
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer
{
    public int numNodesImport, numNodesExport;
    public double[,] weights;
    public double[] biases;
    public double[] weightedInputs;
    public double[] activations;
    public double[] inputs;

    public Layer(int numNodesImport, int numNodesExport)
    {
        weightedInputs = new double[numNodesExport];
        activations = new double[numNodesExport];
        inputs = new double[numNodesImport];
        this.numNodesImport = numNodesImport;
        this.numNodesExport = numNodesExport;

        biases = new double[numNodesExport];

        weights = new double[numNodesImport, numNodesExport];
        RandomizeWeights();
        RandomizeBiases();
    }
    //calcuates the derivatives for the nodes in the output layer 
    //this is the derviviest taht are needed before finding dCost with respect to weight

    //calculates the derivates for the nodes in "h" layer
    //this is the derviviest taht are needed before finding dCost with respect to weight


    public double[] CalculateOutputs(double[] inputs)
    {
        this.inputs = inputs;
        for (int i = 0; i < numNodesExport; i++)
        {
            weightedInputs[i] = biases[i];
            for (int j = 0; j < numNodesImport; j++)
            {
                weightedInputs[i] += inputs[j] * weights[j, i];
            }
            activations[i] = ActivationFunction(weightedInputs[i]);
        }
        return activations;
    }

    //recives weighted Input and pumps it thorugh the activation function
    double ActivationFunction(double weightedInput)
    {
        return 1 / (1 + Mathf.Exp((float)-weightedInput));
    }


    //Initialize the weights randomly
    public void RandomizeWeights()
    {
        for (int i = 0; i < numNodesImport; i++)
            for (int j = 0; j < numNodesExport; j++)
            {
                weights[i, j] = (double)Random.Range(-5f, 5f);

            }
    }
    public void RandomizeBiases()
    {
        for (int i = 0; i < numNodesExport; i++)
        {
            biases[i] = (double)Random.Range(-5f, 5f);
        }
    }

}


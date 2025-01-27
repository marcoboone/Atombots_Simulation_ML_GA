using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

public class Network
{
    public Layer[] layers;


    //Create a neural network 
    public Network(int[] layerSizes)
    {

        layers = new Layer[layerSizes.Length - 1];

        for (int i = 0; i < layers.Length; i++)
        {
            //layerSizes[i] is the amoumt of values imported = num nodes in prev layer
            //layerSizes[i+1] is the amount of values exported = num nodes in that layer
            layers[i] = new Layer(layerSizes[i], layerSizes[i + 1]);
        }
    }

    //recevies input neuron activations and returns output neuron activations
    public double[] CalculateOutputs(double[] inputs)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            //Sebastian is a genius here
            //calculated outputs become new new inputs for next layer
            inputs = layers[i].CalculateOutputs(inputs);

        }
        return inputs;

    }
    public void SetMatrices(List<double[,]> weightMatrix, List<double[]> biasMatrix)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].weights = weightMatrix[i];
            layers[i].biases = biasMatrix[i];
        }
    }

}

